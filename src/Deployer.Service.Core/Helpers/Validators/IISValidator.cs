using System;
using Deployer.Service.Contract.Exceptions;
using Deployer.Service.Contract.Settings;

namespace Deployer.Service.Core.Helpers.Validators
{
    public class IISValidator : Validator
    {
        public IISValidator(IConf conf) : base (conf) {
            Name = "IIS validator";
        }

        #region Overrides of Validator

        public override bool IsValidForInstall() {
            try {
                return !_conf.IISManager.HasSurvey();
            } catch (Exception ex) {
                throw new IISValidationException(Name, ex);
            }
        }

        public override bool IsValidForUpdate() {
            return !IsValidForInstall();
        }

        #endregion

        public static IISValidator Create(IConf appSettings) {
            return new IISValidator(appSettings);
        }
    }
}