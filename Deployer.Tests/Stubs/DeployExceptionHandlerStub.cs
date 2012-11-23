﻿using System;
using System.Collections.Generic;
using Deployer.Service.Contract;
using Deployer.Service.Contract.Exceptions;
using Deployer.Service.Contract.Helpers;

namespace Deployer.Tests.Stubs
{
    public class DeployExceptionHandlerStub : IDeployExceptionHandler
    {
        public void HandleValidation(ValidationException ex, IDeployEvents callback) {
            // ex handling
        }

        public void HandleDeploy(DeployException ex, IDeployEvents callback, List<IDeployer> deploySteps, Action<IDeployer> rollbackExecutor) {
            foreach (var deployStep in deploySteps) {
                rollbackExecutor(deployStep);
                Console.WriteLine("Called rollback for '{0}'", deployStep.Name);
            }
                
        }

        public void HandleUnknown(Exception ex, IDeployEvents callback) {
            // ex handling
        }
    }
}