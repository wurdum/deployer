using System.Runtime.Serialization;

namespace Deployer.Service.Contract.Contexts
{
    [DataContract]
    public class DeployContext : IExtensibleDataObject
    {
        [DataMember(IsRequired = true)]
        public string UriName { get; set; }

        [DataMember(IsRequired = false)]
        public string DirectoryName { get; set; }

        [DataMember(IsRequired = false)]
        public string DatabaseName { get; set; }

        [DataMember(IsRequired = false)]
        public DeployMode Mode { get; set; }

        #region Implementation of IExtensibleDataObject

        public ExtensionDataObject ExtensionData {get; set; }

        #endregion
    }

    [DataContract]
    public enum DeployMode
    {
        [EnumMember] Install = 0,
        [EnumMember] UpdateBin = 1,
        [EnumMember] UpdateAppData,
        [EnumMember] UpdateAllExceptConf,
        [EnumMember] UpdateAll
    }
}