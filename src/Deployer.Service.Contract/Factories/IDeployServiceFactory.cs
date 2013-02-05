using System.Collections.Generic;
using Deployer.Service.Contract.Contexts;
using Deployer.Service.Contract.Data;
using Deployer.Service.Contract.Exceptions;
using Deployer.Service.Contract.Helpers;
using Deployer.Service.Contract.Settings;

namespace Deployer.Service.Contract.Factories
{
    public interface IDeployServiceFactory {
        IServiceLogger CreateLoggerObj();
        IDeployEvents CreateCallbackObj();
        IConf CreateConfObj(string sessionKey, DeployContext deployContext, IConfFactory confFactory);
        IEnumerable<IValidator> CreateValidatorsList(IConf settings);
        IEnumerable<IDeployer> CreateDeployersList(IConf settings);
        ISession CreateSessionObj(IConf settings, string user);
        IDeployExceptionHandler CreateDeployExHandlerObj(IDeployEvents callback, IConf settings, IServiceLogger logger, ISession session);
    }
}