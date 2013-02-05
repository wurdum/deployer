using Deployer.Service.Contract.Helpers;
using Deployer.Service.Contract.Settings;

namespace Deployer.Service.Contract.Factories
{
    public interface IConfFactory
    {
        IPackageManager CreatePackageManager();
        IIISManager CreateIISManager(IISInfo iis, string surveyUri);
    }
}