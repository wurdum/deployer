using Mono.Options;

namespace Deployer.Options
{
    public class DeployerOptionSet : OptionSet
    {
        public bool ShowHelp { get; set; }
        public string Path { get; set; }
        public SetType Type { get; set; }
        public string Uri { get; set; }
        public string Database { get; set; }
        public string Directory { get; set; }
        public string Mode { get; set; }

        public bool IsInstall {
            get {
                return Type == SetType.Install && !string.IsNullOrEmpty(Path) && !string.IsNullOrEmpty(Uri) && string.IsNullOrEmpty(Mode);
            }
        }

        public bool IsUpdate {
            get {
                return Type == SetType.Update && !string.IsNullOrEmpty(Path) && !string.IsNullOrEmpty(Uri) && !string.IsNullOrEmpty(Mode);
            }
        }

        public enum SetType { Install = 1, Update }
    }
}