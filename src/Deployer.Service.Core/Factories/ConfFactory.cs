using System;
using Deployer.Service.Contract.Factories;
using Deployer.Service.Contract.Helpers;
using Deployer.Service.Contract.Settings;

namespace Deployer.Service.Core.Factories
{
    public class ConfFactory : IConfFactory
    {
        private readonly Func<IPackageManager> _pmFactory;
        private readonly Func<IISInfo, string, IIISManager> _iismFactory;

        public ConfFactory(Func<IPackageManager> pmFactory, Func<IISInfo, string, IIISManager> iismFactory) {
            _pmFactory = pmFactory;
            _iismFactory = iismFactory;
        }

        public IPackageManager CreatePackageManager() {
            return _pmFactory();
        }

        public IIISManager CreateIISManager(IISInfo iis, string surveyUri) {
            return _iismFactory(iis, surveyUri);
        }
    }
}