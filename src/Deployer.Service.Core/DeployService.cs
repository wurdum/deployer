using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.ServiceModel;
using Deployer.Service.Contract;
using Deployer.Service.Contract.Contexts;
using Deployer.Service.Contract.Exceptions;
using Deployer.Service.Contract.Factories;
using Deployer.Service.Contract.Helpers;
using Deployer.Service.Contract.Settings;
using Deployer.Service.Core.Helpers;
using Exception = System.Exception;

namespace Deployer.Service.Core
{
    [InterceptAllExceptions]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Multiple, AddressFilterMode = AddressFilterMode.Any)]
    public class DeployService : IDeployService, IDisposable
    {
        private readonly IDeployServiceFactory _dsFactory;
        private readonly IConfFactory _cFactory;
        private readonly IServiceLogger _logger;

        private byte[] _filesBuffer;

        private string _user;
        private string _sessionKey;
        private string _updatedForlder;
        private bool _accessIsGranted;

        public DeployService(IDeployServiceFactory dsFactory, IConfFactory cFactory) {
            _dsFactory = dsFactory;
            _cFactory = cFactory;
            _logger = _dsFactory.CreateLoggerObj();
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
            _sessionKey = NormalizeKey(OperationContext.Current != null ? OperationContext.Current.SessionId : Guid.NewGuid().ToString());
            _user = ServiceSecurityContext.Current != null ? ServiceSecurityContext.Current.PrimaryIdentity.Name : _sessionKey;
            _updatedForlder = folderName;

            _logger.Info(string.Format("Try to open access from session '{0}' to folder '{1}'", _sessionKey, _updatedForlder));
            _accessIsGranted = FoldersLocks.Add(folderName, _sessionKey);
            _logger.Info(string.Format("Folder '{0}' is locked", _updatedForlder));

            _logger.Info(string.Format("Access is {0}", _accessIsGranted ? "granted" : "denied"));
            return _accessIsGranted;
        }

        [OperationBehavior]
        public int SendFilesChunk(byte[] chunk) {
            if (_filesBuffer == null) {
                _filesBuffer = chunk;
            } else {
                var expandedBuffer = new byte[_filesBuffer.Length + chunk.Length];
                Array.Copy(_filesBuffer, expandedBuffer, _filesBuffer.Length);
                Array.Copy(chunk, 0, expandedBuffer, _filesBuffer.Length, chunk.Length);
                _filesBuffer = expandedBuffer;
            }

            return chunk.Length;
        }

        [OperationBehavior]
        public void Deploy(DeployContext context, byte[] filesHash) {
            if (!_accessIsGranted)
                throw new AuthenticationException("Session should be terminated because access wasn't granted");
            if (_filesBuffer == null || _filesBuffer.Length == 0)
                throw new InvalidOperationException("Files was not transfered before deployment");
            if (!FileHashIsEqual(filesHash, MD5.Create().ComputeHash(_filesBuffer)))
                throw new ArgumentException("File sending was failed because it's hash is wrong");
            var callback = _dsFactory.CreateCallbackObj();
            try {
                _accessIsGranted = false;
                var conf = _dsFactory.CreateConfObj(_sessionKey, context, _cFactory);
                if (!GetDirectoryName(conf.SurveyPath.Survey).Equals(_updatedForlder, StringComparison.InvariantCultureIgnoreCase))
                    throw new ArgumentException("Current session allows deploy only to '" + _updatedForlder + "' folder");

                _logger.Info(string.Format("Starting deploy process for folder '{0}'", _updatedForlder));
                
                RunDeployment(conf, callback, context, _filesBuffer);

                _logger.Info("Deploy completed successfully");
            } catch (Exception ex) {
                callback.OnFault(UnhandledException.CreateFromEx(ex));
                throw;
            } finally {
                callback.OnClose();
                Dispose();
            }
        }

        #endregion

        private void RunDeployment(IConf conf, IDeployEvents callback, DeployContext context, byte[] files) {
            using (var session = _dsFactory.CreateSessionObj(conf, _user)) {
                _logger.Info(string.Format("Session for '{0}' is opened", _sessionKey));

                var deploySteps = new List<IDeployer>();
                var failedValidators = new List<IValidator>();
                var exceptionHandler = _dsFactory.CreateDeployExHandlerObj(callback, conf, _logger, session);
                try {
                    conf.PackageManager.BytesToZip(files, conf.PackagePaths.Survey);
                    _logger.Info(string.Format("Zip files was unpacked to '{0}'", conf.PackagePaths.Survey));

                    DeployExecutor.Validate(_dsFactory.CreateValidatorsList(conf), failedValidators, callback, conf.Survey.DeployMode);
                    _logger.Info(string.Format("Validation process is complete. Found '{0}' error(s)", failedValidators.Count));
                    if (failedValidators.Any())
                        throw new ValidationException("Validation at " + String.Join(", ", failedValidators.Select(v => v.Name).ToArray()) + " failed.");

                    DeployExecutor.Deploy(_dsFactory.CreateDeployersList(conf), deploySteps, callback, conf.Survey.DeployMode);
                    _logger.Info(string.Format("Deploy is finished for '{0}'", _sessionKey));

                    session.Commit();
                    _logger.Info(string.Format("Session for '{0}' is commited successfully", _sessionKey));
                } catch (ValidationException exception) {
                    exceptionHandler.HandleValidation(exception, callback);
                } catch (DeployException exception) {
                    exceptionHandler.HandleDeploy(exception, callback, deploySteps, GetRollbackAction(context));
                } catch (Exception exception) {
                    exceptionHandler.HandleUnknown(exception, callback);
                }
            }
        }

        private string NormalizeKey(string key) {
            var normalizedKey = key.Replace("-", "");
            return normalizedKey.Length < 50 ? normalizedKey : normalizedKey.Substring(0, 49);
        }

        private bool FileHashIsEqual(IEnumerable<byte> expectedHash, IEnumerable<byte> actualHash) {
            return expectedHash.SequenceEqual(actualHash);
        }

        private static Action<IDeployer> GetRollbackAction(DeployContext context) {
            if (context.Mode == DeployMode.Install)
                return d => d.InstallRollback();
            return d => d.UpdateRollback();
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