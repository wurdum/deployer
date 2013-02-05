using System.Configuration;
using System.Linq;
using Moq;
using NUnit.Framework;
using Deployer.Service.Contract.Settings;
using Deployer.Service.Core.Settings;
using Deployer.Service.Data;
using DeployMode = Deployer.Service.Contract.Contexts.DeployMode;

namespace Deployer.Tests.Service.Settings
{
    [TestFixture]
    public class DeploySessionTests
    {
        private IConf _conf;
        private string _user;

        [SetUp]
        public void PrepareDeploySettings() {
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
        public void DataContextNotEmpty() {
            using (var session = new Session(_conf, _user))
            using (var context = session.GetDataContext()) {
                Assert.NotNull(context);
                Assert.That(context.DatabaseExists(), Is.True);
            }
        }

        [Test]
        public void DeploySessionFixationTest() {
            var session = new Session(_conf, _user);
            using (var context = new DbDataContext(_conf.ConnectionStrings.Deployer)) {
                var deploy = context.Deploys.Single(d => d.SessionKey == _conf.SessionKey);
                Assert.Null(deploy.EndUtc);
                Assert.NotNull(deploy.StartUtc);
            }

            session.Close();

            using (var context = new DbDataContext(_conf.ConnectionStrings.Deployer)) {
                var deploy = context.Deploys.Single(d => d.SessionKey == _conf.SessionKey);
                Assert.NotNull(deploy.EndUtc);
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