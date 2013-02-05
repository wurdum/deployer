using Moq;
using NUnit.Framework;
using Deployer.Service.Contract.Helpers;
using Deployer.Service.Contract.Settings;
using Deployer.Service.Core.Helpers.Validators;

namespace Deployer.Tests.Service.Helpers.Validators
{
    [TestFixture]
    public class IISValidatorTests
    {
        private Mock<IIISManager> _mockedIISManager;
        private IISValidator _validator;

        [SetUp]
        public void SetUp() {
            _mockedIISManager = new Mock<IIISManager>();
            var mockedConf = new Mock<IConf>();
            mockedConf.Setup(c => c.IISManager).Returns(_mockedIISManager.Object);
            _validator = new IISValidator(mockedConf.Object);
        }

        [TearDown]
        public void TearDown() {
            _mockedIISManager = null;
            _validator = null;
        }

        [Test]
        public void VaidationIsSucceeded() {
            _mockedIISManager.Setup(m => m.HasSurvey()).Returns(false);

            Assert.True(_validator.IsValidForInstall());
            Assert.False(_validator.IsValidForUpdate());
        }

        [Test]
        public void VaidationIsFailed() {
            _mockedIISManager.Setup(m => m.HasSurvey()).Returns(true);

            Assert.False(_validator.IsValidForInstall());
            Assert.True(_validator.IsValidForUpdate());
        }
    }
}