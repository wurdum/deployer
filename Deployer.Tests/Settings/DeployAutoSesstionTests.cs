using System;
using System.Data.Linq;
using System.Linq;
using Deployer.Service.Contract.Contexts;
using Deployer.Service.Core.Settings;
using Deployer.Service.Data;
using Deployer.Tests.Stubs;
using NUnit.Framework;
using DeployMode = Deployer.Service.Contract.Contexts.DeployMode;
using Exception = System.Exception;

namespace Deployer.Tests.Settings
{
    [TestFixture]
    public class DeployAutoSesstionTests
    {
        private DeploySettingsStub _settings;
        private string _user;
        private string _sessionKey;

        [TestFixtureSetUp]
        public void PrepareDeploySettings() {
            var deployOptions = new DeployContext { UriName = "Test" };
            _settings = new DeploySettingsStub(SettingsFile.Path, deployOptions, DeployMode.Install);

            Console.WriteLine("Test session has been created.");
        }

        [SetUp]
        public void TestSetUp() {
            _sessionKey = Guid.NewGuid().ToString();
            _user = GetTestUser("Test");
        }

        [TearDown]
        public void RemoveTestData() {
            using (var context = new DbDataContext(_settings["DeployerConnectionString"])) {
                var deploy = context.Deploys.Single(d => d.SessionKey == _sessionKey);
                var user = context.Users.Single(u => u.Login == _user);
                context.Deploys.DeleteOnSubmit(deploy);
                context.Users.DeleteOnSubmit(user);
                context.SubmitChanges();
                Console.WriteLine("Test data have been cleaned.");
            }
        }

        [Test]
        public void DeploySessionAutoFixationTest() {
            using (var context = new DbDataContext(_settings["DeployerConnectionString"])) {
                Deploy deploy;
                using (var session = new DeploySession(_settings, _sessionKey, _user)) {
                    deploy = context.Deploys.Single(d => d.SessionKey == _sessionKey);
                    Assert.That(deploy.Status, Is.EqualTo((int) DeployStatusModel.InProcess));
                    Assert.Null(deploy.EndUtc);
                    Assert.NotNull(deploy.StartUtc);
                    session.Commit();
                }
                context.Refresh(RefreshMode.OverwriteCurrentValues, context.Deploys);
                Assert.NotNull(deploy);
                Assert.NotNull(deploy.EndUtc);
                Assert.That(deploy.Status, Is.EqualTo((int) DeployStatusModel.Succeed));
            }
        }

        [Test]
        public void DeploySessionAutoFixationWithExceptionTest() {
            using (var context = new DbDataContext(_settings["DeployerConnectionString"])) {
                try {
                    using (var session = new DeploySession(_settings, _sessionKey, _user)) {
                        throw new Exception("Wow, something is happaned.");
                    }
                } catch (Exception) { }
                var deploy = context.Deploys.Single(d => d.SessionKey == _sessionKey);
                Assert.NotNull(deploy);
                Assert.NotNull(deploy.EndUtc);
                Assert.That(deploy.Status, Is.EqualTo((int) DeployStatusModel.Failed));
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