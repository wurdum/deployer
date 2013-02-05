using System;
using Deployer.Service.Data;

namespace Deployer.Service.Contract.Data
{
    public interface ISession : IDisposable
    {
        DbDataContext GetDataContext();
        void Open();
        void Close();
        void Commit();
    }
}