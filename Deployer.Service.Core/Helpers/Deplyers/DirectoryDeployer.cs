using System;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using Deployer.Service.Contract.Exceptions;
using Deployer.Service.Contract.Settings;

namespace Deployer.Service.Core.Helpers.Deplyers
{
    public class DirectoryDeployer : Deployer
    {
        public DirectoryDeployer(IDeploySettings appSettings) : base(appSettings) {
            Name = "Directory";
        }

        #region Overrides of Deployer

        public override void Install() {
            try {
                _appSettings.PackageManager.UnpackZipIfOverwriteThrow(_appSettings.SurveyPackagePath, _appSettings.SurveyPath);
                using (var connStrTemplate = File.OpenText(_appSettings["ConnStringTemplate"])) {
                    var connStrForSurvey = connStrTemplate.ReadToEnd().Replace("<%DbName%>", _appSettings.DbName);
                    var pathToSurveyConnStr = Path.Combine(_appSettings.SurveyPath, "ConnectionStrings.config");
                    using (var surveyConnStr = File.CreateText(pathToSurveyConnStr))
                        surveyConnStr.Write(connStrForSurvey);
                }

                CopyAllBinsFrom(_appSettings["AdditionalBinsFolder"]);
                CreateDirWithIISAccess("Export", "IIS_IUSRS");
                CreateDirWithIISAccess("Exceptions", "IIS_IUSRS");
            } catch (Exception ex) {
                throw new DirectoryDeploymentException(Name, ex);
            }
        }

        public override void InstallRollback() {
            try {
                var surveyFolder = _appSettings.SurveyPath;
                if (Directory.Exists(surveyFolder) && Directory.GetFiles(surveyFolder).Any())
                    Directory.Delete(surveyFolder, true);
            } catch (Exception ex) {
                throw new RollbackException(Name, ex);
            }
        }

        public override void Update() {
            try {
                var surveyFolder = _appSettings.IISSurveyPath;
                var surveyBackup = _appSettings.SurveyBackupPackagePath;
                _appSettings.PackageManager.FolderToZip(surveyFolder, surveyBackup);
                _appSettings.PackageManager.UnpackZipByRules(_appSettings.SurveyPackagePath, surveyFolder, _appSettings.GetUpdateRules());
            } catch (Exception ex) {
                throw new DirectoryDeploymentException(Name, ex);
            }
        }

        public override void UpdateRollback() {
            try {
                var surveyBackup = _appSettings.SurveyBackupPackagePath;
                var surveyFolder = _appSettings.IISSurveyPath;
                _appSettings.PackageManager.UnpackZipSilently(surveyBackup, surveyFolder);
            } catch (Exception ex) {
                throw new RollbackException(Name, ex);
            }
        }

        #endregion

        private void CopyAllBinsFrom(string additionalBinfFolder) {
            var additionalBins = additionalBinfFolder;
            foreach (var dirPath in Directory.GetDirectories(additionalBins, "*", SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(additionalBins, _appSettings.SurveyBinPath));

            foreach (var filePath in Directory.GetFiles(additionalBins, "*.*", SearchOption.AllDirectories))
                File.Copy(filePath, filePath.Replace(additionalBins, _appSettings.SurveyBinPath), true);
        }

        private void CreateDirWithIISAccess(string dirName, string iisUserName) {
            var exportDir = Directory.CreateDirectory(Path.Combine(_appSettings.SurveyPath, dirName));
            var exportDirSecurity = exportDir.GetAccessControl();
            exportDirSecurity.AddAccessRule(new FileSystemAccessRule(iisUserName, FileSystemRights.Modify,
                                                                     InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                                                                     PropagationFlags.None, AccessControlType.Allow));
            exportDir.SetAccessControl(exportDirSecurity);
        }

        public static DirectoryDeployer Create(IDeploySettings appSettings) {
            return new DirectoryDeployer(appSettings);
        }
    }
}