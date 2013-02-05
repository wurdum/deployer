using System.Configuration;
using System.Data.Linq;
using System.Linq;
using Moq;
using NUnit.Framework;
using Deployer.Service.Contract.Settings;
using Deployer.Service.Core.Settings;
using Deployer.Service.Data;
using DeployMode = Deployer.Service.Contract.Contexts.DeployMode;
using Exception = System.Exception;

namespace Deployer.Tests.Service.Settings
{
    [TestFixture]
    public class DeployAutoSesstionTests
    {
        private IConf _conf;
        private string _user;
        
        [SetUp]
        public void TestSetUp() {
            var mockedConf = new Mock<IConf>();
            mockedConf.Setup(s => s.ConnectionStrings).Returns(new ConnectionStrings(null, ConfigurationManager.ConnectionStrings["deployer"].ConnectionString));
            mockedConf.Setup(s => s.Survey).Returns(new Survey("Test", null, DeployMode.Install));
            mockedConf.Setup(s => s.SessionKey).Returns(TestUtils.GetPostfix(50));
            _conf = mockedConf.Object;
            _user = GetTestUser("Test" + TestUtils.GetPostfix());
        }

        [TearDown]
        public void RemoveTestData() {
            using (var context = new DbDataContext(_conf.ConnectionStrings.Deployer)) {
                var deploy = context.Deploys.Single(d => d.SessionKey == _conf.SessionKey);
                var user = context.Users.Single(u => u.Login == _user);
                context.Deploys.DeleteOnSubmit(deploy);
                context.Users.DeleteOnSubmit(user);
                context.SubmitChanges();
            }
        }

        [Test]
        public void DeploySessionAutoFixationTest() {
            using (var context = new DbDataContext(_conf.ConnectionStrings.Deployer)) {
                Deploy deploy;
                using (var session = new Session(_conf, _user)) {
                    deploy = context.Deploys.Single(d => d.SessionKey == _conf.SessionKey);
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
            using (var context = new DbDataContext(_conf.ConnectionStrings.Deployer)) {
                try {
                    using (var session = new Session(_conf, _user)) {
                        throw new Exception("Wow, something is happaned.");
                    }
                } catch (Exception) { }
                var deploy = context.Deploys.Single(d => d.SessionKey == _conf.SessionKey);
                Assert.NotNull(deploy);
                Assert.NotNull(deploy.EndUtc);
                Assert.That(deploy.Status, Is.EqualTo((int) DeployStatusModel.Failed));
            }
        }

        private string GetTestUser(string name) {
            using (var context = new DbDataContext(_conf.ConnectionStrings.Deployer)) {
                context.Users.InsertOnSubmit(new User { Login = name, Password = name });
                context.SubmitChanges();
            }
            return name;
        }
    }
}