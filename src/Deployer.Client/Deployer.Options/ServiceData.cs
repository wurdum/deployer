using System;
using Deployer.Client.DeployServiceReference;

namespace Deployer.Options
{
    public class ServiceData : IEquatable<ServiceData>
    {
        public DeployMode Mode { get; set; }
        public byte[] Files { get; set; }
        public string Uri { get; set; }
        public string Directory { get; set; }
        public string Database { get; set; }

        #region equality members

        public bool Equals(ServiceData other) {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return Mode == other.Mode && String.Equals(Uri, other.Uri) &&
                   String.Equals(Directory, other.Directory) && String.Equals(Database, other.Database);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != this.GetType())
                return false;
            return Equals((ServiceData) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (int) Mode;
                hashCode = (hashCode*397) ^ (Uri != null ? Uri.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (Directory != null ? Directory.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (Database != null ? Database.GetHashCode() : 0);
                return hashCode;
            }
        }

        #endregion
            
        public override string ToString() {
            return String.Format("DeployMode: {0}, SurveyUri: {1}, SurveyDirectory: {2}, SurveyDatabase: {3}", Mode, Uri, Directory, Database);
        }
    }
}