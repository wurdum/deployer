using System.ServiceModel;
using Deployer.Service.Contract.Contexts;

namespace Deployer.Service.Contract
{
    [ServiceContract(CallbackContract = typeof(IDeployEvents), SessionMode = SessionMode.Required)]
    public interface IDeployService
    {
        [OperationContract(IsInitiating = true)]
        bool OpenSession(string folderName);

        [OperationContract(IsInitiating = false)]
        int SendFilesChunk(byte[] chunk);

        [OperationContract(IsOneWay = true, IsInitiating = false)]
        void Deploy(DeployContext context, byte[] filesHash);
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