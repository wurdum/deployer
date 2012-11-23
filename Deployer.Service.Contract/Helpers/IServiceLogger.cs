using System;

namespace Deployer.Service.Contract.Helpers
{
    public interface IServiceLogger 
    {
        void Info(string msg);
        void Error(string msg);
        void Error(string msg, Exception ex);
    }
}