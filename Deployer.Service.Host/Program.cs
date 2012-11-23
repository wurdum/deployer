using System;
using System.ServiceModel;
using Deployer.Service.Core;
using Microsoft.Practices.Unity;
using Unity.Wcf;

namespace Deployer.Service.Host
{
    class Program
    {
        static void Main(string[] args) {
            var container = new UnityContainer();
            ConfigureContainer(container);

            var host = new UnityServiceHost(container, typeof(DeployService));
            
            host.Open();
            Alert(host);

            Console.ReadLine();
        }

        private static void Alert(ServiceHostBase host) {
            foreach (var endpoint in host.Description.Endpoints)
                Console.WriteLine(endpoint.Address);
        }

        private static void ConfigureContainer(IUnityContainer container) {
            ServiceContainer.Configure(container);
        }
    }
}
