using System.Collections.Generic;

namespace Deployer.Service.WebHost.Models
{
    public class LogModel
    {
        public string Active { get; set; }
        public IEnumerable<string> LogEntries { get; set; }
        public IEnumerable<string> Content { get; set; } 
    }
}