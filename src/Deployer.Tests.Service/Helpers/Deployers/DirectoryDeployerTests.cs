using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using Moq;
using NUnit.Framework;
using Deployer.Service.Contract.Contexts;
using Deployer.Service.Contract.Settings;
using Deployer.Service.Contract.Helpers;
using Deployer.Service.Core.Helpers;
using Deployer.Service.Core.Helpers.Deplyers;
using Deployer.Service.Core.Settings.Section;

namespace Deployer.Tests.Service.Helpers.Deployers
{
    [TestFixture]
    public class DirectoryDeployerTests
    {
        private string _surveyDir;
        private IConf _conf;

        [TestFixtureSetUp]
        public void CreateTestEnvironment() {
            _surveyDir = "TestSurvey" + TestUtils.GetPostfix();
            _conf = CreateConfForPackage(_surveyDir);

            TestUtils.PreparePackageFor(DeployMode.Install, _conf.PackagePaths.Survey);
            Directory.CreateDirectory(_conf.CommonPaths.Surveys);
            Directory.CreateDirectory(_conf.CommonPaths.Backups);
        }

        [TestFixtureTearDown]
        public void CleanTestEnvironment() {
            Directory.Delete(_conf.CommonPaths.Surveys, true);
            Directory.Delete(_conf.CommonPaths.Backups, true);
            File.Delete(_conf.PackagePaths.Survey);

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        [TearDown]
        public void CleanSurveysFolder() {
            var surveysFolder = new DirectoryInfo(_conf.CommonPaths.Surveys);
            foreach (var subDir in surveysFolder.GetDirectories())
                subDir.Delete(true);
        }

        [Test]
        public void DirectoryDeployerInstallTest() {
            var deployer = DirectoryDeployer.Create(_conf);

            deployer.Install();

            Assert.That(File.Exists(Path.Combine(_conf.SurveyPath.Survey, "web.config")), Is.True);
            Assert.That(File.Exists(Path.Combine(_conf.SurveyPath.Survey, "connectionstrings.config")), Is.True);
            Assert.That(File.Exists(Path.Combine(_conf.SurveyPath.Survey, @"bin\spssio32.dll")), Is.True);
            Assert.That(File.Exists(Path.Combine(_conf.SurveyPath.Survey, @"bin\system.web.mvc.dll")), Is.True);
            Assert.That(File.Exists(Path.Combine(_conf.SurveyPath.Survey, @"bin\win64\spssio64.dll")), Is.True);
            Assert.That(File.Exists(Path.Combine(_conf.SurveyPath.Survey, @"bin\win64\icudt38.dll")), Is.True);

            foreach (var dir in _conf.IIS.DirsNeedIISAccess)
                Assert.That(HasIISUserAccess(dir.Key, dir.Value), Is.True);
        }

        [Test]
        public void DirectoryDeployerInstallRollbackTest() {
            var deployer = DirectoryDeployer.Create(_conf);

            deployer.Install();
            deployer.InstallRollback();

            Assert.That(Directory.Exists(_conf.SurveyPath.Survey), Is.False);
        }

        [Test]
        public void DirectoryDeployerUpdateTest() {
            DirectoryDeployer.Create(_conf).Install();
            var confForUpdate = CreateConfForPackage("TestSurveyUpd");
            DirectoryDeployer.Create(confForUpdate).Update();

            Assert.That(File.Exists(confForUpdate.PackagePaths.Backup), Is.True);
            Assert.That(Directory.Exists(Path.Combine(_conf.SurveyPath.Survey, "common")), Is.False);
            Assert.That(Directory.Exists(Path.Combine(_conf.SurveyPath.Survey, "data")), Is.False);
        }

        [Test]
        public void DirectoryDeployerUpdateRollbackTest() {
            DirectoryDeployer.Create(_conf).Install();
            var confForUpdate = CreateConfForPackage("TestSurveyUpd");
            var directoryDeployer = DirectoryDeployer.Create(confForUpdate);

            directoryDeployer.Update();
            directoryDeployer.UpdateRollback();

            Assert.That(File.Exists(confForUpdate.PackagePaths.Backup), Is.True);
            Assert.That(Directory.Exists(Path.Combine(_conf.SurveyPath.Survey, "common")), Is.True);
            Assert.That(Directory.Exists(Path.Combine(_conf.SurveyPath.Survey, "data")), Is.True);
        }

        private IConf CreateConfForPackage(string packageName) {
            var confSection = DeployerConfigurationSection.Instance;

            var pathToSurvey = Path.Combine(confSection.Settings.Paths.Surveys, _surveyDir);
            var survey = new Survey(null, _surveyDir, default(DeployMode));
            var surveyPaths = new SurveyPaths(pathToSurvey, () => Path.Combine(pathToSurvey, "bin"));
            var packagePaths = new PackagePaths(Path.Combine(confSection.Settings.Paths.Uploads, packageName + ".zip"),
                                                Path.Combine(confSection.Settings.Paths.Backups, packageName + ".zip"));
            var commonPaths = new CommonPaths(confSection.Settings.Paths.Surveys, confSection.Settings.Paths.Uploads,
                                              confSection.Settings.Paths.Backups);
            var templates = new TemplatesPaths(confSection.Settings.Templates.ConnectionStrings, confSection.Settings.Paths.AdditionalResources.Bins);
            var iisManager = new Mock<IIISManager>();
            iisManager.Setup(m => m.SurveyPath).Returns(surveyPaths.Survey);

            var mockedConf = new Mock<IConf>();
            mockedConf.Setup(s => s.Survey).Returns(survey);
            mockedConf.Setup(s => s.SurveyPath).Returns(surveyPaths);
            mockedConf.Setup(s => s.CommonPaths).Returns(commonPaths);
            mockedConf.Setup(s => s.PackagePaths).Returns(packagePaths);
            mockedConf.Setup(s => s.Templates).Returns(templates);
            mockedConf.Setup(s => s.PackageManager).Returns(new PackageManager());
            mockedConf.Setup(s => s.IISManager).Returns(iisManager.Object);
            mockedConf.Setup(s => s.IIS).Returns(new IISInfo(null, null, null, confSection.Settings.IIS.DirsWithIISAccess.Cast<DirElement>()
                .Select(dir => new KeyValuePair<string, string>(dir.Name, dir.User))));

            return mockedConf.Object;
        }

        private bool HasIISUserAccess(string dir, string iisUserName) {
            var export = new DirectoryInfo(Path.Combine(_conf.SurveyPath.Survey, dir)).GetAccessControl().GetAccessRules(true, false, typeof (NTAccount));
            return export.Cast<FileSystemAccessRule>()
                .Any(r => (r.FileSystemRights & FileSystemRights.Modify) == FileSystemRights.Modify &&
                          r.IdentityReference.Value.EndsWith(iisUserName));
        }
    }
}