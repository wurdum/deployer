using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Deployer.Client.DeployServiceReference;
using Ionic.Zip;
using Mono.Options;

namespace Deployer.Client
{
    public class DeployClientOptions
    {
        private readonly OptionSet _op;
        private ArgsSet _argsSet;
        private string _path,_uri, _mode, _directory, _database;
        private bool _showHelp;

        public DeployClientOptions() {
            _argsSet = new ArgsSet();
            _op = new OptionSet {
                { "h|help", "show help", o => _showHelp = o != null },
                { "i|install=", "install survey from specified filder", o => {
                        _path = o;
                        _argsSet._iIsSet = !string.IsNullOrEmpty(o);
                    } 
                },
                { "u|update=", "update survey from specified filder", o => {
                        _path = o;
                        _argsSet._uIsSet = !string.IsNullOrEmpty(o);
                    }
                },
                { "n|name=", "the name of survey uri", o => {
                        _uri = o.ToUpperInvariant();
                        _argsSet._nIsSet = !string.IsNullOrEmpty(o);
                    } 
                },
                { "m|updateMode=", "update mode [1=Update only bins, 2=Update only AppData, 3=Update all except configs (web.config, etc.), 4=Update all]", o => {
                        _mode = o;
                        _argsSet._mIsSet = !string.IsNullOrEmpty(o);
                    }
                },
                { "d|directory=", "the name of survey folder", o => _directory = o.ToUpperInvariant() },
                { "b|database=", "the nama of survey database", o => _database = o.ToUpperInvariant() },
            };
        }

        public ServiceData ParseArgs(string[] args) {
            try {
                var errorMsg = string.Empty;
                var deployMode = DeployMode.Install;
                var files = new byte[0];
                var unknownArgs =_op.Parse(args);

                if (!IsArgsEmpty(args, out errorMsg) ||
                    !IsArgsValid(out errorMsg) ||
                    !HasntUnknownArgs(unknownArgs, out errorMsg) ||
                    !IsArgsTextValid(new[] { _uri }, new[] { _directory, _database }, out errorMsg) ||
                    !IsPathValid(_path, out errorMsg) ||
                    !TryReadMode(_mode, out deployMode, out errorMsg) ||
                    !TryReadFiles(_path, out files)) {

                    Console.WriteLine(errorMsg);
                    _showHelp = true;
                }

                if (_showHelp)
                    return ShowHelp();

                return new ServiceData {
                    DeployMode = deployMode,
                    SurveyUri = _uri,
                    SurveyFiles = files,
                    SurveyDirectory = string.IsNullOrEmpty(_directory) ? _uri : _directory,
                    SurveyDatabase = string.IsNullOrEmpty(_database) ? _uri : _database
                };
            } catch (OptionException ex) {
                Console.WriteLine("Input error: " + ex.Message);
                return ShowHelp();
            } catch {
                Console.WriteLine("While arguments are processed unknown error has occured.");
                return ShowHelp();
            }
        }

        public class ServiceData
        {
            public DeployMode DeployMode { get; set; }
            public byte[] SurveyFiles { get; set; }
            public string SurveyUri { get; set; }
            public string SurveyDirectory { get; set; }
            public string SurveyDatabase { get; set; }
        }

        private ServiceData ShowHelp() {
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
            _op.WriteOptionDescriptions(Console.Out);

            return null;
        }

        private bool IsArgsEmpty(string[] args, out string errorMsg) {
            errorMsg = string.Empty;
            if (args == null || args.Length == 0) {
                errorMsg = "Please specify deploy arguments.";
                return false;
            }
            return true;
        }

        private bool HasntUnknownArgs(List<string> unknownArgs, out string errorMsg) {
            errorMsg = string.Empty;
            if (unknownArgs.Any()) {
                errorMsg = string.Format("Command promt contains unknown arguments: {0}", string.Join(" ", unknownArgs));
                return false;
            }
            return true;
        }

        private bool IsArgsValid(out string errorMsg) {
            errorMsg = string.Empty;
            if (_argsSet._iIsSet && _argsSet._nIsSet && !_argsSet._uIsSet && !_argsSet._mIsSet)
                return true;
            if (_argsSet._uIsSet && _argsSet._nIsSet && _argsSet._mIsSet && !_argsSet._iIsSet)
                return true;
            errorMsg = "Arguments was specified incorrectly.";
            return false;
        }

        private bool IsArgsTextValid(string[] notEmpty, string[] empty, out string errorMsg) {
            errorMsg = string.Empty;

            if (notEmpty.Any(string.IsNullOrEmpty)) {
                errorMsg = "Some of arguments is empty.";
                return false;
            }

            if (notEmpty.Union(empty).Any(a => !string.IsNullOrEmpty(a) && !Regex.IsMatch(a, @"^[a-zA-Z][a-zA-Z0-9_]*$"))) {
                errorMsg = "All names should starts from letter and contains only letters, character '_' and numbers.";
                return false;
            }

            return true;
        }

        private bool IsPathValid(string path, out string errorMsg) {
            errorMsg = string.Empty;
            try {
                if (!Directory.GetFiles(path, "Global.asax", SearchOption.AllDirectories).Any() ||
                    !Directory.GetDirectories(path, "App_Data").Any()) {
                    throw new ArgumentException("Wrong path to survey", path);
                }   
            } catch {
                errorMsg = "Specified path not contains the survey. Please check it.";
                return false;
            }
            return true;
        }

        private bool TryReadMode(string o, out DeployMode mode, out string errorMsg) {
            mode = DeployMode.Install;
            errorMsg = string.Empty;

            if (!string.IsNullOrEmpty(o)) {
                try {
                    var deploy = Convert.ToInt32(o);
                    mode = (DeployMode) deploy;

                    if (mode == DeployMode.Install)
                        throw new ArgumentException();

                    return true;
                } catch {
                    errorMsg = "'-m/updateMode' argument could has values [1,2,3,4].";
                    return false;
                }
            }

            if (_argsSet._uIsSet) {
                errorMsg = "In 'update' mode argument '-m/updateMode' should be specified.";
                return false;
            }

            return true;
        }

        private bool TryReadFiles(string path, out byte[] files) {
            files = new byte[0];
            var zipBufferMemory = new MemoryStream(1024 * 1024 * 4);
            using (var zipProjectDirectory = new ZipFile()) {
                try {
                    zipProjectDirectory.AddDirectory(path);
                    zipProjectDirectory.Save(zipBufferMemory);
                } catch (Exception ex) {
                    Console.WriteLine(ex.Message);
                    return false;
                }
            }

            if (zipBufferMemory.Length == 0) {
                Console.WriteLine("Folder was not read, please try again.");
                return false;
            }

            files = zipBufferMemory.ToArray();
            return true;
        }

        private struct ArgsSet
        {
            public bool _iIsSet;
            public bool _uIsSet;
            public bool _nIsSet;
            public bool _mIsSet;
        }
    }
}