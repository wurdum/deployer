using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Deployer.Service.Contract.Contexts
{
    [DataContract]
    public class ActionContext
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public IEnumerable<ActionStepContext> Steps { get; set; }

        [DataMember]
        public string Description { get; set; }
    }

    [DataContract]
    public class ActionStepContext
    {
        [DataMember]
        public string Key { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public StepStatus Status { get; set; }
    }

    [DataContract]
    public enum StepStatus
    {
        [EnumMember] None = -1,
        [EnumMember] InProgress,
        [EnumMember] Complete,
        [EnumMember] Failed
    }
}