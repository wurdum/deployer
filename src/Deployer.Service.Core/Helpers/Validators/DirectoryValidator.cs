using System;
using System.IO;
using System.Linq;
using Deployer.Service.Contract.Exceptions;
using Deployer.Service.Contract.Settings;

namespace Deployer.Service.Core.Helpers.Validators
{
    public class DirectoryValidator : Validator
    {
        public DirectoryValidator(IConf conf) : base(conf) {
            Name = "Directory validator";
        }

        #region Overrides of Validator

        public override bool IsValidForInstall() {
            try {
                var surveyFolder = _conf.SurveyPath.Survey;
                return !(Directory.Exists(surveyFolder) && Directory.GetFiles(surveyFolder).Any());
            } catch(Exception ex) {
                throw new DirectoryValidationException(Name, ex);
            }
        }

        public override bool IsValidForUpdate() {
            try {
                var surveyFromIISMeta = _conf.IISManager.SurveyPath;
                return Directory.Exists(surveyFromIISMeta) && Directory.GetFiles(surveyFromIISMeta).Any(); // will work only at host
            } catch (Exception ex) {
                throw new DirectoryValidationException(Name, ex);
            }
        }

        #endregion

        public static DirectoryValidator Create(IConf appSettings) {
            return new DirectoryValidator(appSettings);
        }
    }
}