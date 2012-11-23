using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using Deployer.Service.Contract.Contexts;
using Deployer.Service.Core.Helpers;
using Deployer.Service.Core.Helpers.Deplyers;
using Deployer.Tests.Stubs;
using Ionic.Zip;
using NUnit.Framework;

namespace Deployer.Tests.Deployers
{
    [TestFixture]
    public class DirectoryDeployerTests
    {
        private DeploySettingsStub _settings;
        private List<string> _filesInZip;

        [TestFixtureSetUp]
        public void CreateTestEnvironment() {
            var deployOptions = new DeployContext { UriName = "TestSurvey", DirectoryName = "TestSurvey", DatabaseName = "TestDb" };
            _settings = new DeploySettingsStub(SettingsFile.Path, deployOptions, DeployMode.Install, new PackageManager());

            _filesInZip = new List<string>();
            using (var zip = new ZipFile(_settings.SurveyPackagePath))
                _filesInZip.AddRange(zip.Select(file => file.FileName));

            Directory.CreateDirectory(_settings["SurveysFolder"]);
            Directory.CreateDirectory(_settings["SurveysBackupForlder"]);
            Console.WriteLine("Test environment has been created.");
        }

        [TestFixtureTearDown]
        public void CleanTestEnvironment() {
            Directory.Delete(_settings["SurveysFolder"], true);
            Directory.Delete(_settings["SurveysBackupForlder"], true);

            Console.WriteLine("Test environment has been cleaned.");
        }

        [TearDown]
        public void CleanSurveysFolder() {
            var surveysFolder = new DirectoryInfo(_settings["SurveysFolder"]);
            foreach (var subDir in surveysFolder.GetDirectories())
                subDir.Delete(true);

            Console.WriteLine("Surveys folder has been cleaned.");
        }

        [Test]
        public void DirectoryDeployerInstallTest() {
            var deployer = DirectoryDeployer.Create(_settings);

            deployer.Install();

            Assert.That(File.Exists(Path.Combine(_settings.SurveyPath, "web.config")), Is.True);
            Assert.That(File.Exists(Path.Combine(_settings.SurveyPath, "connectionstrings.config")), Is.True);
            Assert.That(File.Exists(Path.Combine(_settings.SurveyPath, @"bin\spssio32.dll")), Is.True);
            Assert.That(File.Exists(Path.Combine(_settings.SurveyPath, @"bin\system.web.mvc.dll")), Is.True);
            Assert.That(File.Exists(Path.Combine(_settings.SurveyPath, @"bin\win64\spssio64.dll")), Is.True);
            Assert.That(File.Exists(Path.Combine(_settings.SurveyPath, @"bin\win64\icudt38.dll")), Is.True);

            Assert.That(HasIISUserAccess("Export", "IIS_IUSRS"), Is.True);
            Assert.That(HasIISUserAccess("Exceptions", "IIS_IUSRS"), Is.True);
        }

        [Test]
        public void DirectoryDeployerInstallRollbackTest() {
            var deployer = DirectoryDeployer.Create(_settings);

            deployer.Install();
            deployer.InstallRollback();

            Assert.That(Directory.Exists(_settings.SurveyPath), Is.False);
        }

        [Test]
        public void DirectoryDeployerUpdateTest() {
            DirectoryDeployer.Create(_settings).Install();
            var updateSettings = new DeploySettingsStub(SettingsFile.Path, new DeployContext() {
                UriName = "TestSurveyUpd", 
                DirectoryName = "TestSurvey"
            }, DeployMode.UpdateAll, new PackageManager());
            DirectoryDeployer.Create(updateSettings).Update();

            Assert.That(File.Exists(updateSettings.SurveyBackupPackagePath), Is.True);
            Assert.That(Directory.Exists(Path.Combine(_settings.SurveyPath, "common")), Is.False);
            Assert.That(Directory.Exists(Path.Combine(_settings.SurveyPath, "data")), Is.False);
        }

        [Test]
        public void DirectoryDeployerUpdateRollbackTest() {
            DirectoryDeployer.Create(_settings).Install();
            var updateSettings = new DeploySettingsStub(SettingsFile.Path, new DeployContext() {
                UriName = "TestSurveyUpd", 
                DirectoryName = "TestSurvey"
            }, DeployMode.UpdateAll, new PackageManager());
            DirectoryDeployer.Create(updateSettings).Update();

            Assert.That(Directory.Exists(Path.Combine(_settings.SurveyPath, "common")), Is.False);
            Assert.That(Directory.Exists(Path.Combine(_settings.SurveyPath, "data")), Is.False);

            DirectoryDeployer.Create(updateSettings).UpdateRollback();

            Assert.That(File.Exists(updateSettings.SurveyBackupPackagePath), Is.True);
            Assert.That(Directory.Exists(Path.Combine(_settings.SurveyPath, "common")), Is.True);
            Assert.That(Directory.Exists(Path.Combine(_settings.SurveyPath, "data")), Is.True);
        }

        private bool HasIISUserAccess(string dir, string iisUserName) {
            var export = new DirectoryInfo(Path.Combine(_settings.SurveyPath, dir)).GetAccessControl().GetAccessRules(true, false, typeof (NTAccount));
            return export.Cast<FileSystemAccessRule>()
                .Any(r => (r.FileSystemRights & FileSystemRights.Modify) == FileSystemRights.Modify &&
                          r.IdentityReference.Value.EndsWith(iisUserName));
        }
    }
}