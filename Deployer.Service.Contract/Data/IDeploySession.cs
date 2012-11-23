using System;
using Deployer.Service.Data;

namespace Deployer.Service.Contract.Data
{
    public interface IDeploySession : IDisposable
    {
        DbDataContext GetDataContext();
        void Open(string sessionKey);
        void Close();
        void Commit();
    }
}