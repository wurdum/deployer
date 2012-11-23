using System.Collections.Generic;
using Deployer.Service.Contract.Helpers;
using Deployer.Service.Contract.Settings;

namespace Deployer.Tests.Stubs
{
    public class PackageManagerStub : IPackageManager
    {
        public void FolderToZip(string sourceDir, string path) { }

        public void BytesToZip(byte[] packageArray, string path) { }

        public void UnpackZipByRules(string packagePath, string unpackPath, IEnumerable<IUpdateRule> updateRules) { }

        public void UnpackZipSilently(string packagePath, string unpackPath) { }

        public void UnpackZipIfOverwriteThrow(string packagePath, string unpackPath) { }

        public void CleanFolder(string pathToSurveyFiles, IEnumerable<IUpdateRule> updateRules) { }
    }
}