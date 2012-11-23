using Deployer.Client.DeployServiceReference;

namespace Deployer.Client
{
    public class Response : IDeployServiceCallback
    {
        private readonly DeployNotifier _writer;

        public Response(DeployNotifier writer) {
            _writer = writer;
        }

        #region Implementation of IDeployServiceCallback

        public void OnActionStart(ActionContext context) {
            _writer.NotifyAction(context);
        }

        public void OnRollback(ActionContext context) {
            _writer.NotifyAction(context);
        }

        public void NotifyStepState(ActionStepContext context) {
            _writer.NotifyActionStep(context);
        }

        public void OnError(ErrorContext context) {
            _writer.NotifyError(context);
        }

        public void OnFault(UnhandledException exception) {
            _writer.NotifyFault(exception);
        }

        public void OnClose() {
            _writer.NotifyClose();
            DeployClient.CloseClient = true;
        }

        #endregion
    }
}