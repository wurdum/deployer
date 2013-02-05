using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Deployer.Client.DeployServiceReference;
using Deployer.Options;
using Ionic.Zip;
using NUnit.Framework;

namespace Deployer.Tests.Client
{
    [TestFixture]
    public class OptionsReaderTests
    {
        private const string SurveyDirName = "Survey";
        private const string EmptyDirName = "Empty";
        private const string TestSurveyPackage = "Resources\\TestSurvey.zip";

        protected string _surveyDir;
        protected string _emptyDir;
        protected string _testsExecutionLocation;

        [TestFixtureSetUp]
        public void PrepareSurveyForClient() {
            ReloadSurveyDirs();

            if (Directory.Exists(_surveyDir) || Directory.Exists(_emptyDir))
                RemoveDirWithTestSurvey();
            
            Directory.CreateDirectory(_surveyDir);
            Directory.CreateDirectory(_emptyDir);

            var pathToSurveyPackage = Path.Combine(_testsExecutionLocation, TestSurveyPackage);
            using (var zip = new ZipFile(pathToSurveyPackage))
                zip.ExtractAll(_surveyDir, ExtractExistingFileAction.Throw);
        }

        private void ReloadSurveyDirs() {
            var executingDll = Assembly.GetExecutingAssembly().CodeBase;
            var executingDllPath = new Uri(executingDll).AbsolutePath;
            _testsExecutionLocation = Directory.GetParent(executingDllPath).FullName;

            _emptyDir = Path.Combine(_testsExecutionLocation, EmptyDirName);
            _surveyDir = Path.Combine(_testsExecutionLocation, SurveyDirName);
        }

        [TestFixtureTearDown]
        public void RemoveDirWithTestSurvey() {
            if (Directory.Exists(_emptyDir))
                Directory.Delete(_emptyDir, true);
            if (Directory.Exists(_surveyDir))
                Directory.Delete(_surveyDir, true);
        }

        [Test]
        [TestCaseSource("_emptyArgs")]
        public void IfArgsIsNullOrEmptyExit(string[] args) {
            ParseArgsAndCheckResultForNull(args);
        }

        [Test]
        [TestCaseSource("WrongArgs")]
        public void IfArgsSetIsNotValidExit(string[] args) {
            ParseArgsAndCheckResultForNull(args);
        }

        [Test]
        [TestCaseSource("UnknownArgs")]
        public void IfHasUnknownArgsExit(string[] args) {
            ParseArgsAndCheckResultForNull(args);
        }

        [Test]
        [TestCaseSource("SomeArgsIsEmpty")]
        public void IfAnyOfArgsIsEmptyExit(string[] args) {
            ParseArgsAndCheckResultForNull(args);
        }

        [Test]
        [TestCaseSource("NotPathArgsHasIllegalChars")]
        public void IfNotPathArgsHasIllegalCharsExit(string[] args) {
            ParseArgsAndCheckResultForNull(args);
        }

        [Test]
        [TestCaseSource("PathHasNoSurvey")]
        public void IfPathIsHasNoSurveyExit(string[] args) {
            ParseArgsAndCheckResultForNull(args);
        }

        [Test]
        [TestCaseSource("UpdateModeWrongValues")]
        public void IfModeHasWrongValueExit(string[] args) {
            ParseArgsAndCheckResultForNull(args);
        }

        [Test]
        [TestCaseSource("ModeShouldBeSpecifiedForUpdate")]
        public void IfUpdateThenModeShoudBeSpecifiedExit(string[] args) {
            ParseArgsAndCheckResultForNull(args);
        }

        [Test]
        [TestCaseSource("CorrectData")]
        public void FilesShoudBeReadAndCompressedCorrectly(string[] args) {
            var options = new OptionsParser();
            var data = options.Parse(args);
            var surveyPackage = LoadSurveyToPackage(_surveyDir);

            Assert.NotNull(data);
            CollectionAssert.AreEqual(surveyPackage, data.Files);
        }

        [Test]
        [TestCaseSource("ArgsForInterpretation")]
        public ServiceData ArgsShouldBeInterpretedCorrectly(string[] args) {
            var options = new OptionsParser();
            return options.Parse(args);
        }

        private byte[] LoadSurveyToPackage(string surveyDir) {
            using (var zipBufferMemory = new MemoryStream(1024 * 1024 * 4))
            using (var zipProjectDirectory = new ZipFile() { UseUnicodeAsNecessary = true }) {
                zipProjectDirectory.AddDirectory(surveyDir);
                zipProjectDirectory.Save(zipBufferMemory);

                return zipBufferMemory.ToArray();
            }
        }

        private static void ParseArgsAndCheckResultForNull(string[] args) {
            var options = new OptionsParser();
            var data = options.Parse(args);

            Assert.IsNull(data);
        }

        private IEnumerable<TestCaseData> _emptyArgs {
            get {
                yield return new TestCaseData(new object[] {new[] {(string) null}});
                yield return new TestCaseData(new object[] {new[] {string.Empty}});
            }
        }

        private IEnumerable<TestCaseData> WrongArgs {
            get {
                yield return new TestCaseData(new object[] {new[] {"-i", ""}});
                yield return new TestCaseData(new object[] {new[] {"-i", "", "-u", ""}});
                yield return new TestCaseData(new object[] {new[] {"-i", "", "-m", ""}});
                yield return new TestCaseData(new object[] {new[] {"-i", "", "-n", "", "-u", ""}});
                yield return new TestCaseData(new object[] {new[] {"-u", ""}});
                yield return new TestCaseData(new object[] {new[] {"-u", "", "-i", ""}});
                yield return new TestCaseData(new object[] {new[] {"-u", "", "-m", ""}});
                yield return new TestCaseData(new object[] {new[] {"-u", "", "-n", ""}});
                yield return new TestCaseData(new object[] {new[] {"-u", "", "-n", "", "-m", "", "-i", ""}});
            }
        }

        private IEnumerable<TestCaseData> UnknownArgs {
            get {
                yield return new TestCaseData(new object[] {new[] {"-i", "", "-n", "", "-x", ""}});
                yield return new TestCaseData(new object[] {new[] {"-u", "", "-n", "", "-m", "", "-x", ""}});
            }
        }

        private IEnumerable<TestCaseData> SomeArgsIsEmpty {
            get {
                yield return new TestCaseData(new object[] {new[] {"-i", "/some/path", "-n", ""}});
                yield return new TestCaseData(new object[] {new[] {"-u", "", "-n", "NAME", "-m", "1"}});
            }
        }

        private IEnumerable<TestCaseData> NotPathArgsHasIllegalChars {
            get {
                yield return new TestCaseData(new object[] {new[] {"-i", "/some/path", "-n", "my-name"}});
                yield return new TestCaseData(new object[] {new[] {"-u", "", "-n", "NAME", "-m", "1"}});
                yield return new TestCaseData(new object[] {new[] {"-u", "/path", "-n", "1NAME", "-m", "1"}});
            }
        }

        private IEnumerable<TestCaseData> PathHasNoSurvey {
            get {
                ReloadSurveyDirs();
                yield return new TestCaseData(new object[] {new[] {"-i", _emptyDir, "-n", "my-name"}});
                yield return new TestCaseData(new object[] {new[] {"-u", _emptyDir, "-n", "NAME", "-m", "1"}});
            }
        }

        private IEnumerable<TestCaseData> UpdateModeWrongValues {
            get {
                ReloadSurveyDirs();
                yield return new TestCaseData(new object[] {new[] {"-u", _surveyDir, "-n", "NAME", "-m", "a"}});
                yield return new TestCaseData(new object[] {new[] {"-u", _surveyDir, "-n", "NAME", "-m", "-1"}});
                yield return new TestCaseData(new object[] {new[] {"-u", _surveyDir, "-n", "NAME", "-m", "0"}});
                yield return new TestCaseData(new object[] {new[] {"-u", _surveyDir, "-n", "NAME", "-m", "5"}});
            }
        }

        private IEnumerable<TestCaseData> ModeShouldBeSpecifiedForUpdate {
            get {
                ReloadSurveyDirs();
                yield return new TestCaseData(new object[] {new[] {"-u", _surveyDir, "-n", "NAME"}});
                yield return new TestCaseData(new object[] {new[] {"-u", _surveyDir, "-n", "NAME12"}});
            }
        }

        private IEnumerable<TestCaseData> CorrectData {
            get {
                ReloadSurveyDirs();
                yield return new TestCaseData(new object[] {new[] {"-i", _surveyDir, "-n", "NAME1"}});
                yield break;
                yield return new TestCaseData(new object[] {new[] {"-i", _surveyDir, "-n", "NAME2", "-b", "NAME2"}});
                yield return new TestCaseData(new object[] {new[] {"-u", _surveyDir, "-n", "NAME3", "-m", "1"}});
                yield return new TestCaseData(new object[] {new[] {"-u", _surveyDir, "-n", "NAME4", "-m", "2"}});
            }
        }

        private IEnumerable<TestCaseData> ArgsForInterpretation {
            get {
                ReloadSurveyDirs();

                yield return new TestCaseData(new object[] { new[] { "-i", _surveyDir, "-n", "NAME1" }})
                    .Returns(new ServiceData {
                        Mode = DeployMode.Install,
                        Uri = "NAME1",
                        Directory = "NAME1",
                        Database = "NAME1"
                    });
                
                yield return new TestCaseData(new object[] { new[] { "-i", _surveyDir, "-n", "NAME", "-b", "NAME3", "-d", "NAME2" } })
                    .Returns(new ServiceData {
                        Mode = DeployMode.Install,
                        Uri = "NAME",
                        Directory = "NAME2",
                        Database = "NAME3"
                    });

                yield return new TestCaseData(new object[] { new[] { "-u", _surveyDir, "-n", "NAME4", "-m", "2" } })
                    .Returns(new ServiceData {
                        Mode = DeployMode.UpdateAppData,
                        Uri = "NAME4",
                        Directory = "NAME4",
                        Database = "NAME4"
                    });

                yield return new TestCaseData(new object[] { new[] { "-u", _surveyDir, "-n", "NAME4", "-m", "4" } })
                    .Returns(new ServiceData {
                        Mode = DeployMode.UpdateAll,
                        Uri = "NAME4",
                        Directory = "NAME4",
                        Database = "NAME4"
                    });
            }
        }
    }
}
