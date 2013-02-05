using System;
using Deployer.Service.Contract.Helpers;
using Deployer.Service.Contract.Settings;

namespace Deployer.Service.Core.Helpers.Validators
{
    public abstract class Validator : MarshalByRefObject, IValidator
    {
        protected readonly IConf _conf;
        protected Validator(IConf conf) {
            _conf = conf;
        }

        public string Name { get; protected set; }

        public abstract bool IsValidForInstall();
        public abstract bool IsValidForUpdate();
    }
}