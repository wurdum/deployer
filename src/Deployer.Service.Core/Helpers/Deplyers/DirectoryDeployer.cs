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
        public DirectoryDeployer(IConf conf) : base(conf) {
            Name = "Directory";
        }

        #region Overrides of Deployer

        public override void Install() {
            try {
                _conf.PackageManager.UnpackZipIfOverwriteThrow(_conf.PackagePaths.Survey, _conf.SurveyPath.Survey);
                using (var connStrTemplate = File.OpenText(_conf.Templates.ConnectionString)) {
                    var connStrForSurvey = connStrTemplate.ReadToEnd().Replace("<%DbName%>", _conf.Survey.DbName);
                    var pathToSurveyConnStr = Path.Combine(_conf.SurveyPath.Survey, "ConnectionStrings.config");
                    using (var surveyConnStr = File.CreateText(pathToSurveyConnStr))
                        surveyConnStr.Write(connStrForSurvey);
                }

                CopyAllBinsFrom(_conf.Templates.AdditionalBins);

                foreach (var dir in _conf.IIS.DirsNeedIISAccess)
                    CreateDirWithIISAccess(dir.Key, dir.Value);
            } catch (Exception ex) {
                throw new DirectoryDeploymentException(Name, ex);
            }
        }

        public override void InstallRollback() {
            try {
                var surveyFolder = _conf.SurveyPath.Survey;
                if (Directory.Exists(surveyFolder) && Directory.GetFiles(surveyFolder).Any())
                    Directory.Delete(surveyFolder, true);
            } catch (Exception ex) {
                throw new RollbackException(Name, ex);
            }
        }

        public override void Update() {
            try {
                var surveyFolder = _conf.IISManager.SurveyPath;
                var surveyBackup = _conf.PackagePaths.Backup;
                _conf.PackageManager.FolderToZip(surveyFolder, surveyBackup);
                _conf.PackageManager.UnpackZipByRules(_conf.PackagePaths.Survey, surveyFolder, _conf.UpdateRules);
            } catch (Exception ex) {
                throw new DirectoryDeploymentException(Name, ex);
            }
        }

        public override void UpdateRollback() {
            try {
                var surveyFolder = _conf.IISManager.SurveyPath;
                var surveyBackup = _conf.PackagePaths.Backup;
                _conf.PackageManager.UnpackZipSilently(surveyBackup, surveyFolder);
            } catch (Exception ex) {
                throw new RollbackException(Name, ex);
            }
        }

        #endregion

        private void CopyAllBinsFrom(string additionalBinfFolder) {
            var additionalBins = additionalBinfFolder;
            foreach (var dirPath in Directory.GetDirectories(additionalBins, "*", SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(additionalBins, _conf.SurveyPath.GetBinDir()));

            foreach (var filePath in Directory.GetFiles(additionalBins, "*.*", SearchOption.AllDirectories))
                File.Copy(filePath, filePath.Replace(additionalBins, _conf.SurveyPath.GetBinDir()), true);
        }

        private void CreateDirWithIISAccess(string dirName, string iisUserName) {
            var exportDir = Directory.CreateDirectory(Path.Combine(_conf.SurveyPath.Survey, dirName));
            var exportDirSecurity = exportDir.GetAccessControl();
            exportDirSecurity.AddAccessRule(new FileSystemAccessRule(iisUserName, FileSystemRights.Modify,
                                                                     InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                                                                     PropagationFlags.None, AccessControlType.Allow));
            exportDir.SetAccessControl(exportDirSecurity);
        }

        public static DirectoryDeployer Create(IConf appSettings) {
            return new DirectoryDeployer(appSettings);
        }
    }
}