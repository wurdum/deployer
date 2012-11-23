using System;
using System.Linq;
using Deployer.Service.Contract.Data;
using Deployer.Service.Contract.Settings;
using Deployer.Service.Data;

namespace Deployer.Service.Core.Settings
{
    public class DeploySession : IDeploySession
    {
        private readonly string _sessionKey;
        private readonly string _user;
        private readonly IDeploySettings _settings;
        private bool _isCommited;

        protected DeploySession(IDeploySettings settings) {
            _settings = settings;
        }

        public DeploySession(IDeploySettings settings, string sessionKey, string user) : this(settings) {
            _sessionKey = sessionKey;
            _user = user;
            _isCommited = false;
            Open(_sessionKey);
        }

        public DbDataContext GetDataContext() {
            return new DbDataContext(_settings["DeployerConnectionString"]);
        }

        public void Open(string sessionKey) {
            using (var context = GetDataContext()) {
                context.Deploys.InsertOnSubmit(new Deploy {
                    User = _user,
                    Mode = (int) _settings.DeployMode,
                    Status = (int) DeployStatusModel.InProcess,
                    SessionKey = sessionKey.Length >= 50 ? sessionKey.Substring(0, 49) : sessionKey,
                    SurveyName = _settings.SurveyName,
                    StartUtc = DateTime.UtcNow
                });

                context.SubmitChanges();
                _settings.SessionKey = sessionKey;
            }
        }

        public void Close() {
            using (var context = GetDataContext()) {
                var deploy = context.Deploys.Single(d => d.SessionKey == _settings.SessionKey);
                deploy.Status = _isCommited ? (int) DeployStatusModel.Succeed : (int) DeployStatusModel.Failed;
                deploy.EndUtc = DateTime.UtcNow;
                context.SubmitChanges();
            }
        }

        public void Commit() {
            _isCommited = true;
        }

        #region Implementation of IDisposable

        public void Dispose() {
            Close();
        }

        #endregion
    }
}