using Deployer.Client.DeployServiceReference;

namespace Deployer.Transport
{
    public class Response : IDeployServiceCallback
    {
        private readonly Notifier _writer;

        public Response(Notifier writer) {
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
            ClientFacade.NeedClose = true;
        }

        #endregion
    }
}