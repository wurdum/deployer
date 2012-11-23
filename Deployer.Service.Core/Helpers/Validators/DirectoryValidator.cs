using System;
using System.IO;
using System.Linq;
using Deployer.Service.Contract.Exceptions;
using Deployer.Service.Contract.Settings;

namespace Deployer.Service.Core.Helpers.Validators
{
    public class DirectoryValidator : Validator
    {
        public DirectoryValidator(IDeploySettings appSettings) : base(appSettings) {
            Name = "Directory validator";
        }

        #region Overrides of Validator

        public override bool IsValidForInstall() {
            try {
                var surveyFolder = _appSettings.SurveyPath;
                return !(Directory.Exists(surveyFolder) && Directory.GetFiles(surveyFolder).Any());
            } catch(Exception ex) {
                throw new DirectoryValidationException(Name, ex);
            }
        }

        public override bool IsValidForUpdate() {
            try {
                return Directory.Exists(_appSettings.IISSurveyPath) && Directory.GetFiles(_appSettings.IISSurveyPath).Any(); // will work only at host
            } catch (Exception ex) {
                throw new DirectoryValidationException(Name, ex);
            }
        }

        #endregion

        public static DirectoryValidator Create(IDeploySettings appSettings) {
            return new DirectoryValidator(appSettings);
        }
    }
}