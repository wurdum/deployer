using System;
using Deployer.Service.Contract.Settings;
using Deployer.Service.Core.Helpers.Validators;

namespace Deployer.Tests.Stubs
{
    public class ValidatorStub : Validator
    {
        private readonly string _type;

        public ValidatorStub(IDeploySettings appSettings) : base(appSettings) {}

        public ValidatorStub(string name, string type, IDeploySettings appSettings) : base(appSettings) {
            Name = name;
            _type = type;
        }

        public override bool IsValidForInstall() {
            Console.WriteLine("'{0}': validation for {1}", Name, _type);
            return true;
        }

        public override bool IsValidForUpdate() {
            Console.WriteLine("'{0}': validation for {1}", Name, _type);
            return true;
        }
    }

    public class DeployerStub : Service.Core.Helpers.Deplyers.Deployer
    {
        public DeployerStub(IDeploySettings appSettings) : base(appSettings) {}
        
        public DeployerStub(string name, IDeploySettings appSettings) : base(appSettings) {
            Name = name;
        }

        public override void Install() {
            Console.WriteLine("'{0}': install", Name);
        }

        public override void InstallRollback() {
            Console.WriteLine("'{0}': install rollback", Name);
        }

        public override void Update() {
            Console.WriteLine("'{0}': update", Name);
        }

        public override void UpdateRollback() {
            Console.WriteLine("'{0}': update rollback", Name);
        }
    }
}