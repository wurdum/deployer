using Deployer.Service.Core;
using Microsoft.Practices.Unity;
using Unity.Wcf;

namespace Deployer.Service.WebHost
{
	public class WcfServiceFactory : UnityServiceHostFactory
	{
	    protected override void ConfigureContainer(IUnityContainer container) {
            ServiceContainer.Configure(container);   
        }
    }
}