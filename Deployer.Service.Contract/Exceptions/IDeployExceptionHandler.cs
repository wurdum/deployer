using System;
using System.Collections.Generic;
using Deployer.Service.Contract.Helpers;

namespace Deployer.Service.Contract.Exceptions
{
    public interface IDeployExceptionHandler {
        void HandleValidation(ValidationException ex, IDeployEvents callback);
        void HandleDeploy(DeployException ex, IDeployEvents callback, List<IDeployer> deploySteps, Action<IDeployer> rollbackExecutor);
        void HandleUnknown(Exception ex, IDeployEvents callback);
    }
}