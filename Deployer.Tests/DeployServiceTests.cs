using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using Deployer.Service.Contract;
using Deployer.Service.Contract.Contexts;
using Deployer.Service.Contract.Data;
using Deployer.Service.Contract.Exceptions;
using Deployer.Service.Contract.Helpers;
using Deployer.Service.Contract.Settings;
using Deployer.Service.Core;
using Deployer.Tests.Stubs;
using Microsoft.Practices.Unity;
using Moq;
using NUnit.Framework;

namespace Deployer.Tests
{
    [TestFixture]
    public class DeployServiceTests
    {
        private IUnityContainer _container;
        private IServiceLogger _logger;
        private IDeployEvents _callback;
        private Mock<IDeploySession> _sessionMock;
        private DeployContext _deployContext;

        [TestFixtureSetUp]
        public void ResolveServiceDependencies() {
            _container = IocContainerStub.GetContainer();
            _deployContext = new DeployContext {DatabaseName = "Test", DirectoryName = "Test", UriName = "Test"};
            _logger = new ServiceLoggerStub();
            _callback = new CallbackChannelStub();
            _sessionMock = new Mock<IDeploySession>();
            _sessionMock.Setup(s => s.Open(It.IsAny<string>()));
            _sessionMock.Setup(s => s.Close());
        }

        [TestFixtureTearDown]
        public void DisposeContainer() {
            _container.Dispose();
        }

        [Test]
        public void ServiceShouldDemandOpenSessionMethod() {
            var factory = new Mock<IDeployServiceFactory>();
            factory.Setup(s => s.CreateCallbackObj()).Returns(_callback);
            factory.Setup(s => s.CreateLoggerObj()).Returns(_logger);
            var service = new DeployService(factory.Object);

            Assert.Throws<AuthenticationException>(() => service.Install(_deployContext, new byte[10]));
        }

        [Test]
        public void ServiceShouldLockUsedFolders() {
            var factory = new Mock<IDeployServiceFactory>();
            factory.Setup(f => f.CreateLoggerObj()).Returns(_logger);
            var service1 = new DeployService(factory.Object);
            var service2 = new DeployService(factory.Object);

            Assert.True(service1.OpenSession("folder1"));
            Assert.False(service2.OpenSession("folder1"));
            service1.Dispose();
            Assert.True(service2.OpenSession("folder1"));
        }

        [Test]
        [TestCase(DeployMode.Install)]
        [TestCase(DeployMode.UpdateAll)]
        public void ServiceShouldCallAllValidatorsAndDeployers(DeployMode mode) {
            var validators = GetMockedValidators(3).ToList();
            var deployers = GetMockedDeployers(3).ToList();

            var settingsMock = new Mock<IDeploySettings>();
            settingsMock.Setup(s => s.SurveyPath).Returns(@"d:\sms\folder1");
            settingsMock.Setup(s => s.PackageManager).Returns(new PackageManagerStub());
            settingsMock.Setup(s => s.DeployMode).Returns(mode);

            var factory = new Mock<IDeployServiceFactory>();
            factory.Setup(f => f.CreateLoggerObj()).Returns(_logger);
            factory.Setup(f => f.CreateSettingsObj(It.IsAny<string>(), It.IsAny<DeployContext>(), It.IsAny<DeployMode>(), It.IsAny<IPackageManager>())).Returns(settingsMock.Object);
            factory.Setup(f => f.CreateSessionObj(It.IsAny<IDeploySettings>(), It.IsAny<string>(), It.IsAny<string>())).Returns(_sessionMock.Object);
            factory.Setup(f => f.CreateCallbackObj()).Returns(_callback);
            factory.Setup(f => f.CreateDeployExHandlerObj(It.IsAny<IDeployEvents>(), It.IsAny<IDeploySettings>(), It.IsAny<IServiceLogger>(), It.IsAny<IDeploySession>()))
                .Returns(new Mock<IDeployExceptionHandler>().Object);
            factory.Setup(f => f.CreateValidatorsList(It.IsAny<IDeploySettings>())).Returns(validators.Select(v => v.Object));
            factory.Setup(f => f.CreateDeployersList(It.IsAny<IDeploySettings>())).Returns(deployers.Select(v => v.Object));

            var service = new DeployService(factory.Object);
            if (service.OpenSession("folder1"))
                if (mode == DeployMode.Install)
                    service.Install(_deployContext, new byte[100]);
                else
                    service.Update(_deployContext, DeployMode.UpdateAll, new byte[100]);

            foreach (var validator in validators)
                if (mode == DeployMode.Install)
                    validator.Verify(v => v.IsValidForInstall(), Times.Once());
                else
                    validator.Verify(v => v.IsValidForUpdate(), Times.Once());
            foreach (var deployer in deployers)
                if (mode == DeployMode.Install)
                    deployer.Verify(d => d.Install(), Times.Once());
                else
                    deployer.Verify(d => d.Update(), Times.Once());

            service.Dispose();
        }

        [Test]
        [TestCase(DeployMode.Install)]
        [TestCase(DeployMode.UpdateAll)]
        public void IfValidationFailedHadleExAndDontCallDeployers(DeployMode mode) {
            var validator = new Mock<IValidator>();
            validator.Setup(v => v.IsValidForInstall()).Returns(false);
            validator.Setup(v => v.IsValidForUpdate()).Returns(false);
            
            var deployer = new Mock<IDeployer>();
            deployer.Setup(d => d.Install());
            deployer.Setup(d => d.Update());

            var exceptionHandler = new Mock<IDeployExceptionHandler>();
            exceptionHandler.Setup(e => e.HandleValidation(It.IsAny<ValidationException>(), It.IsAny<IDeployEvents>()));

            var settingsMock = new Mock<IDeploySettings>();
            settingsMock.Setup(s => s.SurveyPath).Returns(@"d:\sms\folder1");
            settingsMock.Setup(s => s.PackageManager).Returns(new PackageManagerStub());
            settingsMock.Setup(s => s.DeployMode).Returns(mode);

            var factory = new Mock<IDeployServiceFactory>();
            factory.Setup(f => f.CreateLoggerObj()).Returns(_logger);
            factory.Setup(f => f.CreateSettingsObj(It.IsAny<string>(), It.IsAny<DeployContext>(), It.IsAny<DeployMode>(), It.IsAny<IPackageManager>())).Returns(settingsMock.Object);
            factory.Setup(f => f.CreateSessionObj(It.IsAny<IDeploySettings>(), It.IsAny<string>(), It.IsAny<string>())).Returns(_sessionMock.Object);
            factory.Setup(f => f.CreateCallbackObj()).Returns(_callback);
            factory.Setup(f => f.CreateDeployExHandlerObj(It.IsAny<IDeployEvents>(), It.IsAny<IDeploySettings>(), It.IsAny<IServiceLogger>(), It.IsAny<IDeploySession>()))
                .Returns(exceptionHandler.Object);
            factory.Setup(f => f.CreateValidatorsList(It.IsAny<IDeploySettings>())).Returns(new List<IValidator> {validator.Object});
            factory.Setup(f => f.CreateDeployersList(It.IsAny<IDeploySettings>())).Returns(new List<IDeployer> {deployer.Object});

            var service = new DeployService(factory.Object);
            if (service.OpenSession("folder1"))
                if (mode == DeployMode.Install)
                    service.Install(_deployContext, new byte[100]);
                else
                    service.Update(_deployContext, DeployMode.UpdateAll, new byte[100]);

            if (mode == DeployMode.Install)
                validator.Verify(v => v.IsValidForInstall(), Times.Once());
            else
                validator.Verify(v => v.IsValidForUpdate(), Times.Once());

            deployer.Verify(d => d.Install(), Times.Never());
            deployer.Verify(d => d.Update(), Times.Never());
            exceptionHandler.Verify(e => e.HandleValidation(It.IsAny<ValidationException>(), It.IsAny<IDeployEvents>()), Times.Once());

            service.Dispose();
        }

        [Test]
        [TestCase(DeployMode.Install)]
        [TestCase(DeployMode.UpdateAll)]
        public void IfDeployThrowsExShouldBeHandled(DeployMode mode) {
            var validator = new Mock<IValidator>();
            validator.Setup(v => v.IsValidForInstall()).Returns(true);
            validator.Setup(v => v.IsValidForUpdate()).Returns(true);

            var correctDeployers = GetMockedDeployers(3).ToList();
            var deployersWithEx = correctDeployers.ToList();
            var deployerWithEx = new Mock<IDeployer>();
            deployerWithEx.Setup(d => d.Name).Returns(string.Format("deployer-{0}", 4));
            deployerWithEx.Setup(d => d.Install()).Throws(new DeployException("deploy ex install"));
            deployerWithEx.Setup(d => d.Update()).Throws(new DeployException("deploy ex update"));
            deployersWithEx.Add(deployerWithEx);

            var settingsMock = new Mock<IDeploySettings>();
            settingsMock.Setup(s => s.SurveyPath).Returns(@"d:\sms\folder1");
            settingsMock.Setup(s => s.PackageManager).Returns(new PackageManagerStub());
            settingsMock.Setup(s => s.DeployMode).Returns(mode);

            var factory = new Mock<IDeployServiceFactory>();
            factory.Setup(f => f.CreateLoggerObj()).Returns(_logger);
            factory.Setup(f => f.CreateSettingsObj(It.IsAny<string>(), It.IsAny<DeployContext>(), It.IsAny<DeployMode>(), It.IsAny<IPackageManager>())).Returns(settingsMock.Object);
            factory.Setup(f => f.CreateSessionObj(It.IsAny<IDeploySettings>(), It.IsAny<string>(), It.IsAny<string>())).Returns(_sessionMock.Object);
            factory.Setup(f => f.CreateCallbackObj()).Returns(_callback);
            factory.Setup(f => f.CreateDeployExHandlerObj(It.IsAny<IDeployEvents>(), It.IsAny<IDeploySettings>(), It.IsAny<IServiceLogger>(), It.IsAny<IDeploySession>()))
                .Returns(new DeployExceptionHandlerStub());
            factory.Setup(f => f.CreateValidatorsList(It.IsAny<IDeploySettings>())).Returns(new List<IValidator> { validator.Object });
            factory.Setup(f => f.CreateDeployersList(It.IsAny<IDeploySettings>())).Returns(deployersWithEx.Select(d => d.Object));

            var service = new DeployService(factory.Object);
            if (service.OpenSession("folder1"))
                if (mode == DeployMode.Install)
                    service.Install(_deployContext, new byte[100]);
                else
                    service.Update(_deployContext, DeployMode.UpdateAll, new byte[100]);

            if (mode == DeployMode.Install)
                validator.Verify(v => v.IsValidForInstall(), Times.Once());
            else
                validator.Verify(v => v.IsValidForUpdate(), Times.Once());

            foreach (var deployer in correctDeployers)
                if (mode == DeployMode.Install) {
                    deployer.Verify(d => d.Install(), Times.Once());
                    deployer.Verify(d => d.InstallRollback(), Times.Once());
                } else {
                    deployer.Verify(d => d.Update(), Times.Once());
                    deployer.Verify(d => d.UpdateRollback(), Times.Once());
                }

            service.Dispose();
        }

        private IEnumerable<Mock<IValidator>> GetMockedValidators(int number) {
            for (var i = 0; i < number; i++) {
                var validator = new Mock<IValidator>();
                validator.Setup(v => v.Name).Returns(string.Format("validator-{0}", i));
                validator.Setup(v => v.IsValidForInstall()).Returns(true);
                validator.Setup(v => v.IsValidForUpdate()).Returns(true);
                yield return validator;
            }
        }

        private IEnumerable<Mock<IDeployer>> GetMockedDeployers(int number) {
            for (var i = 0; i < number; i++) {
                var deployer = new Mock<IDeployer>();
                deployer.Setup(d => d.Name).Returns(string.Format("deployer-{0}", i));
                deployer.Setup(d => d.Install());
                deployer.Setup(d => d.Update());
                yield return deployer;
            }
        }
    }
}