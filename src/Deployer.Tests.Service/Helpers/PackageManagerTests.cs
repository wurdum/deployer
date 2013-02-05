using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ionic.Zip;
using NUnit.Framework;
using Deployer.Service.Contract.Contexts;
using Deployer.Service.Contract.Helpers;
using Deployer.Service.Core.Helpers;
using Deployer.Service.Core.Settings;
using Deployer.Service.Core.Settings.Section;

namespace Deployer.Tests.Service.Helpers
{
    [TestFixture]
    public class PackageManagerTests
    {
        private DeployerConfigurationSection _confSection;
        private IPackageManager _packageManager;
        private string _surveyDir;

        private List<string> _filesInInstallZip;
        private string _pathToFolderForDeploy;
        private string _pathToZipForInstall;
        private string _pathToZipForUpdateOnlyBin;
        private string _pathToZipForUpdateOnlyAppData;
        private string _pathToZipForUpdate;

        [TestFixtureSetUp]
        public void CreateTestEnvironment() {
            _surveyDir = "TestSurvey" + TestUtils.GetPostfix();
            _confSection = DeployerConfigurationSection.Instance;
            _packageManager = new PackageManager();

            var pathToZipsForTests = _confSection.Settings.Paths.Uploads;
            _pathToFolderForDeploy = _confSection.Settings.Paths.Surveys;
            _pathToZipForInstall = Path.Combine(pathToZipsForTests, "TestSurvey.zip");
            _pathToZipForUpdate = Path.Combine(pathToZipsForTests, "TestSurveyUpd.zip");
            _pathToZipForUpdateOnlyBin = Path.Combine(pathToZipsForTests, "TestSurveyUpdOnlyBin.zip");
            _pathToZipForUpdateOnlyAppData = Path.Combine(pathToZipsForTests, "TestSurveyUpdOnlyAppData.zip");

            _filesInInstallZip = new List<string>();
            using (var zip = new ZipFile(_pathToZipForInstall) { UseUnicodeAsNecessary = true })
                _filesInInstallZip.AddRange(zip.Select(file => file.FileName));

            Directory.CreateDirectory(_pathToFolderForDeploy);
        }

        [TestFixtureTearDown]
        public void CleanTestEnvironment() {
            Directory.Delete(_pathToFolderForDeploy, true);

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        [TearDown]
        public void CleanSurveysFolder() {
            var surveysFolder = new DirectoryInfo(_pathToFolderForDeploy);
            foreach (var file in surveysFolder.GetFiles())
                file.Delete();

            foreach (var subDir in surveysFolder.GetDirectories())
                subDir.Delete(true);
        }

        [Test]
        public void BytesToZipTest() {
            var tempZip = Path.Combine(_confSection.Settings.Paths.Surveys, _surveyDir + ".zip");

            using (var zipFileStream = new FileStream(_pathToZipForInstall, FileMode.Open, FileAccess.Read))
                _packageManager.BytesToZip(ReadStreamToEnd(zipFileStream), tempZip);

            var filesInZipFromBytes = new List<string>();
            using (var zip = new ZipFile(tempZip) { UseUnicodeAsNecessary = true })
                filesInZipFromBytes.AddRange(zip.Select(file => file.FileName));

            CollectionAssert.AreEquivalent(_filesInInstallZip, filesInZipFromBytes);
        }

        [Test]
        public void FolderToZipTest() {
            var tempZip = Path.Combine(_confSection.Settings.Paths.Surveys, _surveyDir + ".zip");
            var pathToSurvey = Path.Combine(_confSection.Settings.Paths.Surveys, _surveyDir);

            _packageManager.UnpackZipSilently(_pathToZipForInstall, pathToSurvey);
            _packageManager.FolderToZip(pathToSurvey, tempZip);

            var filesInNewZip = new List<string>();
            using (var zip = new ZipFile(tempZip) { UseUnicodeAsNecessary = true })
                filesInNewZip.AddRange(zip.Select(file => file.FileName));

            CollectionAssert.AreEquivalent(_filesInInstallZip, filesInNewZip);
        }

        [Test]
        public void FolderCleanWithAllExceptConfDeployModeTest() {
            var pathToSurvey = Path.Combine(_confSection.Settings.Paths.Surveys, _surveyDir);
            var updateRulesBuilder = new UpdateRulesBuilder(_confSection);

            _packageManager.UnpackZipIfOverwriteThrow(_pathToZipForInstall, pathToSurvey);
            _packageManager.CleanFolder(pathToSurvey, updateRulesBuilder[DeployMode.UpdateAllExceptConf]);

            Assert.True(File.Exists(Path.Combine(pathToSurvey, "web.config")));
            Assert.True(File.Exists(Path.Combine(pathToSurvey, "connectionstrings.config")));
            Assert.True(Directory.Exists(Path.Combine(pathToSurvey, "bin")));
            Assert.False(File.Exists(Path.Combine(pathToSurvey, @"views\web.config")));
            Assert.False(File.Exists(Path.Combine(pathToSurvey, @"customviews\web.config")));
            Assert.False(File.Exists(Path.Combine(pathToSurvey, @"common\parameters.cs")));
        }

        [Test]
        public void UnpackZipIfOverwriteThrowTest() {
            var pathToSurvey = Path.Combine(_confSection.Settings.Paths.Surveys, _surveyDir);

            Assert.DoesNotThrow(() => _packageManager.UnpackZipIfOverwriteThrow(_pathToZipForInstall, pathToSurvey));
            Assert.Throws<ZipException>(() => _packageManager.UnpackZipIfOverwriteThrow(_pathToZipForInstall, pathToSurvey));
        }

        [Test]
        public void UnpackZipByRulesOnlyBinTest() {
            var pathToSurvey = Path.Combine(_confSection.Settings.Paths.Surveys, _surveyDir);
            var updateRulesBuilder = new UpdateRulesBuilder(_confSection);

            _packageManager.UnpackZipIfOverwriteThrow(_pathToZipForInstall, pathToSurvey);
            _packageManager.UnpackZipByRules(_pathToZipForUpdateOnlyBin, pathToSurvey, updateRulesBuilder[DeployMode.UpdateBin]);

            Assert.True(File.Exists(Path.Combine(pathToSurvey, "web.config")));
            Assert.True(File.Exists(Path.Combine(pathToSurvey, "connectionstrings.config")));
            Assert.True(File.Exists(Path.Combine(pathToSurvey, @"views\web.config")));
            Assert.True(File.Exists(Path.Combine(pathToSurvey, @"customviews\web.config")));
            Assert.True(File.Exists(Path.Combine(pathToSurvey, @"bin\spssio32.dll")));
            Assert.True(File.Exists(Path.Combine(pathToSurvey, @"bin\surveyEngine.configuration.dll")));
            Assert.True(File.Exists(Path.Combine(pathToSurvey, @"bin\system.web.mvc.dll")));
            Assert.True(File.Exists(Path.Combine(pathToSurvey, @"bin\win64\spssio64.dll")));
            Assert.False(File.Exists(Path.Combine(pathToSurvey, @"bin\codehelper.common.dll")));
        }

        [Test]
        public void UnpackZipByRulesOnlyAppDataTest() {
            var pathToSurvey = Path.Combine(_confSection.Settings.Paths.Surveys, _surveyDir);
            var updateRulesBuilder = new UpdateRulesBuilder(_confSection);

            _packageManager.UnpackZipIfOverwriteThrow(_pathToZipForInstall, pathToSurvey);
            _packageManager.UnpackZipByRules(_pathToZipForUpdateOnlyAppData, pathToSurvey, updateRulesBuilder[DeployMode.UpdateAppData]);

            Assert.True(File.Exists(Path.Combine(pathToSurvey, "web.config")));
            Assert.True(File.Exists(Path.Combine(pathToSurvey, "connectionstrings.config")));
            Assert.True(Directory.Exists(Path.Combine(pathToSurvey, @"app_data\questionsdata")));
            Assert.True(File.Exists(Path.Combine(pathToSurvey, @"app_data\questionsdata\q4variants.txt")));
            Assert.False(File.Exists(Path.Combine(pathToSurvey, @"app_data\questionsdata\q1variants.txt")));
            Assert.False(File.Exists(Path.Combine(pathToSurvey, @"app_data\questionsdata\q2variants.txt")));
        }

        [Test]
        [TestCase(DeployMode.UpdateAll)]
        [TestCase(DeployMode.UpdateAllExceptConf)]
        public void UnpackZipByRulesTest(DeployMode deployMode) {
            var pathToSurvey = Path.Combine(_confSection.Settings.Paths.Surveys, _surveyDir);
            var updateRulesBuilder = new UpdateRulesBuilder(_confSection);

            _packageManager.UnpackZipIfOverwriteThrow(_pathToZipForInstall, pathToSurvey);
            _packageManager.UnpackZipByRules(_pathToZipForUpdate, pathToSurvey, updateRulesBuilder[deployMode]);

            Assert.True(File.Exists(Path.Combine(pathToSurvey, "web.config")));
            Assert.True(File.Exists(Path.Combine(pathToSurvey, "connectionstrings.config")));
            Assert.True(File.Exists(Path.Combine(pathToSurvey, @"bin\spssio32.dll")));
            Assert.True(File.Exists(Path.Combine(pathToSurvey, @"bin\win64\spssio64.dll")));
            Assert.True(File.Exists(Path.Combine(pathToSurvey, @"app_data\white.csv")));
            Assert.False(Directory.Exists(Path.Combine(pathToSurvey, "common")));
            Assert.False(Directory.Exists(Path.Combine(pathToSurvey, "data")));

            var webConfText = File.ReadAllText(Path.Combine(pathToSurvey, "web.config"));

            if (deployMode == DeployMode.UpdateAll)
                Assert.That(webConfText, Is.EqualTo("Test"));
            else if (deployMode == DeployMode.UpdateAllExceptConf)
                Assert.That(webConfText, Is.Not.EqualTo("Test"));
        }

        public static byte[] ReadStreamToEnd(Stream stream) {
            var buffer = new byte[32768];
            var read = 0;

            int chunk;
            while ((chunk = stream.Read(buffer, read, buffer.Length - read)) > 0) {
                read += chunk;
                if (read != buffer.Length) continue;
                
                var nextByte = stream.ReadByte();
                if (nextByte == -1) return buffer;

                var expandedBuffer = new byte[buffer.Length * 2];
                Array.Copy(buffer, expandedBuffer, buffer.Length);
                expandedBuffer[read] = (byte) nextByte;
                buffer = expandedBuffer;
                read++;
            }
            
            
            var retrieved = new byte[read];
            Array.Copy(buffer, retrieved, read);
            return retrieved;
        }
    }
}