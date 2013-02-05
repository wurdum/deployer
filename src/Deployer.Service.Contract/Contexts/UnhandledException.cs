using System;
using System.Runtime.Serialization;

namespace Deployer.Service.Contract.Contexts
{
    [DataContract]
    public class UnhandledException
    {
        [DataMember]
        public string Message { get; set; }
        [DataMember]
        public string Source { get; set; }
        [DataMember]
        public string StackTrace { get; set; }
        [DataMember]
        public string InnerExMessage { get; set; }

        public static UnhandledException CreateFromEx(Exception ex) {
            return new UnhandledException {
                Message = ex.Message,
                Source = ex.Source,
                StackTrace = ex.StackTrace,
                InnerExMessage = ex.InnerException == null ? string.Empty : ex.InnerException.Message 
            };
        }
    }
}