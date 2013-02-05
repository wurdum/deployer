using System.Collections.Generic;
using System.DirectoryServices;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Deployer.Service.Contract.Helpers;
using Deployer.Service.Contract.Settings;
using Deployer.Service.Core.Helpers.IISManagers;
using Deployer.Service.Core.Settings.Section;

namespace Deployer.Tests.Service.Helpers.IISManagers
{
    [TestFixture]
    public class IIS6ManagerTests
    {
        private const string NodeSchema = "IIsWebVirtualDir";
        private const bool InProcFlag = true;
        private const int AppIsolatedLvl = 2;

        private IIISManager _iisManager;
        private Credentials _credentials;
        private IISInfo _iisInfo;
        private string _surveyName;
        private string _surveyPath;

        [TestFixtureSetUp]
        public void EnvironmentSetUp() {
            var confSection = DeployerConfigurationSection.Instance;
            if (confSection.Settings.IIS.Version == "7")
                Assert.Ignore("No configuration for IIS6");

            _surveyName = "Test" + TestUtils.GetPostfix();
            _surveyPath = Path.Combine(confSection.Settings.Paths.Surveys, _surveyName);
            _credentials = new Credentials(
                confSection.Settings.Credentials.Domain, 
                confSection.Settings.Credentials.Login,
                confSection.Settings.Credentials.Password);
            _iisInfo = new IISInfo(
                confSection.Settings.IIS.Version,
                confSection.Settings.IIS.Address,
                confSection.Settings.IIS.ApplicationPool,
                null
            );

            if (!TryConnectToIIS(confSection.Settings.IIS.Address))
                Assert.Ignore("Connection to IIS6 server failed");
        }

        [SetUp]
        public void SetUp() {
            _iisManager = new IIS6Manager(_iisInfo, _surveyName);
        }

        [TearDown]
        public void TearDown() {
            _iisManager = null;
            DeleteSurveyIfExists();
        }

        [Test]
        public void HasSurveyForNotExistingSiteTest() {
            using (Impersonate()) {
                Assert.False(_iisManager.HasSurvey());
            }
        }

        [Test]
        public void HasSurveyForExistingSiteTest() {
            using (Impersonate()) {
                CreateSurvey();

                Assert.True(_iisManager.HasSurvey());
            }
        }

        [Test]
        public void CreateSurveyTest() {
            using (Impersonate()) {
                _iisManager.CreateSurvey(_surveyPath);

                Assert.IsTrue(SiteIsCreated());
            }
        }

        [Test]
        public void DeleteSurveyExistsTest() {
            using (Impersonate()) {
                CreateSurvey();

                _iisManager.DeleteSurvey();

                Assert.False(SiteIsCreated());
            }
        }

        [Test]
        public void DeleteNonExistingSiteThrows() {
            using (Impersonate()) {
                Assert.Throws<KeyNotFoundException>(() => _iisManager.DeleteSurvey());
            }
        }

        [Test]
        public void SurveyPathTest() {
            using (Impersonate()) {
                _iisManager.CreateSurvey(_surveyPath);

                Assert.AreEqual(_surveyPath, _iisManager.SurveyPath);
            }
        }

        [Test]
        public void SurveyPathThrowsOnNonExistingSurvey() {
            using (Impersonate()) {
                Assert.Throws<KeyNotFoundException>(() => {
                    var s = _iisManager.SurveyPath;
                });
            }
        }

        private bool TryConnectToIIS(string address) {
            return DirectoryEntry.Exists(address);
        }

        #region helpers

        private void CreateSurvey() {
            var site = new DirectoryEntry(_iisInfo.Address);
            var app = site.Children.Add(_surveyName, NodeSchema);
            app.Properties["Path"][0] = _surveyPath;
            app.Properties["AppPoolId"][0] = _iisInfo.AppPool;
            app.CommitChanges();
            app.Invoke("AppCreate", InProcFlag);
            app.Properties["AppIsolated"][0] = AppIsolatedLvl;
            app.Properties["AppFriendlyName"][0] = _surveyName;
            app.CommitChanges();
        }

        private void DeleteSurveyIfExists() {
            var appUri = PathCombine(_iisInfo.Address, _surveyName);
            if (!DirectoryEntry.Exists(appUri))
                return;

            var site = new DirectoryEntry(_iisInfo.Address);
            var app = site.Children.Cast<DirectoryEntry>().SingleOrDefault(vd => vd.Name == _surveyName && vd.Properties["KeyType"].Value.ToString().Trim() == NodeSchema);
            site.Invoke("Delete", new[] { app.SchemaClassName, _surveyName });
            site.CommitChanges();
        }

        public bool SiteIsCreated() {
            return DirectoryEntry.Exists(PathCombine(_iisInfo.Address, _surveyName));
        }

        private string PathCombine(params string[] paths) {
            var resultPath = Path.Combine(paths);
            return resultPath.Replace('\\', '/');
        }

        #endregion

        private Impersonator Impersonate() {
            return new Impersonator(_credentials.Domain, _credentials.Login, _credentials.Password);
        }
    }
}