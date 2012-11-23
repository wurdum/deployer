using System;
using System.Linq;
using Deployer.Service.Contract.Exceptions;
using Deployer.Service.Contract.Settings;
using Microsoft.Web.Administration;

namespace Deployer.Service.Core.Helpers.Validators
{
    public class IIS7Validator : Validator
    {
        public IIS7Validator(IDeploySettings appSettings) : base (appSettings) {
            Name = "IIS7 validator";
        }

        #region Overrides of Validator

        public override bool IsValidForInstall() {
            try {
                var root = _appSettings.GetIISRoot();
                var iis = new ServerManager();
                var site = iis.Sites.First(s => s.Name.Equals(root, StringComparison.InvariantCultureIgnoreCase));
                return !site.Applications.Any(a => a.Path.Equals(GetApplicationName()));
            } catch (Exception ex) {
                throw new IISValidationException(Name, ex);
            }
        }

        public override bool IsValidForUpdate() {
            return !IsValidForInstall();
        }

        #endregion

        private string GetApplicationName() {
            return "/" + _appSettings.SurveyName;
        }

        public static IIS7Validator Create(IDeploySettings appSettings) {
            return new IIS7Validator(appSettings);
        }
    }
}