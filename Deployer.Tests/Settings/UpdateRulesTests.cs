using System.Collections.Generic;
using System.Linq;
using Deployer.Service.Contract.Contexts;
using Deployer.Service.Contract.Settings;
using Deployer.Service.Core.Helpers;
using Deployer.Tests.Stubs;
using NUnit.Framework;

namespace Deployer.Tests.Settings
{
    [TestFixture]
    public class UpdateRulesTests
    {
        private readonly DeployContext _deployOptions = new DeployContext { UriName = "TestSurvey", DirectoryName = "TestSurvey", DatabaseName = "TestDb" };
        private readonly List<string> _neverUpdatable = new List<string> {
            "connectionstrings.config", "app_data/white.csv", "app_data/black.csv"
        };

        private readonly List<string> _surveyFiles = new List<string> {
            "Default.aspx", "Common/QuestionCollection.cs", "Data/SurveyDataProvider.cs"
        };

        private readonly List<string> _surveyBinFiles = new List<string> {
            "bin/SRV.dll", "bin/SurveyEngine.Core.dll"
        };

        private readonly List<string> _surveyDataFiles = new List<string> {
            "app_data/questionsdata/Q1Rows.txt", "app_data/questionsdata/Questions.txt"
        };

        [Test]
        public void RulesForUpdateAllTest() {
            var updateRules = GetUpdateRulesFor(DeployMode.UpdateAll);

            Assert.True(_surveyFiles.All(updateRules.SatisfiesForUpdate));
            Assert.True(_surveyBinFiles.All(updateRules.SatisfiesForUpdate));
            Assert.True(_surveyDataFiles.All(updateRules.SatisfiesForUpdate));
            Assert.True(_neverUpdatable.All(f => !updateRules.SatisfiesForUpdate(f)));
        }

        [Test]
        public void RulesForUpdateAllExceptConfigTest() {
            var updateRules = GetUpdateRulesFor(DeployMode.UpdateAllExceptConf);

            Assert.True(_surveyFiles.All(updateRules.SatisfiesForUpdate));
            Assert.True(_surveyBinFiles.All(updateRules.SatisfiesForUpdate));
            Assert.True(_surveyDataFiles.All(updateRules.SatisfiesForUpdate));
            Assert.True(!updateRules.SatisfiesForUpdate("web.config"));
            Assert.True(_neverUpdatable.All(f => !updateRules.SatisfiesForUpdate(f)));
        }

        [Test]
        public void RulesForOnlyAppDataTest() {
            var updateRules = GetUpdateRulesFor(DeployMode.UpdateAppData);

            Assert.True(_surveyDataFiles.All(updateRules.SatisfiesForUpdate));
            Assert.True(_surveyFiles.All(f => !updateRules.SatisfiesForUpdate(f)));
            Assert.True(_surveyBinFiles.All(f => !updateRules.SatisfiesForUpdate(f)));
            Assert.True(!updateRules.SatisfiesForUpdate("web.config"));
            Assert.True(_neverUpdatable.All(f => !updateRules.SatisfiesForUpdate(f)));
        }

        [Test]
        public void RulesOnlyBinTest() {
            var updateRules = GetUpdateRulesFor(DeployMode.UpdateBin);

            Assert.True(_surveyBinFiles.All(updateRules.SatisfiesForUpdate));
            Assert.True(_surveyFiles.All(f => !updateRules.SatisfiesForUpdate(f)));
            Assert.True(_surveyDataFiles.All(f => !updateRules.SatisfiesForUpdate(f)));
            Assert.True(!updateRules.SatisfiesForUpdate("web.config"));
            Assert.True(_neverUpdatable.All(f => !updateRules.SatisfiesForUpdate(f)));
        }

        private IEnumerable<IUpdateRule> GetUpdateRulesFor(DeployMode deployMode) {
            var settings = new DeploySettingsStub(SettingsFile.Path, _deployOptions, deployMode);
            var updateRules = settings.GetUpdateRules();
            return updateRules;
        }
    }
}