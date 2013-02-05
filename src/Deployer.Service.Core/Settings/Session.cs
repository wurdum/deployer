using System;
using System.Linq;
using Deployer.Service.Contract.Data;
using Deployer.Service.Contract.Settings;
using Deployer.Service.Data;

namespace Deployer.Service.Core.Settings
{
    public class Session : ISession
    {
        private readonly string _user;
        private readonly IConf _conf;
        private bool _isCommited;

        protected Session(IConf conf) {
            _conf = conf;
        }

        public Session(IConf conf, string user) : this(conf) {
            _user = user;
            _isCommited = false;
            Open();
        }

        public DbDataContext GetDataContext() {
            return new DbDataContext(_conf.ConnectionStrings.Deployer);
        }

        public void Open() {
            using (var context = GetDataContext()) {
                context.Deploys.InsertOnSubmit(new Deploy {
                    User = _user,
                    Mode = (int) _conf.Survey.DeployMode,
                    Status = (int) DeployStatusModel.InProcess,
                    SessionKey = _conf.SessionKey.Length >= 50 ? _conf.SessionKey.Substring(0, 49) : _conf.SessionKey,
                    SurveyName = _conf.Survey.Name,
                    StartUtc = DateTime.UtcNow
                });

                context.SubmitChanges();
            }
        }

        public void Close() {
            using (var context = GetDataContext()) {
                var deploy = context.Deploys.Single(d => d.SessionKey == _conf.SessionKey);
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