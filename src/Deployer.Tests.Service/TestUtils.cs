using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Deployer.Service.Contract.Contexts;

namespace Deployer.Tests.Service
{
    public static class TestUtils
    {
        private static Dictionary<DeployMode, string> _packages;

        static TestUtils() {
            var testsAssembly = Assembly.GetExecutingAssembly();
            var testsAssemblyLocation = new Uri(testsAssembly.CodeBase).LocalPath;
            var testsRoot = Path.GetDirectoryName(testsAssemblyLocation);
            var packagesRoot = Path.Combine(testsRoot, "Resources");
            _packages = new Dictionary<DeployMode, string> {
                { DeployMode.Install, Path.Combine(packagesRoot, "TestSurvey.zip") },
                { DeployMode.UpdateAll, Path.Combine(packagesRoot, "TestSurveyUpd.zip") },
                { DeployMode.UpdateAppData, Path.Combine(packagesRoot, "TestSurveyUpdOnlyAppData.zip") },
                { DeployMode.UpdateBin, Path.Combine(packagesRoot, "TestSurveyUpdOnlyBin.zip") }
            };
        }

        public static string GetPostfix() {
            return GetPostfix(6);
        }

        public static string GetPostfix(int lenght) {
            var zeroBasedLenth = lenght - 1;
            var guid = Guid.NewGuid().ToString().Replace("-", "");
            return guid.Length < zeroBasedLenth ? guid : guid.Substring(0, zeroBasedLenth);
        }

        public static void PreparePackageFor(DeployMode mode, string path) {
            File.Copy(_packages[mode], path, true);
        }
    }
}