using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Security.Cryptography;
using Deployer.Tests.Service.Stubs;
using Moq;
using NUnit.Framework;
using Deployer.Service.Contract;
using Deployer.Service.Contract.Contexts;
using Deployer.Service.Contract.Data;
using Deployer.Service.Contract.Exceptions;
using Deployer.Service.Contract.Factories;
using Deployer.Service.Contract.Helpers;
using Deployer.Service.Contract.Settings;
using Deployer.Service.Core;

namespace Deployer.Tests.Service
{
    [TestFixture]
    public class DeployServiceTests
    {
        private static IServiceLogger _logger = new ServiceLoggerStub();
        private static IDeployEvents _callback = new CallbackChannelStub();
        private static string _surveyName;
        private static string _surveyPath;

        private SendingFiles _files;

        [TestFixtureSetUp]
        public void PrepareEnvironment() {
            _logger = new ServiceLoggerStub();
            _callback = new CallbackChannelStub();
            _surveyName = "Test" + TestUtils.GetPostfix();
            _surveyPath = "D:\\sss\\" + _surveyName;

            _files = new SendingFiles(1024 * 1024 * 3);
        }

        [TestFixtureTearDown]
        public void ReleaseEnvironment() {
            _files = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        [Test]
        public void ServiceShouldDemandOpenSessionMethod() {
            var dsFacory = DefaultMocks.GetServiceFactory();

            var service = new DeployService(dsFacory.Object, null);

            Assert.Throws<AuthenticationException>(() => service.Deploy(GetDeployContext(DeployMode.Install), new byte[10]));
        }

        [Test]
        public void ServiceShouldLockUsedFolders() {
            var dsFactory = DefaultMocks.GetServiceFactory();

            using (var service1 = new DeployService(dsFactory.Object, null))
            using (var service2 = new DeployService(dsFactory.Object, null)) {
                Assert.True(service1.OpenSession(_surveyName));
                Assert.False(service2.OpenSession(_surveyName));
            }
        }

        [Test]
        public void ServiceShouldReleaseLockedFolders() {
            var dsFactory = DefaultMocks.GetServiceFactory();

            using (var service1 = new DeployService(dsFactory.Object, null)) {
                using (var service2 = new DeployService(dsFactory.Object, null)) {
                    service2.OpenSession(_surveyName);
                    Assert.False(service1.OpenSession(_surveyName));
                }

                Assert.True(service1.OpenSession(_surveyName));    
            }
        }

        [Test]
        public void CallDeployWithoutFilesWillFail() {
            var dsFactory = DefaultMocks.GetServiceFactory();
            var cFactory = DefaultMocks.GetConfFactory();

            Assert.Throws<InvalidOperationException>(() => {
                using (var service = new DeployService(dsFactory.Object, cFactory.Object)) {
                    service.OpenSession(_surveyName);
                    service.Deploy(GetDeployContext(), new byte[0]);
                }
            });
        }

        [Test]
        public void IfFilesTransferWasCorruptedDeployThrows() {
            var dsFactory = DefaultMocks.GetServiceFactory();
            var cFactory = DefaultMocks.GetConfFactory();

            Assert.Throws<ArgumentException>(() => {
                using (var service = new DeployService(dsFactory.Object, cFactory.Object)) {
                    service.OpenSession(_surveyName);

                    foreach (var chunk in _files.Chunks)
                        service.SendFilesChunk(chunk);

                    var spoiledFiles = new byte[_files.Bytes.Length];
                    Array.Copy(_files.Bytes, spoiledFiles, _files.Bytes.Length);
                    spoiledFiles[100] = 0;

                    service.Deploy(GetDeployContext(), MD5.Create().ComputeHash(spoiledFiles));
                }
            });
        }

        [Test]
        [TestCase(DeployMode.Install)]
        [TestCase(DeployMode.UpdateAll)]
        public void ServiceShouldCallAllValidatorsAndDeployers(DeployMode mode) {
            var cFactory = DefaultMocks.GetConfFactory();
            var validators = DefaultMocks.GetMockedValidators(3).ToList();
            var deployers = DefaultMocks.GetMockedDeployers(3).ToList();
            var dsFactory = DefaultMocks.GetServiceFactory(mode);
            dsFactory.Setup(f => f.CreateValidatorsList(It.IsAny<IConf>())).Returns(validators.Select(v => v.Object));
            dsFactory.Setup(f => f.CreateDeployersList(It.IsAny<IConf>())).Returns(deployers.Select(v => v.Object));

            using (var service = new DeployService(dsFactory.Object, cFactory.Object))
                ConsumeService(service, mode);

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
        }

        [Test]
        [TestCase(DeployMode.Install)]
        [TestCase(DeployMode.UpdateAll)]
        public void IfValidationFailedHadleExAndDontCallDeployers(DeployMode mode) {
            var cFactory = DefaultMocks.GetConfFactory();
            
            var validator = new Mock<IValidator>();
            validator.Setup(v => v.IsValidForInstall()).Returns(false);
            validator.Setup(v => v.IsValidForUpdate()).Returns(false);
            
            var deployer = new Mock<IDeployer>();
            deployer.Setup(d => d.Install());
            deployer.Setup(d => d.Update());
            
            var exceptionHandler = new Mock<IDeployExceptionHandler>();
            exceptionHandler.Setup(e => e.HandleValidation(It.IsAny<ValidationException>(), It.IsAny<IDeployEvents>()));

            var dsFactory = DefaultMocks.GetServiceFactory(mode);
            dsFactory.Setup(f => f.CreateDeployExHandlerObj(It.IsAny<IDeployEvents>(), It.IsAny<IConf>(), It.IsAny<IServiceLogger>(), It.IsAny<ISession>()))
                .Returns(exceptionHandler.Object);
            dsFactory.Setup(f => f.CreateValidatorsList(It.IsAny<IConf>())).Returns(new List<IValidator> {validator.Object});
            dsFactory.Setup(f => f.CreateDeployersList(It.IsAny<IConf>())).Returns(new List<IDeployer> {deployer.Object});

            using (var service = new DeployService(dsFactory.Object, cFactory.Object))
                ConsumeService(service, mode);

            if (mode == DeployMode.Install)
                validator.Verify(v => v.IsValidForInstall(), Times.Once());
            else
                validator.Verify(v => v.IsValidForUpdate(), Times.Once());

            deployer.Verify(d => d.Install(), Times.Never());
            deployer.Verify(d => d.Update(), Times.Never());
            exceptionHandler.Verify(e => e.HandleValidation(It.IsAny<ValidationException>(), It.IsAny<IDeployEvents>()), Times.Once());
        }

        [Test]
        [TestCase(DeployMode.Install)]
        [TestCase(DeployMode.UpdateAll)]
        public void IfDeployThrowsExShouldBeHandled(DeployMode mode) {
            var cFactory = DefaultMocks.GetConfFactory();

            var validator = new Mock<IValidator>();
            validator.Setup(v => v.IsValidForInstall()).Returns(true);
            validator.Setup(v => v.IsValidForUpdate()).Returns(true);

            var correctDeployers = DefaultMocks.GetMockedDeployers(3).ToList();
            var deployersWithEx = correctDeployers.ToList();
            var deployerWithEx = new Mock<IDeployer>();
            deployerWithEx.Setup(d => d.Name).Returns(string.Format("deployer-{0}", 4));
            deployerWithEx.Setup(d => d.Install()).Throws(new DeployException("deploy ex install"));
            deployerWithEx.Setup(d => d.Update()).Throws(new DeployException("deploy ex update"));
            deployersWithEx.Add(deployerWithEx);

            var settingsMock = new Mock<IConf>();
            settingsMock.Setup(s => s.SurveyPath).Returns(new SurveyPaths(_surveyPath, null));
            settingsMock.Setup(s => s.Survey).Returns(new Survey(null, null, mode));
            settingsMock.Setup(s => s.PackageManager).Returns(new PackageManagerStub());

            var dsFactory = DefaultMocks.GetServiceFactory(mode);
            dsFactory.Setup(f => f.CreateDeployExHandlerObj(It.IsAny<IDeployEvents>(), It.IsAny<IConf>(), It.IsAny<IServiceLogger>(), It.IsAny<ISession>()))
                .Returns(new DeployExceptionHandlerStub());
            dsFactory.Setup(f => f.CreateValidatorsList(It.IsAny<IConf>())).Returns(new List<IValidator> { validator.Object });
            dsFactory.Setup(f => f.CreateDeployersList(It.IsAny<IConf>())).Returns(deployersWithEx.Select(d => d.Object));

            using (var service = new DeployService(dsFactory.Object, cFactory.Object))
                ConsumeService(service, mode);

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
        }

        private void ConsumeService(IDeployService service, DeployMode mode) {
            if (!service.OpenSession(_surveyName))
                throw new InvalidOperationException("Session wasn't opened");

            foreach (var chunk in _files.Chunks)
                service.SendFilesChunk(chunk);

            service.Deploy(GetDeployContext(mode), MD5.Create().ComputeHash(_files.Bytes));
        }

        private DeployContext GetDeployContext(DeployMode mode = DeployMode.Install) {
            return new DeployContext {
                DatabaseName = _surveyName,
                DirectoryName = _surveyName,
                UriName = _surveyName,
                Mode = mode               
            };
        }

        private static class DefaultMocks
        {
            public static IEnumerable<Mock<IValidator>> GetMockedValidators(int number) {
                for (var i = 0; i < number; i++) {
                    var validator = new Mock<IValidator>();
                    validator.Setup(v => v.Name).Returns(string.Format("validator-{0}", i));
                    validator.Setup(v => v.IsValidForInstall()).Returns(true);
                    validator.Setup(v => v.IsValidForUpdate()).Returns(true);
                    yield return validator;
                }
            }

            public static IEnumerable<Mock<IDeployer>> GetMockedDeployers(int number) {
                for (var i = 0; i < number; i++) {
                    var deployer = new Mock<IDeployer>();
                    deployer.Setup(d => d.Name).Returns(string.Format("deployer-{0}", i));
                    deployer.Setup(d => d.Install());
                    deployer.Setup(d => d.Update());
                    yield return deployer;
                }
            }

            public static Mock<IConf> GetConf(DeployMode mode) {
                var confMock = new Mock<IConf>();
                confMock.Setup(s => s.SurveyPath).Returns(new SurveyPaths(_surveyPath, null));
                confMock.Setup(s => s.Survey).Returns(new Survey(null, null, mode));
                confMock.Setup(s => s.PackageManager).Returns(new PackageManagerStub());
                return confMock;
            }

            public static Mock<IDeployServiceFactory> GetServiceFactory(DeployMode mode = DeployMode.Install) {
                var sessionMock = new Mock<ISession>();
                sessionMock.Setup(s => s.Open());
                sessionMock.Setup(s => s.Close());

                var dsFactory = new Mock<IDeployServiceFactory>();
                dsFactory.Setup(f => f.CreateLoggerObj()).Returns(_logger);
                dsFactory.Setup(f => f.CreateConfObj(It.IsAny<string>(), It.IsAny<DeployContext>(), It.IsAny<IConfFactory>())).Returns(GetConf(mode).Object);
                dsFactory.Setup(f => f.CreateSessionObj(It.IsAny<IConf>(), It.IsAny<string>())).Returns(sessionMock.Object);
                dsFactory.Setup(f => f.CreateCallbackObj()).Returns(_callback);
                dsFactory.Setup(f => f.CreateDeployExHandlerObj(It.IsAny<IDeployEvents>(), It.IsAny<IConf>(), It.IsAny<IServiceLogger>(), It.IsAny<ISession>()))
                    .Returns(new Mock<IDeployExceptionHandler>().Object);
                return dsFactory;
            }

            public static Mock<IConfFactory> GetConfFactory() {
                var cFactory = new Mock<IConfFactory>();
                cFactory.Setup(f => f.CreatePackageManager()).Returns(new PackageManagerStub());
                return cFactory;
            }
        }

        private class SendingFiles
        {
            private static int _chunkSize = 1024*512;

            public SendingFiles(long size) {
                Bytes = new byte[size];
                new Random().NextBytes(Bytes);
                Chunks = GetChunks(Bytes, _chunkSize).ToArray();
            }

            public byte[] Bytes { get; private set; }
            public IEnumerable<byte[]> Chunks { get; private set; }

            private IEnumerable<byte[]> GetChunks(byte[] file, int chunkSize) {
                if (file.Length == 0)
                    yield break;

                var totalChunks = file.Length / chunkSize;
                for (var i = 0; i <= totalChunks; i++) {
                    var offset = i * chunkSize;
                    var lenght = Math.Min(chunkSize, file.Length - offset);

                    if (lenght == 0)
                        yield break;

                    var chunk = new byte[lenght];
                    Array.Copy(file, offset, chunk, 0, lenght);
                    yield return chunk;
                }
            }
        }
    }
}