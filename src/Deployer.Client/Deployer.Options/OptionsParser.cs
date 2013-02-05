using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Ionic.Zip;
using Mono.Options;
using Deployer.Client.DeployServiceReference;

namespace Deployer.Options
{
    public class OptionsParser
    {
        private readonly DeployerOptionSet _options;

        public OptionsParser() {
            _options = new DeployerOptionSet();
            _options.Add("h|help", "show help", o => _options.ShowHelp = o != null);
            _options.Add("i|install=", "install survey from specified filder", o => {
                _options.Path = o;
                _options.Type = DeployerOptionSet.SetType.Install;
            });
            _options.Add("u|update=", "update survey from specified filder", o => {
                _options.Path = o;
                _options.Type = DeployerOptionSet.SetType.Update;
            });
            _options.Add("n|name=", "the name of survey uri", o => {
                _options.Uri = o.ToUpperInvariant();
            });
            _options.Add("m|updateMode=", "update mode [1=Update only bins, 2=Update only AppData, 3=Update all except configs (web.config, etc.), 4=Update all]", o => {
                _options.Mode = o;
            });
            _options.Add("d|directory=", "the name of survey folder", o => _options.Directory = o.ToUpperInvariant());
            _options.Add("b|database=", "the nama of survey database", o => _options.Database = o.ToUpperInvariant());
        }

        public ServiceData Parse(string[] args) {
            try {
                if (args == null || args.Length == 0)
                    throw new OptionValidationException("Please specify deploy arguments");

                var unknownArgs = _options.Parse(args);
                if (unknownArgs.Any())
                    throw new OptionValidationException("Command promt contains unknown arguments: " + string.Join(" ", unknownArgs));

                ValidateOptions();

                var deployMode = GetMode(_options.Mode);
                var files = GetDirectoryPackage(_options.Path);
                
                return new ServiceData {
                    Mode = deployMode,
                    Uri = _options.Uri,
                    Files = files,
                    Directory = string.IsNullOrEmpty(_options.Directory) ? _options.Uri : _options.Directory,
                    Database = string.IsNullOrEmpty(_options.Database) ? _options.Uri : _options.Database
                };
            } catch (OptionValidationException ex) {
                return ShowHelp(ex.Message);
            } catch (OptionException ex) {
                return ShowHelp("Input error: " + ex.Message);
            } catch {
                return ShowHelp("While arguments are processed unknown error has occured.");
            }
        }

        private void ValidateOptions() {
            if (_options.ShowHelp)
                throw new OptionValidationException();

            if (string.IsNullOrEmpty(_options.Uri))
                throw new OptionValidationException("Some of required arguments are empty");

            if (new[] {_options.Uri, _options.Directory, _options.Database}.Any(a => !string.IsNullOrEmpty(a) && !Regex.IsMatch(a, @"^[a-zA-Z][a-zA-Z0-9_]*$")))
                throw new OptionValidationException("All text arguments should starts from letter and contains only letters, character '_' or numbers");

            if (!_options.IsInstall && !_options.IsUpdate)
                throw new OptionValidationException("The set of arguments was specified incorrectly");

            if (!Directory.GetFiles(_options.Path, "Global.asax", SearchOption.AllDirectories).Any() ||
                !Directory.GetDirectories(_options.Path, "App_Data").Any())
                throw new OptionValidationException("Specified path doesn't contain the survey. Please check it");
        }

        private ServiceData ShowHelp(string msg = null) {
            if (!string.IsNullOrEmpty(msg))
                Console.WriteLine(msg);

            var useCases = new[] {
                "{0} -i (path to sources) -n (uri name) [-d (dir name)] [-b (database name)]",
                "{0} -install=(path to sources) -name=(uri name) [-directory=(dir name)] [-databse=(database name)]",
                "{0} -u (path to sources) -u (update mode) -n (uri name)",
                "{0} -update=(path to sources) -mode=(update mode) -name=(uri name)"
            };

            Console.WriteLine();
            Console.WriteLine("This console app is client to service which helps to deploy survey to remote service over http.");
            Console.WriteLine("You could use two set of arguments to deploy your app:");
            useCases.ToList().ForEach(u => Console.WriteLine("\t" + u, Process.GetCurrentProcess().MainModule.ModuleName));
            Console.WriteLine();
            _options.WriteOptionDescriptions(Console.Out);

            return null;
        }

        private static DeployMode GetMode(string o) {
            var mode = DeployMode.Install;
            
            if (string.IsNullOrEmpty(o))
                return mode;

            try {
                var numericMode = Convert.ToInt32(o);
                mode = (DeployMode) numericMode;
            } catch {
                throw new OptionValidationException("'-m/updateMode' argument has unrecognized value");
            }

            var deployModes = Enum.GetValues(typeof(DeployMode)).Cast<DeployMode>();
            if (!deployModes.Contains(mode) || mode == DeployMode.Install)
                throw new OptionValidationException("'-m/updateMode' argument could has only [1,2,3,4] values");

            return mode;
        }

        private byte[] GetDirectoryPackage(string path) {
            var zipBufferMemory = new MemoryStream(1024 * 1024 * 4);
            using (var zipProjectDirectory = new ZipFile() {UseUnicodeAsNecessary = true}) {
                try {
                    zipProjectDirectory.AddDirectory(path);
                    zipProjectDirectory.Save(zipBufferMemory);
                } catch (Exception ex) {
                    throw new OptionValidationException(ex.Message);
                }
            }

            if (zipBufferMemory.Length == 0)
                throw new OptionValidationException("Dir was not read, please try again");

            return zipBufferMemory.ToArray();
        }
    }

    public class OptionValidationException : Exception
    {
        public OptionValidationException(string message) : base(message) {}
        public OptionValidationException() : this(string.Empty) {}
    }
}