using System;
using Deployer.Service.Contract.Helpers;

namespace Deployer.Tests.Stubs
{
    public class ServiceLoggerStub : IServiceLogger
    {
        public void Info(string msg) {
            Console.WriteLine("Inf: " + msg);
        }

        public void Error(string msg) {
            Console.WriteLine("Error: " + msg);
        }

        public void Error(string msg, Exception ex) {
            Console.WriteLine("Error with exception: " + msg + "\r\n" + ex);
        }
    }
}