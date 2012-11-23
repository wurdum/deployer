using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Hosting;
using System.Xml;
using System.Xml.Linq;
using Deployer.Service.Contract.Contexts;
using Deployer.Service.Contract.Helpers;
using Deployer.Service.Contract.Settings;
using Microsoft.Web.Administration;

namespace Deployer.Service.Core.Settings
{
    public class DeploySettings : IDeploySettings
    {
        private Dictionary<string, string> _settings;
        private readonly DeployContext _options;
        private readonly DeployMode _deployMode;
        private readonly IPackageManager _packageManager;
        private readonly UpdateRulesBuilder _updateRules;
        private readonly string _settingsFile;

        protected DeploySettings(string settingsFile, IPackageManager packageManager) {
            _settingsFile = Path.Combine(AppFolder, settingsFile);
            if (string.IsNullOrEmpty(_settingsFile) || !File.Exists(_settingsFile))
                throw new ArgumentException("Specified file with settings doesn't exists.", "settingsFile");
            
            _packageManager = packageManager;
            LoadSettings();
        }

        protected DeploySettings(string settingsFile, DeployContext options, IPackageManager packageManager)
            : this(settingsFile, packageManager) {
            _options = options;
        }

        public DeploySettings(string settingsFile, DeployContext options, DeployMode deployMode, IPackageManager packageManager)
            : this(settingsFile, options, packageManager) {
            _deployMode = deployMode;
            if (_deployMode != DeployMode.Install)
                _updateRules = new UpdateRulesBuilder(_settingsFile);
        }

        protected virtual string AppFolder {
            get { return GetApplicationPath(); }
        }

        public string SessionKey { get; set; }

        public string SurveyName { get { return _options.UriName; } }
        public string DbName { get { return _options.DatabaseName ?? _options.UriName; } }
        public DeployMode DeployMode { get { return _deployMode; } }
        public virtual IPackageManager PackageManager { get { return _packageManager; } }

        /// <summary>
        /// Path to folder where located survey files
        /// </summary>
        /// <value>C:\Surveys\AAA</value>
        public string SurveyPath {
            get { return Path.Combine(_settings["SurveysFolder"], _options.DirectoryName ?? _options.UriName); }
        }

        private string _iisSurveyPath;
        /// <summary>
        /// Path to folder where located survey files, which was
        /// extracted from IIS metadata
        /// </summary>
        /// <value>C:\Surveys\AAA</value>
        public virtual string IISSurveyPath {
            get { return _iisSurveyPath ?? (_iisSurveyPath = GetAppPathFromIIS()); }
        }

        public string SurveyBinPath {
            get { return Path.Combine(SurveyPath, "bin"); }
        }

        /// <summary>
        /// Path to uploaded zip with surveys files
        /// </summary>
        /// <value>C:\SurveysZips\AAA.zip</value>
        public string SurveyPackagePath {
            get { return Path.Combine(_settings["UploadedZipFolder"], _options.UriName + ".zip"); }
        }

        public string SurveyBackupPackagePath {
            get { return Path.Combine(_settings["SurveysBackupForlder"], _options.UriName + ".zip"); }
        }

        public virtual string this[string key] {
            get {
                string value;
                if (!_settings.TryGetValue(key, out value))
                    throw new ArgumentException("Key is out of range", "key");
                return value;
            }
        }

        public IEnumerable<IUpdateRule> GetUpdateRules() {
            return _updateRules.GetRules(DeployMode);
        }

        public string GetIISRoot() {
            return _settings["IISAdress"];
        }

        private string GetAppPathFromIIS() {
            var root = GetIISRoot();
            var iis = new ServerManager();
            var site = iis.Sites.FirstOrDefault(s => s.Name.Equals(root, StringComparison.InvariantCultureIgnoreCase));
            if (site == null)
                return string.Empty;
            var app = site.Applications.FirstOrDefault(a => a.Path.Equals("/" + SurveyName));
            return app == null ? string.Empty : app.VirtualDirectories["/"].PhysicalPath;
        }

        private void LoadSettings() {
            using (var reader = XmlReader.Create(_settingsFile)) {
                var xDocument = XDocument.Load(reader);
                _settings = xDocument.Root
                    .Elements().First(e => e.Name == "DeploySettings")
                    .Elements().ToDictionary(xe => xe.Attribute("name").Value, xe => xe.Value);
            }
        }

        private static string GetApplicationPath() {
            return HostingEnvironment.ApplicationHost == null
                ? Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)
                : Path.Combine(HostingEnvironment.ApplicationPhysicalPath, "bin");
        }

        public static Dictionary<string, string> LoadSettings(string settingsFile) {
            using (var reader = XmlReader.Create(Path.Combine(GetApplicationPath(), settingsFile))) {
                var xDocument = XDocument.Load(reader);
                return xDocument.Root
                    .Elements().First(e => e.Name == "DeploySettings")
                    .Elements().ToDictionary(xe => xe.Attribute("name").Value, xe => xe.Value);
            }
        }
    }
}