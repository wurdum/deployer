using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Hosting;
using Deployer.Service.Contract.Factories;
using Deployer.Service.Contract.Contexts;
using Deployer.Service.Contract.Helpers;
using Deployer.Service.Contract.Settings;
using Deployer.Service.Core.Settings.Section;

namespace Deployer.Service.Core.Settings
{
    public class Conf : IConf
    {
        protected readonly DeployContext _context;
        protected readonly UpdateRulesBuilder _updateRules;

        public Conf(string sessionKey, DeployContext context, IConfFactory factory) {
            if (sessionKey == null || sessionKey.Length >= 50)
                throw new ArgumentException("Session key should be no more than 50 characters", "sessionKey");

            var confSection = DeployerConfigurationSection.Instance;

            SessionKey = sessionKey;
            _context = context;
            _updateRules = new UpdateRulesBuilder(confSection);
            
            InitializeSettings(confSection);

            PackageManager = factory.CreatePackageManager();
            IISManager = factory.CreateIISManager(IIS, _context.UriName);
        }

        private void InitializeSettings(DeployerConfigurationSection confSection) {
            Credentials = new Credentials(
                confSection.Settings.Credentials.Domain,
                confSection.Settings.Credentials.Login,
                confSection.Settings.Credentials.Password);
            Survey = new Survey(_context.UriName, _context.DatabaseName ?? _context.UriName, _context.Mode);
            var surveyDir = Path.Combine(confSection.Settings.Paths.Surveys, _context.DirectoryName ?? _context.UriName);
            SurveyPath = new SurveyPaths(surveyDir, () => Path.Combine(surveyDir, "bin"));
            PackagePaths = new PackagePaths(
                Path.Combine(confSection.Settings.Paths.Uploads, _context.UriName + ".zip"),
                Path.Combine(confSection.Settings.Paths.Backups, _context.UriName + ".zip"));
            CommonPaths = new CommonPaths(
                confSection.Settings.Paths.Surveys, 
                confSection.Settings.Paths.Uploads, 
                confSection.Settings.Paths.Backups);
            DbScriptsPaths = new DbScriptsPaths(
                confSection.Settings.ScriptsPaths.DbCreate,
                confSection.Settings.ScriptsPaths.DbValidate,
                confSection.Settings.ScriptsPaths.DbDelete);
            Templates = new TemplatesPaths(
                confSection.Settings.Templates.ConnectionStrings,
                confSection.Settings.Paths.AdditionalResources.Bins);
            ConnectionStrings = new ConnectionStrings(
                ConfigurationManager.ConnectionStrings["master"].ConnectionString,
                ConfigurationManager.ConnectionStrings["deployer"].ConnectionString);
            IIS = new IISInfo(
                confSection.Settings.IIS.Version, 
                confSection.Settings.IIS.Address, 
                confSection.Settings.IIS.ApplicationPool,
                confSection.Settings.IIS.DirsWithIISAccess == null
                    ? new List<KeyValuePair<string, string>>()
                    : confSection.Settings.IIS.DirsWithIISAccess.Cast<DirElement>()
                        .Select(dir => new KeyValuePair<string, string>(dir.Name, dir.User)));
        }

        protected virtual string AppFolder {
            get { return GetApplicationPath(); }
        }

        public string SessionKey { get; protected set; }

        public IPackageManager PackageManager { get; protected set; }

        public IIISManager IISManager { get; protected set; }

        public Credentials Credentials { get; protected set; }

        public Survey Survey { get; protected set; }
        
        public SurveyPaths SurveyPath { get; protected set; }
        
        public PackagePaths PackagePaths { get; protected set; }

        public CommonPaths CommonPaths { get; set; }

        public DbScriptsPaths DbScriptsPaths { get; protected set; }
        
        public TemplatesPaths Templates { get; protected set; }

        public ConnectionStrings ConnectionStrings { get; protected set; }
        
        public IISInfo IIS { get; protected set; }

        public IEnumerable<IUpdateRule> UpdateRules {
            get { return _updateRules[Survey.DeployMode]; }
        }

        private static string GetApplicationPath() {
            return HostingEnvironment.ApplicationHost == null
                ? Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)
                : Path.Combine(HostingEnvironment.ApplicationPhysicalPath, "bin");
        }
    }
}