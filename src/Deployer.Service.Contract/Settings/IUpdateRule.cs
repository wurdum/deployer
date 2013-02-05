namespace Deployer.Service.Contract.Settings
{
    public interface IUpdateRule
    {
        bool Satisfies(string filePath);
    }
}