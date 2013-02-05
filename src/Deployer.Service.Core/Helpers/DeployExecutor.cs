using System;
using System.Collections.Generic;
using System.Linq;
using Deployer.Service.Contract;
using Deployer.Service.Contract.Contexts;
using Deployer.Service.Contract.Helpers;

namespace Deployer.Service.Core.Helpers
{
    public class DeployExecutor
    {
        private readonly IDeployEvents _callback;
        private readonly DeployMode _mode;

        public DeployExecutor(IDeployEvents callback, DeployMode mode) {
            _callback = callback;
            _mode = mode;
        }

        /// <summary>
        /// Starts validation by using validators from ctor and writes all failed 
        /// validations to <see cref="failedValidators"/>
        /// </summary>
        public void Validate(IEnumerable<IValidator> validators, List<IValidator> failedValidators) {
            if (failedValidators == null || failedValidators.Count != 0)
                throw new ArgumentException("Failed validators collection wasn't initialized or already has some elements.", "failedValidators");

            ExecuteWithCallbacks("Validation", validators, failedValidators, (ecValidator, ecFailedValidators, ecStepContext) => {
                var isValid = _mode == DeployMode.Install ? ecValidator.IsValidForInstall() : ecValidator.IsValidForUpdate();
                if (isValid) {
                    ecStepContext.Status = StepStatus.Complete;
                } else {
                    failedValidators.Add(ecValidator);
                    ecStepContext.Status = StepStatus.Failed;
                }
            });
        }

        /// <summary>
        /// Executes all deploys from <see cref="deployers"/> and writes completes deploys to 
        /// <see cref="deploySteps"/>
        /// </summary>
        public void Deploy(IEnumerable<IDeployer> deployers, List<IDeployer> deploySteps) {
            if (deploySteps == null || deploySteps.Count != 0)
                throw new ArgumentException("Install steps container wasn't initialized or already has some elements.", "installSteps");

            ExecuteWithCallbacks("Deployment", deployers, deploySteps, (ecDeployer, ecDeploySteps, ecStepContext) => {
                try {
                    if (_mode == DeployMode.Install)
                        ecDeployer.Install();
                    else ecDeployer.Update();
                } finally {
                    ecDeploySteps.Add(ecDeployer);
                }
                ecStepContext.Status = StepStatus.Complete;
            });
        }

        protected void ExecuteWithCallbacks<T>(string action, IEnumerable<T> serviceHelpers, List<T> helpersState, Action<T, List<T>, ActionStepContext> executionBody) 
            where T : IHelper {
            var actionSteps = serviceHelpers.Select(h => new ActionStepContext { Name = h.Name }).ToList();
            var actionContext = new ActionContext {
                Name = action,
                Steps = actionSteps,
                Description = DescriptionsFactory.Action(action).Start(DateTime.UtcNow)
            };

            _callback.OnActionStart(actionContext);

            foreach (var helper in serviceHelpers) {
                var stepContext = actionContext.Steps.Single(s => s.Name == helper.Name);
                stepContext.Description = DescriptionsFactory.Action("Step").Start(DateTime.UtcNow);
                stepContext.Status = StepStatus.InProgress;
                _callback.NotifyStepState(stepContext);

                try {
                    executionBody(helper, helpersState, stepContext);
                } catch (Exception) {
                    stepContext.Status = StepStatus.Failed;
                    throw;
                } finally {
                    stepContext.Description = DescriptionsFactory.Action("Step").Complete(DateTime.UtcNow);
                    _callback.NotifyStepState(stepContext);
                }
            }
        }

        public static void Deploy(IEnumerable<IDeployer> deployers, List<IDeployer> installSteps, IDeployEvents callback, DeployMode mode) {
            var deployExecutor = new DeployExecutor(callback, mode);
            deployExecutor.Deploy(deployers, installSteps);
        }

        public static void Validate(IEnumerable<IValidator> validators, List<IValidator> failedValidators, IDeployEvents callback, DeployMode mode) {
            var validationExecutor = new DeployExecutor(callback, mode);
            validationExecutor.Validate(validators, failedValidators);
        }
    }
}