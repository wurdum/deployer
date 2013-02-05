using System;
using System.Collections.Generic;
using Deployer.Service.Contract;
using Deployer.Service.Contract.Contexts;
using Deployer.Service.Contract.Data;
using Deployer.Service.Contract.Exceptions;
using Deployer.Service.Contract.Factories;
using Deployer.Service.Contract.Helpers;
using Deployer.Service.Contract.Settings;

namespace Deployer.Service.Core.Factories
{
    public class DeployServiceFactory : IDeployServiceFactory
    {
        private readonly IServiceLogger _logger;
        private readonly Func<IDeployEvents> _callbackFactory;
        private readonly Func<string, DeployContext, IConfFactory, IConf> _confFactory;
        private readonly Func<IConf, IEnumerable<IValidator>> _validatorsFactory;
        private readonly Func<IConf, IEnumerable<IDeployer>> _deployersFactory;
        private readonly Func<IConf, string, ISession> _sessionFactory;
        private readonly Func<IDeployEvents, IConf, IServiceLogger, ISession, IDeployExceptionHandler> _exHandlerFactory;

        public DeployServiceFactory(
            IServiceLogger logger,
            Func<IDeployEvents> callbackFactory,
            Func<string, DeployContext, IConfFactory, IConf> confFactory,
            Func<IConf, IEnumerable<IValidator>> validatorsFactory, 
            Func<IConf, IEnumerable<IDeployer>> deployersFactory,
            Func<IConf, string, ISession> sessionFactory,
            Func<IDeployEvents, IConf, IServiceLogger, ISession, IDeployExceptionHandler> exHandlerFactory) {

            _logger = logger;
            _callbackFactory = callbackFactory;
            _confFactory = confFactory;
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

        public IConf CreateConfObj(string sessionKey, DeployContext deployContext, IConfFactory confFactory) {
            return _confFactory(sessionKey, deployContext, confFactory);
        }

        public IEnumerable<IValidator> CreateValidatorsList(IConf settings) {
            return _validatorsFactory(settings);
        }

        public IEnumerable<IDeployer> CreateDeployersList(IConf settings) {
            return _deployersFactory(settings);
        }

        public ISession CreateSessionObj(IConf settings, string user) {
            return _sessionFactory(settings, user);
        }

        public IDeployExceptionHandler CreateDeployExHandlerObj(IDeployEvents callback, IConf settings, IServiceLogger logger, ISession session) {
            return _exHandlerFactory(callback, settings, logger, session);
        }
    }
}