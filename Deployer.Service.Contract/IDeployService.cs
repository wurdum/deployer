using System.ServiceModel;
using Deployer.Service.Contract.Contexts;

namespace Deployer.Service.Contract
{
    [ServiceContract(CallbackContract = typeof(IDeployEvents), SessionMode = SessionMode.Required)]
    public interface IDeployService
    {
        [OperationContract(IsInitiating = true)]
        bool OpenSession(string folderName);

        [OperationContract(IsOneWay = true, IsInitiating = false)]
        void Install(DeployContext context, byte[] files);

        [OperationContract(IsOneWay = true, IsInitiating = false)]
        void Update(DeployContext context, DeployMode mode, byte[] files);
    }

    [ServiceContract]
    public interface IDeployEvents
    {
        [OperationContract(IsOneWay = true)]
        void OnActionStart(ActionContext context);

        [OperationContract(IsOneWay = true)]
        void OnRollback(ActionContext context);

        [OperationContract(IsOneWay = true)]
        void NotifyStepState(ActionStepContext context);

        [OperationContract(IsOneWay = true)]
        void OnError(ErrorContext context);

        [OperationContract(IsOneWay = true)]
        void OnFault(UnhandledException exception);

        [OperationContract(IsOneWay = true)]
        void OnClose();
    }
}