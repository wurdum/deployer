namespace Deployer.Service.Contract.Helpers
{
    public interface IHelper
    {
        string Name { get; } 
    }

    public interface IValidator : IHelper
    {
        bool IsValidForInstall();
        bool IsValidForUpdate();
    }

    public interface IDeployer : IHelper
    {
        void Install();
        void InstallRollback();
        void Update();
        void UpdateRollback();
    }
}