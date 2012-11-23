using System;
using System.Collections.Generic;
using Deployer.Service.Data;
using DeployMode = Deployer.Service.Contract.Contexts.DeployMode;

namespace Deployer.Service.WebHost.Models
{
    public class DeployRecord
    {
        public string Key {get; set; }
        public string User { get; set; }
        public string Name { get; set; }
        public DeployStatusModel Status { get; set; }
        public DeployMode Mode { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }

        public IEnumerable<ExceptionRecord> Exceptions { get; set; }
    }
}