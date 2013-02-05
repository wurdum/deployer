using System.Collections.Generic;
using Deployer.Service.Contract.Settings;

namespace Deployer.Service.Contract.Helpers
{
    public interface IPackageManager 
    {
        void FolderToZip(string sourceDir, string path);
        void BytesToZip(byte[] packageArray, string path);
        void UnpackZipByRules(string packagePath, string unpackPath, IEnumerable<IUpdateRule> updateRules);
        void UnpackZipSilently(string packagePath, string unpackPath);
        void UnpackZipIfOverwriteThrow(string packagePath, string unpackPath);
        void CleanFolder(string pathToSurveyFiles, IEnumerable<IUpdateRule> updateRules);
    }
}