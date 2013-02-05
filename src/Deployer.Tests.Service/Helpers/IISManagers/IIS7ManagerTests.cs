using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using Microsoft.Web.Administration;
using NUnit.Framework;
using Deployer.Service.Contract.Helpers;
using Deployer.Service.Contract.Settings;
using Deployer.Service.Core.Helpers.IISManagers;
using Deployer.Service.Core.Settings.Section;

namespace Deployer.Tests.Service.Helpers.IISManagers
{
    [TestFixture]
    public class IIS7ManagerTests
    {
        private IIISManager _iisManager;
        private Credentials _credentials;
        private IISInfo _iisInfo;
        private string _surveyName;
        private string _surveyPath;

        [TestFixtureSetUp]
        public void EnvironmentSetUp() {
            var confSection = DeployerConfigurationSection.Instance;

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

            if (confSection.Settings.IIS.Version == "6")
                Assert.Ignore("No configuration for IIS7");

            if (!TryToImpersonate())
                Assert.Ignore("Imersonation to '{0}\\{1}' failed", _credentials.Domain, _credentials.Login);

            if (!TryConnectToIIS(confSection.Settings.IIS.Address))
                Assert.Ignore("Connection to '" + _iisInfo.Address + "' IIS7 site failed");
        }

        [TestFixtureTearDown]
        public void ReleaseEnvironment() {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        
        [SetUp]
        public void SetUp() {
            _iisManager = new IIS7Manager(_iisInfo, _surveyName);
        }

        [TearDown]
        public void TearDown() {
            _iisManager = null;
            DeleteSiteIfExists();
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
                CreateSite();

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
                CreateSite();

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

        private bool TryToImpersonate() {
            var impersonatedUser = string.Empty;

            try {
                using (Impersonate())
                    impersonatedUser = WindowsIdentity.GetCurrent().Name;
            } catch { }

            return impersonatedUser == string.Format("{0}\\{1}", _credentials.Domain, _credentials.Login);
        }

        private bool TryConnectToIIS(string address) {
            using (Impersonate()) {
                var iis = new ServerManager();
                Site site = null;
                try {
                    site = iis.Sites.FirstOrDefault(s => s.Name.Equals(address, StringComparison.InvariantCultureIgnoreCase));
                } catch {}

                return site != null;
            }
        }

        private void CreateSite() {
            using (Impersonate()) {
                var iis = new ServerManager();
                var root = iis.Sites.FirstOrDefault(s => s.Name.Equals(_iisInfo.Address, StringComparison.InvariantCultureIgnoreCase));
                var app = root.Applications.Add(GetAppPath(_surveyName), _surveyPath);
                app.ApplicationPoolName = _iisInfo.AppPool;
                iis.CommitChanges();
            }
        }

        private void DeleteSiteIfExists() {
            using (Impersonate()) {
                var iis = new ServerManager();
                var root = iis.Sites.FirstOrDefault(s => s.Name.Equals(_iisInfo.Address, StringComparison.InvariantCultureIgnoreCase));
                var app = root.Applications.FirstOrDefault(a => a.Path.Equals(GetAppPath(_surveyName), StringComparison.InvariantCultureIgnoreCase));
                if (app == null)
                    return;
                app.Delete();
                iis.CommitChanges();
            }
        }

        public bool SiteIsCreated() {
            var iis = new ServerManager();
            var root = iis.Sites.FirstOrDefault(s => s.Name.Equals(_iisInfo.Address, StringComparison.InvariantCultureIgnoreCase));
            return root.Applications.Any(a => a.Path.Equals(GetAppPath(_surveyName), StringComparison.InvariantCultureIgnoreCase));
        }

        private string GetAppPath(string surveyName) {
            return "/" + surveyName;
        }

        private Impersonator Impersonate() {
            return new Impersonator(_credentials.Domain, _credentials.Login, _credentials.Password);
        }
    }
}