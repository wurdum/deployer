using System;
using Deployer.Service.Contract.Helpers;
using NLog;

namespace Deployer.Service.Core.Helpers
{
    public class ServiceLogger : IServiceLogger
    {
        private readonly Logger _logger;

        public ServiceLogger() {
            _logger = LogManager.GetCurrentClassLogger();
        }

        public void Info(string msg) {
            _logger.Info(msg);
        }

        public void Error(string msg) {
            _logger.Error(msg);
        }

        public void Error(string msg, Exception ex) {
            _logger.ErrorException(msg, ex);
        }
    }
}