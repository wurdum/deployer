using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Deployer.Service.Contract.Helpers;
using Deployer.Service.Contract.Settings;
using Ionic.Zip;

namespace Deployer.Service.Core.Helpers
{
    public class PackageManager : IPackageManager
    {
        public PackageManager() {}

        public void FolderToZip(string sourceDir, string path) {
            using (var surveyBackup = new ZipFile()) {
                surveyBackup.AddDirectory(sourceDir);
                surveyBackup.Save(path);
            }
        }

        public void BytesToZip(byte[] packageArray, string path) {
            using (var wr = new BinaryWriter(new FileStream(path, FileMode.Create, FileAccess.ReadWrite))) {
                wr.Write(packageArray);
            }
        }

        public void UnpackZipByRules(string packagePath, string unpackPath, IEnumerable<IUpdateRule> updateRules) {
            CleanFolder(unpackPath, updateRules);
            using (var zip = new ZipFile(packagePath)) {
                foreach (var file in zip.Where(file => updateRules.SatisfiesForUpdate(file.FileName)))
                    file.Extract(unpackPath, ExtractExistingFileAction.OverwriteSilently);
            }
        }

        public void UnpackZipSilently(string packagePath, string unpackPath) {
            UnpackZip(packagePath, unpackPath, ExtractExistingFileAction.OverwriteSilently);
        }

        public void UnpackZipIfOverwriteThrow(string packagePath, string unpackPath) {
            UnpackZip(packagePath, unpackPath, ExtractExistingFileAction.Throw);
        }

        public void CleanFolder(string pathToSurveyFiles, IEnumerable<IUpdateRule> updateRules) {
            CleanFolderInner(pathToSurveyFiles, updateRules, s => PreparePath(pathToSurveyFiles, s));
        }

        private static string PreparePath(string pathToSurveyFiles, string s) {
            return Path.DirectorySeparatorChar.Equals(pathToSurveyFiles.Last()) ? 
                s.Substring(pathToSurveyFiles.Length) : 
                s.Substring(pathToSurveyFiles.Length + 1);
        }

        private void UnpackZip(string packagePath, string unpackPath, ExtractExistingFileAction existingFileAction) {
            using (var file = new ZipFile(packagePath)) {
                foreach (var zipEntry in file)
                    zipEntry.Extract(unpackPath, existingFileAction);
            }
        }

        private bool CleanFolderInner(string path, IEnumerable<IUpdateRule> updateRules, Func<string, string> preparePath) {
            foreach (var file in Directory.GetFiles(path)) {
                var fileName = preparePath(file);
                if (updateRules.SatisfiesForUpdate(fileName)) {
                    File.SetAttributes(file, FileAttributes.Normal);
                    File.Delete(file);
                }
            }

            foreach (var dir in Directory.GetDirectories(path)) {
                var dirName = preparePath(dir);
                if (CleanFolderInner(dir, updateRules, preparePath) && updateRules.SatisfiesForUpdate(dirName))
                    DeletePersistantly(dir);
            }

            return !Directory.GetFiles(path).Any();
        }

        private void DeletePersistantly(string dir) {
            try {
                Directory.Delete(dir);
            } catch (IOException) {
                Thread.Sleep(100);
                Directory.Delete(dir);
            }
        }
    }

    public static class UpdateRulesUtils
    {
        public static bool SatisfiesForUpdate(this IEnumerable<IUpdateRule> rules, string fileName) {
            return rules.All(rule => rule.Satisfies(fileName));
        }
    }
}