using System.Collections.Generic;
using Deployer.Service.Contract.Contexts;
using Deployer.Service.Contract.Helpers;

namespace Deployer.Service.Contract.Settings
{
    public interface IDeploySettings {
        string SessionKey { get; set; }
        string SurveyName { get; }
        string DbName { get; }
        DeployMode DeployMode { get; }
        IPackageManager PackageManager { get; }
        string SurveyPath { get; }
        string IISSurveyPath { get; }
        string SurveyBinPath { get; }
        string SurveyPackagePath { get; }
        string SurveyBackupPackagePath { get; }
        string this[string key] { get; }
        string GetIISRoot();
        IEnumerable<IUpdateRule> GetUpdateRules();
    }
}