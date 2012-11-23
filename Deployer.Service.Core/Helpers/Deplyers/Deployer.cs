using System;
using Deployer.Service.Contract.Helpers;
using Deployer.Service.Contract.Settings;

namespace Deployer.Service.Core.Helpers.Deplyers
{
    public abstract class Deployer : MarshalByRefObject, IDeployer
    {
        protected readonly IDeploySettings _appSettings;
        protected Deployer(IDeploySettings appSettings) {
            _appSettings = appSettings;
        }

        public string Name { get; protected set; }

        public abstract void Install();
        public abstract void InstallRollback();
        public abstract void Update();
        public abstract void UpdateRollback();
    }
}