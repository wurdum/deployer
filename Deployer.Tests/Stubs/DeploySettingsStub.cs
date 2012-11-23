using System;
using Deployer.Service.Contract.Contexts;
using Deployer.Service.Contract.Helpers;
using Deployer.Service.Core.Settings;

namespace Deployer.Tests.Stubs
{
    public class DeploySettingsStub : DeploySettings
    {
        private readonly IPackageManager _packageManager;

        protected DeploySettingsStub(string settingsFile, IPackageManager packageManager) : base(settingsFile, packageManager) { }
        protected DeploySettingsStub(string settingsFile, DeployContext options, IPackageManager packageManager) : base(settingsFile, options, packageManager) {}
        public DeploySettingsStub(string settingsFile, DeployContext options, DeployMode deployMode, IPackageManager packageManager) : base(settingsFile, options, deployMode, packageManager) {
            _packageManager = packageManager;
        }

        public DeploySettingsStub(string settingsFile, DeployContext options, DeployMode deployMode) : base(settingsFile, options, deployMode, new PackageManagerStub()) { }

        public override IPackageManager PackageManager {
            get { return _packageManager; }
        }

        protected override string AppFolder {
            get { return AppDomain.CurrentDomain.BaseDirectory; }
        }

        public override string IISSurveyPath {
            get { return SurveyPath; }
        }
    }
}