using System;
using Deployer.Service.Contract.Helpers;
using Deployer.Service.Contract.Settings;

namespace Deployer.Service.Core.Helpers.Validators
{
    public abstract class Validator : MarshalByRefObject, IValidator
    {
        protected readonly IDeploySettings _appSettings;
        protected Validator(IDeploySettings appSettings) {
            _appSettings = appSettings;
        }

        public string Name { get; protected set; }

        public abstract bool IsValidForInstall();
        public abstract bool IsValidForUpdate();
    }
}