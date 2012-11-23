using System;
using System.Linq;
using Deployer.Service.Contract.Exceptions;
using Deployer.Service.Contract.Settings;
using Microsoft.Web.Administration;

namespace Deployer.Service.Core.Helpers.Deplyers
{
    public class IIS7Deployer : Deployer
    {
        public IIS7Deployer(IDeploySettings appSettings) : base(appSettings) {
            Name = "IIS7";
        }

        #region Overrides of Deployer

        public override void Install() {
            try {
                var root = _appSettings.GetIISRoot();
                var iis = new ServerManager();
                var site = iis.Sites.First(s => s.Name.Equals(root, StringComparison.InvariantCultureIgnoreCase));
                var app = site.Applications.Add(GetApplicationName(), _appSettings.SurveyPath);
                app.ApplicationPoolName = _appSettings["AppPool"];
                iis.CommitChanges();
            } catch (Exception ex) {
                throw new IISDeploymentException(Name, ex);
            }
        }

        public override void InstallRollback() {
            try {
                var root = _appSettings.GetIISRoot();
                var iis = new ServerManager();
                var site = iis.Sites.First(s => s.Name.Equals(root, StringComparison.InvariantCultureIgnoreCase));
                var app = site.Applications.FirstOrDefault(a => a.Path.Equals(GetApplicationName(), StringComparison.InvariantCultureIgnoreCase));
                if (app == null) return;
                app.Delete();
                iis.CommitChanges();
            } catch (Exception ex) {
                throw new RollbackException(Name, ex);
            }
        }

        public override void Update() {
            throw new NotImplementedException("There are no scenarios to update " + Name);
        }

        public override void UpdateRollback() {
            throw new NotImplementedException("There are no scenarios to update " + Name);
        }

        #endregion

        private string GetApplicationName() {
            return "/" + _appSettings.SurveyName;
        }

        public static IIS7Deployer Create(IDeploySettings appSettings) {
            return new IIS7Deployer(appSettings);
        }
    }
}