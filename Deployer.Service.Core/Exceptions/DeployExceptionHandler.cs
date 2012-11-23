using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Deployer.Service.Contract;
using Deployer.Service.Contract.Contexts;
using Deployer.Service.Contract.Data;
using Deployer.Service.Contract.Exceptions;
using Deployer.Service.Contract.Helpers;
using Deployer.Service.Contract.Settings;
using Exception = System.Exception;

namespace Deployer.Service.Core.Exceptions
{
    public class DeployExceptionHandler : IDeployExceptionHandler
    {
        private readonly IDeploySession _session;
        private readonly IDeployEvents _callback;
        private readonly IDeploySettings _appSettings;
        private readonly IServiceLogger _logger;

        public DeployExceptionHandler(IDeployEvents callback, IDeploySettings appSettings, IServiceLogger logger, IDeploySession session) {
            _callback = callback;
            _session = session;
            _appSettings = appSettings;
            _logger = logger;
        }

        public void HandleValidation(ValidationException ex, IDeployEvents callback) {
            _logger.Error("Validation exception is catched", ex);
            HandleException(ex);
        }

        public void HandleDeploy(DeployException ex, IDeployEvents callback, List<IDeployer> deploySteps, Action<IDeployer> rollbackExecutor) {
            _logger.Error("Deploy exception is catched", ex);
            HandleException(ex);
            foreach (var installStep in deploySteps)
                try {
                    rollbackExecutor(installStep);
                    _logger.Info("Rollback is complete");        
                } catch (RollbackException rollbackEx) {
                    HandleException(rollbackEx);
                } catch (Exception unknownEx) {
                    HandleException(unknownEx);
                }
        }

        public void HandleUnknown(Exception ex, IDeployEvents callback) {
            _logger.Error("Unknown exception is catched", ex);
            HandleException(ex);
        }

        private void HandleException(Exception ex) {
            _callback.OnError(new ErrorContext {
                Name = ex.Source,
                Exception = ex.ToXml(),
                Timestamp = DateTime.UtcNow
            });

            _logger.Info("Exception is handled and exception data is sent to client");
            using (var context = _session.GetDataContext()) {
                var deploy = context.Deploys.Single(d => d.SessionKey == _appSettings.SessionKey);
                context.Exceptions.InsertOnSubmit(new Data.Exception {
                    DeployId = deploy.Id,
                    Message = ex.Message.RemoveExcessSpaces(),
                    Source = ex.Source,
                    TimeStamp = DateTime.UtcNow,
                    ExceptionData = ex.ToXml()
                });
                context.SubmitChanges();
            }
        }

        public static DeployExceptionHandler Create(IDeployEvents callback, IDeploySettings appSettings, IServiceLogger logger, IDeploySession session) {
            return new DeployExceptionHandler(callback, appSettings, logger, session);
        }
    }

    public static class ExceptionHelper
    {
        public static XElement ToXml(this Exception ex) {
            if (ex == null)
                return null;

            var root = new XElement("exception", 
                new XElement("type", ex.GetType().Name),
                new XElement("message", ex.Message.RemoveExcessSpaces()),
                new XElement("stacktrace", ex.StackTrace.RemoveExcessSpaces()),
                new XElement("innerexception", ex.InnerException.ToXml())
            );
            
            return root;
        }

        public static string RemoveExcessSpaces(this string str) {
            return String.Join("\r\n", str.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.TrimStart().TrimEnd()).ToArray());
        }
    }
}