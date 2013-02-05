using System;
using Deployer.Service.Contract.Exceptions;
using Deployer.Service.Contract.Settings;

namespace Deployer.Service.Core.Helpers.Deplyers
{
    public class IISDeployer : Deployer
    {
        public IISDeployer(IConf conf) : base(conf) {
            Name = "IIS";
        }

        #region Overrides of Deployer

        public override void Install() {
            try {
                _conf.IISManager.CreateSurvey(_conf.SurveyPath.Survey);
            } catch (Exception ex) {
                throw new IISDeploymentException(Name, ex);
            }
        }

        public override void InstallRollback() {
            try {
                _conf.IISManager.DeleteSurvey();
            } catch (Exception ex) {
                throw new RollbackException(Name, ex);
            }
        }

        public override void Update() {
            throw new NotImplementedException("There are no scenarios to update " + Name);
        }

        public override void UpdateRollback() {
            throw new NotImplementedException("There are no scenarios to update " + Name);
        }

        #endregion

        public static IISDeployer Create(IConf appSettings) {
            return new IISDeployer(appSettings);
        }
    }
}