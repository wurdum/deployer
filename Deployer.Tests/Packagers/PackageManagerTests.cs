using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Deployer.Service.Contract.Contexts;
using Deployer.Service.Core.Helpers;
using Deployer.Tests.Stubs;
using Ionic.Zip;
using NUnit.Framework;

namespace Deployer.Tests.Packagers
{
    [TestFixture]
    public class PackageManagerTests
    {
        private DeployContext _deployContext;
        private List<string> _filesInZip;
        private string _pathToFolderForDeploy;
        private string _pathToZipForInstall;
        private string _pathToZipForUpdateOnlyBin;
        private string _pathToZipForUpdateOnlyAppData;
        private string _pathToZipForUpdate;

        [TestFixtureSetUp]
        public void CreateTestEnvironment() {
            _deployContext = new DeployContext { UriName = "TestSurvey", DirectoryName = "TestSurvey"};
            var settings = new DeploySettingsStub(SettingsFile.Path, _deployContext, DeployMode.Install);

            var pathToZipsForTests = settings["UploadedZipFolder"];
            _pathToFolderForDeploy = settings["SurveysFolder"];
            _pathToZipForInstall = Path.Combine(pathToZipsForTests, "TestSurvey.zip");
            _pathToZipForUpdate = Path.Combine(pathToZipsForTests, "TestSurveyUpd.zip");
            _pathToZipForUpdateOnlyBin = Path.Combine(pathToZipsForTests, "TestSurveyUpdOnlyBin.zip");
            _pathToZipForUpdateOnlyAppData = Path.Combine(pathToZipsForTests, "TestSurveyUpdOnlyAppData.zip");

            _filesInZip = new List<string>();
            using (var zip = new ZipFile(_pathToZipForInstall))
                _filesInZip.AddRange(zip.Select(file => file.FileName));

            Directory.CreateDirectory(_pathToFolderForDeploy);
            Console.WriteLine("Test environment has been created.");
        }

        [TestFixtureTearDown]
        public void CleanTestEnvironment() {
            Directory.Delete(_pathToFolderForDeploy, true);

            Console.WriteLine("Test environment has been cleaned.");
        }

        [TearDown]
        public void CleanSurveysFolder() {
            var surveysFolder = new DirectoryInfo(_pathToFolderForDeploy);
            foreach (var file in surveysFolder.GetFiles())
                file.Delete();

            foreach (var subDir in surveysFolder.GetDirectories())
                subDir.Delete(true);

            Console.WriteLine("Surveys folder has been cleaned.");
        }

        [Test]
        public void BytesToZipTest() {
            var packageManager = new PackageManager();
            var settings = new DeploySettingsStub(SettingsFile.Path, _deployContext, DeployMode.Install);
            var tempZip = settings.SurveyPath + ".zip";

            using (var zipFileStream = new FileStream(_pathToZipForInstall, FileMode.Open, FileAccess.Read))
                packageManager.BytesToZip(ReadStreamToEnd(zipFileStream), tempZip);

            var filesInZipFromBytes = new List<string>();
            using (var zip = new ZipFile(tempZip))
                filesInZipFromBytes.AddRange(zip.Select(file => file.FileName));

            Assert.That(_filesInZip, Is.EquivalentTo(filesInZipFromBytes));
        }

        [Test]
        public void FolderToZipTest() {
            var packageManager = new PackageManager();
            var settings = new DeploySettingsStub(SettingsFile.Path, _deployContext, DeployMode.Install);
            var tempZip = settings.SurveyPath + ".zip";

            packageManager.UnpackZipSilently(_pathToZipForInstall, settings.SurveyPath);
            packageManager.FolderToZip(settings.SurveyPath, tempZip);

            var filesInNewZip = new List<string>();
            using (var zip = new ZipFile(tempZip))
                filesInNewZip.AddRange(zip.Select(file => file.FileName));

            Assert.That(_filesInZip, Is.EquivalentTo(filesInNewZip));
        }

        [Test]
        public void FolderCleanTest() {
            var settings = new DeploySettingsStub(SettingsFile.Path, _deployContext, DeployMode.UpdateAllExceptConf);
            var packageManager = new PackageManager();

            packageManager.UnpackZipIfOverwriteThrow(settings.SurveyPackagePath, settings.SurveyPath);
            packageManager.CleanFolder(settings.SurveyPath, settings.GetUpdateRules());

            Assert.True(File.Exists(Path.Combine(settings.SurveyPath, "web.config")));
            Assert.True(File.Exists(Path.Combine(settings.SurveyPath, "connectionstrings.config")));
            Assert.True(Directory.Exists(Path.Combine(settings.SurveyPath, "bin")));
            Assert.False(File.Exists(Path.Combine(settings.SurveyPath, @"views\web.config")));
            Assert.False(File.Exists(Path.Combine(settings.SurveyPath, @"customviews\web.config")));
            Assert.False(File.Exists(Path.Combine(settings.SurveyPath, @"common\parameters.cs")));
        }

        [Test]
        public void UnpackZipIfOverwriteThrowTest() {
            var packageManager = new PackageManager();
            var settings = new DeploySettingsStub(SettingsFile.Path, _deployContext, DeployMode.Install);

            Assert.DoesNotThrow(() => packageManager.UnpackZipIfOverwriteThrow(settings.SurveyPackagePath, settings.SurveyPath));
            Assert.Throws<ZipException>(() => packageManager.UnpackZipIfOverwriteThrow(settings.SurveyPackagePath, settings.SurveyPath));
        }

        [Test]
        public void UnpackZipByRulesOnlyBinTest() {
            var packageManager = new PackageManager();
            var settings = new DeploySettingsStub(SettingsFile.Path, _deployContext, DeployMode.UpdateBin);

            packageManager.UnpackZipIfOverwriteThrow(settings.SurveyPackagePath, settings.SurveyPath);
            packageManager.UnpackZipByRules(_pathToZipForUpdateOnlyBin, settings.SurveyPath, settings.GetUpdateRules());

            Assert.True(File.Exists(Path.Combine(settings.SurveyPath, "web.config")));
            Assert.True(File.Exists(Path.Combine(settings.SurveyPath, "connectionstrings.config")));
            Assert.True(File.Exists(Path.Combine(settings.SurveyPath, @"views\web.config")));
            Assert.True(File.Exists(Path.Combine(settings.SurveyPath, @"customviews\web.config")));
            Assert.True(File.Exists(Path.Combine(settings.SurveyPath, @"bin\spssio32.dll")));
            Assert.True(File.Exists(Path.Combine(settings.SurveyPath, @"bin\surveyEngine.configuration.dll")));
            Assert.True(File.Exists(Path.Combine(settings.SurveyPath, @"bin\system.web.mvc.dll")));
            Assert.True(File.Exists(Path.Combine(settings.SurveyPath, @"bin\win64\spssio64.dll")));
            Assert.False(File.Exists(Path.Combine(settings.SurveyPath, @"bin\codehelper.common.dll")));
        }

        [Test]
        public void UnpackZipByRulesOnlyAppDataTest() {
            var packageManager = new PackageManager();
            var settings = new DeploySettingsStub(SettingsFile.Path, _deployContext, DeployMode.UpdateAppData);

            packageManager.UnpackZipIfOverwriteThrow(settings.SurveyPackagePath, settings.SurveyPath);
            packageManager.UnpackZipByRules(_pathToZipForUpdateOnlyAppData, settings.SurveyPath, settings.GetUpdateRules());

            Assert.True(File.Exists(Path.Combine(settings.SurveyPath, "web.config")));
            Assert.True(File.Exists(Path.Combine(settings.SurveyPath, "connectionstrings.config")));
            Assert.True(Directory.Exists(Path.Combine(settings.SurveyPath, @"app_data\questionsdata")));
            Assert.True(File.Exists(Path.Combine(settings.SurveyPath, @"app_data\questionsdata\q4variants.txt")));
            Assert.False(File.Exists(Path.Combine(settings.SurveyPath, @"app_data\questionsdata\q1variants.txt")));
            Assert.False(File.Exists(Path.Combine(settings.SurveyPath, @"app_data\questionsdata\q2variants.txt")));
        }

        [Test]
        [TestCase(DeployMode.UpdateAll)]
        [TestCase(DeployMode.UpdateAllExceptConf)]
        public void UnpackZipByRulesTest(DeployMode surveyDeployMode) {
            var packageManager = new PackageManager();
            var settings = new DeploySettingsStub(SettingsFile.Path, _deployContext, surveyDeployMode);

            packageManager.UnpackZipIfOverwriteThrow(settings.SurveyPackagePath, settings.SurveyPath);
            packageManager.UnpackZipByRules(_pathToZipForUpdate, settings.SurveyPath, settings.GetUpdateRules());

            Assert.True(File.Exists(Path.Combine(settings.SurveyPath, "web.config")));
            Assert.True(File.Exists(Path.Combine(settings.SurveyPath, "connectionstrings.config")));
            Assert.True(File.Exists(Path.Combine(settings.SurveyPath, @"bin\spssio32.dll")));
            Assert.True(File.Exists(Path.Combine(settings.SurveyPath, @"bin\win64\spssio64.dll")));
            Assert.True(File.Exists(Path.Combine(settings.SurveyPath, @"app_data\white.csv")));
            Assert.False(Directory.Exists(Path.Combine(settings.SurveyPath, "common")));
            Assert.False(Directory.Exists(Path.Combine(settings.SurveyPath, "data")));

            var webConfText = File.ReadAllText(Path.Combine(settings.SurveyPath, "web.config"));

            if (surveyDeployMode == DeployMode.UpdateAll)
                Assert.That(webConfText, Is.EqualTo("Test"));
            else if (surveyDeployMode == DeployMode.UpdateAllExceptConf)
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