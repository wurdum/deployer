using System.IO;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Web.Hosting;
using Deployer.Service.Core.Helpers;
using Deployer.Service.Core.Settings;
using Deployer.Service.Data;

namespace Deployer.Service.Core.Security
{
    public class UserNameValidator : UserNamePasswordValidator
    {
        private ServiceLogger _logger;

        public UserNameValidator() {
            _logger = new ServiceLogger();
        }

        public string SettingsFile {
            get { return Path.Combine(HostingEnvironment.ApplicationPhysicalPath, @"bin/DepSettings.xml"); }
        }

        #region Overrides of UserNamePasswordValidator

        public override void Validate(string userName, string password) {
            _logger.Info(string.Format("User '{0}' is trying to access to service", userName));
            var connString = DeploySettings.LoadSettings(SettingsFile)["DeployerConnectionString"];
            using (var context = new DbDataContext(connString))
                if (!context.Users.Any(u => u.Login.Equals(userName) && u.Password.Equals(password))) {
                    _logger.Info(string.Format("Access for user '{0}' is denied", userName));
                    throw new SecurityTokenValidationException("User validation is failed.");
                } else _logger.Info(string.Format("Access for user '{0}' is granted", userName));
        }

        #endregion
    }
}