using Deployer.Service.Contract.Helpers;
using Deployer.Service.Contract.Settings;
using Deployer.Service.Core.Helpers.Deplyers;
using Moq;
using NUnit.Framework;

namespace Deployer.Tests.Service.Helpers.Deployers
{
    [TestFixture]
    public class IISDeployerTest
    {
        private Mock<IIISManager> _mockedIISManager; 
        private IConf _conf;

        [SetUp]
        public void SetUp() {
            _mockedIISManager = new Mock<IIISManager>();
            _mockedIISManager.Setup(m => m.CreateSurvey(It.IsAny<string>()));
            _mockedIISManager.Setup(m => m.DeleteSurvey());

            var mockedConf = new Mock<IConf>();
            mockedConf.Setup(c => c.IISManager).Returns(_mockedIISManager.Object);
            mockedConf.Setup(c => c.SurveyPath).Returns(new SurveyPaths(null, null));

            _conf = mockedConf.Object;
        }

        [Test]
        public void SurveyInstallTest() {
            IISDeployer.Create(_conf).Install();

            _mockedIISManager.Verify(m => m.CreateSurvey(It.IsAny<string>()), Times.Once());
        }

        [Test]
        public void SurveyRollbackTest() {
            IISDeployer.Create(_conf).InstallRollback();

            _mockedIISManager.Verify(m => m.DeleteSurvey(), Times.Once());
        }
    }
}