using System;
using Deployer.Service.Contract.Helpers;

namespace Deployer.Tests.Service.Stubs
{
    public class ServiceLoggerStub : IServiceLogger
    {
        public void Info(string msg) { }

        public void Error(string msg) { }

        public void Error(string msg, Exception ex) { }
    }
}