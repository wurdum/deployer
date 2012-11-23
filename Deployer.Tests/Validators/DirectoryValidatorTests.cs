using System;
using System.IO;
using Deployer.Service.Contract.Contexts;
using Deployer.Service.Core.Helpers.Validators;
using Deployer.Tests.Stubs;
using NUnit.Framework;

namespace Deployer.Tests.Validators
{
    [TestFixture]
    public class DirectoryValidatorTests
    {
        private DeploySettingsStub _settings;

        [TestFixtureSetUp]
        public void CreateTestEnvironment() {
            var deployOptions = new DeployContext {DirectoryName = "TestSurvey"};
            _settings = new DeploySettingsStub(SettingsFile.Path, deployOptions, DeployMode.Install);
            Directory.CreateDirectory(_settings["SurveysFolder"]);

            Console.WriteLine("Test environment has been created.");
        }

        [TestFixtureTearDown]
        public void CleanTestEnvironment() {
            Directory.Delete(_settings["SurveysFolder"], true);
            
            Console.WriteLine("Test environment has been cleaned.");
        }

        [Test]
        public void InstallValidationTest() {
            var validator = DirectoryValidator.Create(_settings);

            Assert.That(validator.IsValidForInstall(), Is.True);
            Directory.CreateDirectory(_settings.SurveyPath);
            Assert.That(validator.IsValidForInstall(), Is.True);
            CreateTextFileIn(_settings.SurveyPath);
            Assert.That(validator.IsValidForInstall(), Is.False);
            Directory.Delete(_settings.SurveyPath, true);
        }

        [Test]
        public void UpdateValidationTest() {
            var validator = DirectoryValidator.Create(_settings);

            Assert.That(validator.IsValidForUpdate(), Is.False);
            Directory.CreateDirectory(_settings.SurveyPath);
            Assert.That(validator.IsValidForUpdate(), Is.False);
            CreateTextFileIn(_settings.SurveyPath);
            Assert.That(validator.IsValidForUpdate(), Is.True);
            Directory.Delete(_settings.SurveyPath, true);
        }

        private void CreateTextFileIn(string path) {
            var textFile = File.CreateText(Path.Combine(path, "testfile.txt"));
            textFile.Close();
        }
    }
}