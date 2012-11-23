using System;
using System.Configuration;
using System.ServiceModel;
using System.Threading;
using Deployer.Client.DeployServiceReference;

namespace Deployer.Client
{
    class DeployClient
    {
        public static bool CloseClient { get; set; }

        public static void Main(string[] args) {
            var options = new DeployClientOptions();
            var serviceData = options.ParseArgs(args);

            if (serviceData == null) return;

            var writer = new DeployNotifier();
            var responce = new Response(writer);
            var client = new DeployServiceClient(new InstanceContext(responce));
            client.ClientCredentials.UserName.UserName = ConfigurationManager.AppSettings["login"];
            client.ClientCredentials.UserName.Password = ConfigurationManager.AppSettings["password"];
            client.InnerChannel.Closed += (o, e) => CloseClient = true;
            client.InnerChannel.Faulted += (o, e) => CloseClient = true;
            try {
                if (client.OpenSession(serviceData.SurveyDirectory)) {
                    Console.WriteLine("Connection to service is established. Connection session is opened.");
                    var deployContext = new DeployContext {
                        UriName = serviceData.SurveyUri, 
                        DirectoryName = serviceData.SurveyDirectory, 
                        DatabaseName = serviceData.SurveyDatabase
                    };
                    if (serviceData.DeployMode == DeployMode.Install) {
                        Console.WriteLine("Install is started.");
                        client.Install(deployContext, serviceData.SurveyFiles);
                    } else {
                        Console.WriteLine("Update is started.");
                        client.Update(deployContext, serviceData.DeployMode, serviceData.SurveyFiles);
                    }
                    Console.WriteLine("Please wait until this procedure is completed.");
                } else {
                    Console.WriteLine("Connection was terminated because folder '{0}' is already is use.", serviceData.SurveyDirectory);
                    CloseClient = true;
                    client.Close();
                }
            } catch (Exception ex) {
                HandleException(ex);
                CloseClient = true;
                client.Abort();
            }

            while (!CloseClient) { Thread.Sleep(500); }

            if (client.State != CommunicationState.Closing &&
                client.State != CommunicationState.Closed)
                client.Close();

            Console.WriteLine("\r\nPress any key to close...");
            Console.ReadKey();
        }

        private static void HandleException(Exception ex) {
            Console.WriteLine("Exception received: {0}", ex.Message);
            Console.WriteLine(ex.StackTrace);
        }
    }
}