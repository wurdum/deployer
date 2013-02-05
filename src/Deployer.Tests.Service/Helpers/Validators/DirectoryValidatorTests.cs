using System.IO;
using Moq;
using NUnit.Framework;
using Deployer.Service.Contract.Helpers;
using Deployer.Service.Contract.Settings;
using Deployer.Service.Core.Helpers.Validators;
using Deployer.Service.Core.Settings.Section;

namespace Deployer.Tests.Service.Helpers.Validators
{
    [TestFixture]
    public class DirectoryValidatorTestsBase
    {
        protected IConf _conf;
        protected IValidator _validator;

        [TestFixtureSetUp]
        public void CreateTestEnvironment() {
            var confSection = DeployerConfigurationSection.Instance;
            var surveyName = "TestSurvey" + TestUtils.GetPostfix();
            var pathToSurvey = Path.Combine(confSection.Settings.Paths.Surveys, surveyName);
            var iisManager = new Mock<IIISManager>();
            iisManager.Setup(m => m.SurveyPath).Returns(pathToSurvey);

            var mockedConf = new Mock<IConf>();
            mockedConf.Setup(s => s.CommonPaths).Returns(new CommonPaths(confSection.Settings.Paths.Surveys, null, null));
            mockedConf.Setup(s => s.SurveyPath).Returns(new SurveyPaths(pathToSurvey, null));
            mockedConf.Setup(s => s.IISManager).Returns(iisManager.Object);
            _conf = mockedConf.Object;

            Directory.CreateDirectory(_conf.CommonPaths.Surveys);
        }

        [TestFixtureTearDown]
        public void CleanTestEnvironment() {
            Directory.Delete(_conf.CommonPaths.Surveys, true);
        }

        [SetUp]
        public void CreateSurveyFolder() {
            _validator = DirectoryValidator.Create(_conf);
        }

        [TearDown]
        public void DeleteSurveyDolder() {
            _validator = null;
            if (Directory.Exists(_conf.SurveyPath.Survey))
                Directory.Delete(_conf.SurveyPath.Survey, true);
        }

        [Test]
        public void IsValidForInstallIfNoDir() {
            Assert.That(_validator.IsValidForInstall(), Is.True);
        }

        [Test]
        public void IsValidForInstallIfDirIsEmpty() {
            Directory.CreateDirectory(_conf.SurveyPath.Survey);

            Assert.That(_validator.IsValidForInstall(), Is.True);
        }

        [Test]
        public void IsNotValidForInstallIfDirContainsAnyFiles() {
            Directory.CreateDirectory(_conf.SurveyPath.Survey);
            CreateTextFileIn(_conf.SurveyPath.Survey);

            Assert.That(_validator.IsValidForInstall(), Is.False);
        }
        
        [Test]
        public void IsValidForUpdateIfDirContaintsFiles() {
            Directory.CreateDirectory(_conf.SurveyPath.Survey);
            CreateTextFileIn(_conf.SurveyPath.Survey);

            Assert.That(_validator.IsValidForUpdate(), Is.True);
        }

        [Test]
        public void IsNotValidIfDirNotContainsFiles() {
            Directory.CreateDirectory(_conf.SurveyPath.Survey);

            Assert.That(_validator.IsValidForUpdate(), Is.False);
        }

        [Test]
        public void IsNotValidIfDirNotExists() {
            Assert.That(_validator.IsValidForUpdate(), Is.False);
        }

        private void CreateTextFileIn(string path) {
            var textFile = File.CreateText(Path.Combine(path, "testfile.txt"));
            textFile.Close();
        }
    }
}