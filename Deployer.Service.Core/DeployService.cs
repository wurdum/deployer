using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Authentication;
using System.ServiceModel;
using Deployer.Service.Contract;
using Deployer.Service.Contract.Contexts;
using Deployer.Service.Contract.Exceptions;
using Deployer.Service.Contract.Helpers;
using Deployer.Service.Contract.Settings;
using Deployer.Service.Core.Helpers;
using Microsoft.SqlServer.Management.Common;
using Exception = System.Exception;

namespace Deployer.Service.Core
{
    [InterceptAllExceptions]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Multiple, AddressFilterMode = AddressFilterMode.Any)]
    public class DeployService : IDeployService, IDisposable
    {
        private readonly IDeployServiceFactory _factory;
        private readonly IServiceLogger _logger;
        public readonly string _settingsFile;

        private string _user;
        private string _sessionKey;
        private string _updatedForlder;
        private bool _accessIsGranted;

        public DeployService(IDeployServiceFactory factory) {
            _factory = factory;
            _logger = _factory.CreateLoggerObj();
            _settingsFile = @"DepSettings.xml";
            _logger.Info("Service instance is created");
            if (OperationContext.Current != null)
                OperationContext.Current.Channel.Closing += ClearLocks;
        }

        protected void ClearLocks(object sender, EventArgs e) {
            FoldersLocks.RemoveForSession(_sessionKey);
            _logger.Info(string.Format("Lock for folder '{0}' is released", _updatedForlder));
        }

        #region Implementation of IDeployService

        [OperationBehavior]
        public bool OpenSession(string folderName) {
            _sessionKey = OperationContext.Current != null ? OperationContext.Current.SessionId : Guid.NewGuid().ToString();
            _user = ServiceSecurityContext.Current != null ? ServiceSecurityContext.Current.PrimaryIdentity.Name : _sessionKey;
            _updatedForlder = folderName;

            _logger.Info(string.Format("Try to open access from session '{0}' to folder '{1}'", _sessionKey, _updatedForlder));
            _accessIsGranted = FoldersLocks.Add(folderName, _sessionKey);
            _logger.Info(string.Format("Folder '{0}' is locked", _updatedForlder));

            _logger.Info(string.Format("Access is {0}", _accessIsGranted ? "granted" : "denied"));
            return _accessIsGranted;
        }

        [OperationBehavior]
        public void Install(DeployContext context, byte[] files) {
            if (!_accessIsGranted)
                throw new AuthenticationException("Session should be terminated because access wasn't granted.");
            if (files.Length == 0)
                throw new ArgumentNullException("files");
            var callback = _factory.CreateCallbackObj();
            try {
                _accessIsGranted = false;
                var appSettings = _factory.CreateSettingsObj(_settingsFile, context, DeployMode.Install, _factory.CreatePackageManager());
                if (!GetDirectoryName(appSettings.SurveyPath).Equals(_updatedForlder, StringComparison.InvariantCultureIgnoreCase))
                    throw new InvalidArgumentException("Current session allows deploy only to '" + _updatedForlder + "' folder.");

                _logger.Info(string.Format("Starting innstall process for folder '{0}'", _updatedForlder));
                Deploy(appSettings, callback, files,
                    _factory.CreateValidatorsList(appSettings),
                    _factory.CreateDeployersList(appSettings),
                    ist => ist.InstallRollback());
            } catch (Exception ex) {
                callback.OnFault(UnhandledException.CreateFromEx(ex));
                throw;
            } finally {
                callback.OnClose();
                Dispose();
            }
        }

        [OperationBehavior]
        public void Update(DeployContext context, DeployMode mode, byte[] files) {
            if (!_accessIsGranted)
                throw new AuthenticationException("Session should be terminated because access wasn't granted.");
            if (files.Length == 0)
                throw new ArgumentNullException("files");
            var callback = _factory.CreateCallbackObj();
            try{
                _accessIsGranted = false;
                var appSettings = _factory.CreateSettingsObj(_settingsFile, context, mode, _factory.CreatePackageManager());
                if (!GetDirectoryName(appSettings.SurveyPath).Equals(_updatedForlder, StringComparison.InvariantCultureIgnoreCase))
                    throw new InvalidArgumentException("Current session allows deploy only to '" + _updatedForlder + "' folder.");

                _logger.Info(string.Format("Starting update process for folder '{0}'", _updatedForlder));
                Deploy(appSettings, callback, files, 
                    _factory.CreateValidatorsList(appSettings), 
                    _factory.CreateDeployersList(appSettings), 
                    ist => ist.UpdateRollback());
            } catch (Exception ex) {
                callback.OnFault(UnhandledException.CreateFromEx(ex));
                throw;
            } finally {
                callback.OnClose();
                Dispose();
            }
        }

        #endregion

        private void Deploy(IDeploySettings appSettings, IDeployEvents callback, byte[] files, 
            IEnumerable<IValidator> validators, IEnumerable<IDeployer> deployers, Action<IDeployer> rollbackAction) {
            using (var session = _factory.CreateSessionObj(appSettings, _sessionKey, _user)) {
                _logger.Info(string.Format("Session for '{0}' is opened", _sessionKey));

                var deploySteps = new List<IDeployer>();
                var failedValidators = new List<IValidator>();
                var exceptionHandler = _factory.CreateDeployExHandlerObj(callback, appSettings, _logger, session);
                try {
                    appSettings.PackageManager.BytesToZip(files, appSettings.SurveyPackagePath);
                    _logger.Info(string.Format("Zip files was unpacked to '{0}'", appSettings.SurveyPackagePath));

                    DeployExecutor.Validate(validators, failedValidators, callback, appSettings.DeployMode);
                    _logger.Info(string.Format("Validation process is complete. Found '{0}' error(s)", failedValidators.Count));
                    if (failedValidators.Any())
                        throw new ValidationException("Validation at " + String.Join(", ", failedValidators.Select(v => v.Name).ToArray()) + " failed.");

                    DeployExecutor.Deploy(deployers, deploySteps, callback, appSettings.DeployMode);
                    _logger.Info(string.Format("Deploy is finished for '{0}'", _sessionKey));

                    session.Commit();
                    _logger.Info(string.Format("Session for '{0}' is commited successfully", _sessionKey));
                } catch (ValidationException exception) {
                    exceptionHandler.HandleValidation(exception, callback);
                } catch (DeployException exception) {
                    exceptionHandler.HandleDeploy(exception, callback, deploySteps, rollbackAction);
                } catch (Exception exception) {
                    exceptionHandler.HandleUnknown(exception, callback);
                }
            }
        }

        private string GetDirectoryName(string surveyPath) {
            try {
                return surveyPath.Substring(surveyPath.LastIndexOf(Path.DirectorySeparatorChar) + 1); // cuts from C:\as\asda -> asda
            } catch {
                return string.Empty;
            }
        }

        #region Implementation of IDisposable

        public void Dispose() {
            _logger.Info("Service instance is released");
            FoldersLocks.Remove(_updatedForlder);
        }

        #endregion
    }
}