using System;
using System.Linq;
using Deployer.Service.Core.Settings;
using Deployer.Service.Data;
using Deployer.Tests.Stubs;
using NUnit.Framework;
using Deployer.Service.Contract.Contexts;
using DeployMode = Deployer.Service.Contract.Contexts.DeployMode;

namespace Deployer.Tests.Settings
{
    [TestFixture]
    public class DeploySessionTests
    {
        private DeploySettingsStub _settings;
        private DeploySession _session;
        private string _user;
        private string _sessionKey;

        [TestFixtureSetUp]
        public void PrepareDeploySettings() {
            var deployOptions = new DeployContext { UriName = "Test" };
            _settings = new DeploySettingsStub(SettingsFile.Path, deployOptions, DeployMode.Install);
            _sessionKey = Guid.NewGuid().ToString();
            _user = GetTestUser("Test");
            _session = new DeploySession(_settings, _sessionKey, _user);

            Console.WriteLine("Test session has been created.");
        }

        [TestFixtureTearDown]
        public void RemoveTestData() {
            using (var context = _session.GetDataContext()) {
                var deploy = context.Deploys.Single(d => d.SessionKey == _settings.SessionKey);
                var user = context.Users.Single(u => u.Login == _user);
                context.Deploys.DeleteOnSubmit(deploy);
                context.Users.DeleteOnSubmit(user);
                context.SubmitChanges();
                Console.WriteLine("Test data have been cleaned.");
            }
        }

        [Test]
        public void SettingsLoadedCorrectly() {
            Assert.That(_settings.SurveyName, Is.EqualTo("Test"));
            Assert.That(_settings.DeployMode, Is.EqualTo(DeployMode.Install));
            Assert.That(_settings["Domain"], Is.EqualTo("Test"));
            Assert.IsNotEmpty(_settings["DeployerConnectionString"]);
        }

        [Test]
        public void DataContextNotEmpty() {
            using (var context = _session.GetDataContext()) {
                Assert.NotNull(context);
                Assert.That(context.DatabaseExists(), Is.True);
            }
        }

        [Test]
        public void DeploySessionFixationTest() {
            Assert.True(_settings.SessionKey == _sessionKey);

            using (var context = _session.GetDataContext()) {
                var deploy = context.Deploys.Single(d => d.SessionKey == _settings.SessionKey);
                Assert.Null(deploy.EndUtc);
                Assert.NotNull(deploy.StartUtc);
                Console.WriteLine("Session has been started.");
            }

            _session.Close();
            using (var context = _session.GetDataContext()) {
                var deploy = context.Deploys.Single(d => d.SessionKey == _settings.SessionKey);
                Assert.NotNull(deploy.EndUtc);
                Console.WriteLine("Session has been finished.");
            }
        }

        private string GetTestUser(string name) {
            using (var context = new DbDataContext(_settings["DeployerConnectionString"])) {
                context.Users.InsertOnSubmit(new User { Login = name, Password = name });
                context.SubmitChanges();
            }
            return name;
        }
    }
}