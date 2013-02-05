using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.ServiceModel;
using Deployer.Client.DeployServiceReference;
using Deployer.Options;

namespace Deployer.Transport
{
    public class ClientFacade : DeployServiceClient
    {
        public static bool NeedClose = false;
        private readonly ServiceData _serviceData;

        public ClientFacade(string login, string password, ServiceData serviceData)
            : base(new InstanceContext(new Response(new Notifier()))) {
            _serviceData = serviceData;

            ClientCredentials.UserName.UserName = login;
            ClientCredentials.UserName.Password = password;
            InnerChannel.Closed += (o, e) => NeedClose = true;
            InnerChannel.Faulted += (o, e) => NeedClose = true;
        }

        public bool OpenSession() {
            return OpenSession(_serviceData.Directory);
        }

        public void SendFiles() {
            var chunks = _serviceData.Files.GetChunks(1024 * 512).ToArray();
            Console.WriteLine("Sending survey files started (total " + chunks.Length + " chunks):");

            var counter = 0;
            foreach (var chunk in chunks) {
                var sent = SendFilesChunk(chunk);
                if (sent != chunk.Length)
                    throw new InvalidOperationException("Operation was terminated because sent chunk length and " +
                                                        "received chunk length are differ!");
                Console.Write("{0}/{1} chunks sent\r", ++counter, chunks.Length);
            }
            Console.WriteLine();
        }

        public void Deploy() {
            var deployContext = new DeployContext {
                UriName = _serviceData.Uri,
                DirectoryName = _serviceData.Directory,
                DatabaseName = _serviceData.Database,
                Mode = _serviceData.Mode
            };

            Console.WriteLine(deployContext.Mode == DeployMode.Install ? "Install is started." : "Update is started.");

            Deploy(deployContext, MD5.Create().ComputeHash(_serviceData.Files));
            
            Console.WriteLine("Please wait until this procedure is completed.");
        }
    }

    public static class FileSendingUtils
    {
        public static IEnumerable<byte[]> GetChunks(this byte[] file, int chunkSize) {
            if (file.Length == 0)
                yield break;

            var totalChunks = file.Length / chunkSize;
            for (var i = 0; i <= totalChunks; i++) {
                var offset = i * chunkSize;
                var lenght = Math.Min(chunkSize, file.Length - offset);

                if (lenght == 0)
                    yield break;

                var chunk = new byte[lenght];
                Array.Copy(file, offset, chunk, 0, lenght);
                yield return chunk;
            }
        }
    }
}