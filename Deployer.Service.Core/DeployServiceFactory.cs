using System;
using System.Collections.Generic;
using Deployer.Service.Contract;
using Deployer.Service.Contract.Contexts;
using Deployer.Service.Contract.Data;
using Deployer.Service.Contract.Exceptions;
using Deployer.Service.Contract.Helpers;
using Deployer.Service.Contract.Settings;

namespace Deployer.Service.Core
{
    public class DeployServiceFactory : IDeployServiceFactory
    {
        private readonly IServiceLogger _logger;
        private readonly Func<IDeployEvents> _callbackFactory;
        private readonly Func<IPackageManager> _pmFactory;
        private readonly Func<string, DeployContext, DeployMode, IPackageManager, IDeploySettings> _settingsFactory;
        private readonly Func<IDeploySettings, IEnumerable<IValidator>> _validatorsFactory;
        private readonly Func<IDeploySettings, IEnumerable<IDeployer>> _deployersFactory;
        private readonly Func<IDeploySettings, string, string, IDeploySession> _sessionFactory;
        private readonly Func<IDeployEvents, IDeploySettings, IServiceLogger, IDeploySession, IDeployExceptionHandler> _exHandlerFactory;

        public DeployServiceFactory(
            IServiceLogger logger,
            Func<IDeployEvents> callbackFactory,
            Func<IPackageManager> pmFactory,
            Func<string, DeployContext, DeployMode, IPackageManager, IDeploySettings> settingsFactory,
            Func<IDeploySettings, IEnumerable<IValidator>> validatorsFactory, 
            Func<IDeploySettings, IEnumerable<IDeployer>> deployersFactory,
            Func<IDeploySettings, string, string, IDeploySession> sessionFactory,
            Func<IDeployEvents, IDeploySettings, IServiceLogger, IDeploySession, IDeployExceptionHandler> exHandlerFactory) {

            _logger = logger;
            _callbackFactory = callbackFactory;
            _pmFactory = pmFactory;
            _settingsFactory = settingsFactory;
            _validatorsFactory = validatorsFactory;
            _deployersFactory = deployersFactory;
            _sessionFactory = sessionFactory;
            _exHandlerFactory = exHandlerFactory;
        }

        public IServiceLogger CreateLoggerObj() {
            return _logger;
        }

        public IDeployEvents CreateCallbackObj() {
            return _callbackFactory();
        }

        public IPackageManager CreatePackageManager() {
            return _pmFactory();
        }

        public IDeploySettings CreateSettingsObj(string settingsFile, DeployContext deployContext, DeployMode deployMode, IPackageManager packageManager) {
            return _settingsFactory(settingsFile, deployContext, deployMode, packageManager);
        }

        public IEnumerable<IValidator> CreateValidatorsList(IDeploySettings settings) {
            return _validatorsFactory(settings);
        }

        public IEnumerable<IDeployer> CreateDeployersList(IDeploySettings settings) {
            return _deployersFactory(settings);
        }

        public IDeploySession CreateSessionObj(IDeploySettings settings, string sessionKey, string user) {
            return _sessionFactory(settings, sessionKey, user);
        }

        public IDeployExceptionHandler CreateDeployExHandlerObj(IDeployEvents callback, IDeploySettings settings, IServiceLogger logger, IDeploySession session) {
            return _exHandlerFactory(callback, settings, logger, session);
        }
    }
}