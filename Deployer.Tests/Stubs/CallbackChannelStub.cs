using Deployer.Service.Contract;
using Deployer.Service.Contract.Contexts;

namespace Deployer.Tests.Stubs
{
    public class CallbackChannelStub : IDeployEvents
    {
        public void OnActionStart(ActionContext context) { }
        public void OnRollback(ActionContext context) { }
        public void NotifyStepState(ActionStepContext context) { }
        public void OnError(ErrorContext context) { }
        public void OnFault(UnhandledException exception) { }
        public void OnClose() { }
    }
}