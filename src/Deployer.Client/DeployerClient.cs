using System;
using System.Configuration;
using System.Threading;
using Deployer.Options;
using Deployer.Transport;

namespace Deployer.Client
{
    class DeployerClient
    {
        public static bool CloseClient { get; set; }

        public static void Main(string[] args) {
            //var testArgs = new[] { "-i", @"D:\deploy\project", "-n", "TESTRUN2" };

            var options = new OptionsParser();
            var serviceData = options.Parse(args);
            if (serviceData == null)
                return;

            var client = new ClientFacade(
                ConfigurationManager.AppSettings["login"],
                ConfigurationManager.AppSettings["password"],
                serviceData);

            try {
                if (client.OpenSession()) {
                    Console.WriteLine("Connection to service is established. Connection session is opened.");

                    client.SendFiles();
                    client.Deploy();
                } else {
                    Console.WriteLine("Connection was terminated because folder '{0}' is already is use.", serviceData.Directory);

                    client.Close();
                    ClientFacade.NeedClose = true;
                }
            } catch (Exception ex) {
                HandleException(ex);
                client.Abort();
                ClientFacade.NeedClose = true;
            }

            while (ClientFacade.NeedClose != true) { Thread.Sleep(500); }

            Console.WriteLine("\r\nPress any key to close...");
            Console.ReadKey();
        }

        private static void HandleException(Exception ex) {
            Console.WriteLine("Exception received: {0}", ex.Message);
            Console.WriteLine(ex.StackTrace);
        }
    }
}