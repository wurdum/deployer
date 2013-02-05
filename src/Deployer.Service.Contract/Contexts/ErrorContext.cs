using System;
using System.Runtime.Serialization;
using System.Xml.Linq;

namespace Deployer.Service.Contract.Contexts
{
    [DataContract]
    public class ErrorContext
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public XElement Exception { get; set; }

        [DataMember]
        public DateTime Timestamp { get; set; }
    }
}