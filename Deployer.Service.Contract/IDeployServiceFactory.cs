using System.Collections.Generic;
using Deployer.Service.Contract.Contexts;
using Deployer.Service.Contract.Data;
using Deployer.Service.Contract.Exceptions;
using Deployer.Service.Contract.Helpers;
using Deployer.Service.Contract.Settings;

namespace Deployer.Service.Contract
{
    public interface IDeployServiceFactory {
        IServiceLogger CreateLoggerObj();
        IDeployEvents CreateCallbackObj();
        IPackageManager CreatePackageManager();
        IDeploySettings CreateSettingsObj(string settingsFile, DeployContext deployContext, DeployMode deployMode, IPackageManager packageManager);
        IEnumerable<IValidator> CreateValidatorsList(IDeploySettings settings);
        IEnumerable<IDeployer> CreateDeployersList(IDeploySettings settings);
        IDeploySession CreateSessionObj(IDeploySettings settings, string sessionKey, string user);
        IDeployExceptionHandler CreateDeployExHandlerObj(IDeployEvents callback, IDeploySettings settings, IServiceLogger logger, IDeploySession session);
    }
}