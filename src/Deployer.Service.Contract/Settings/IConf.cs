using System;
using System.Collections.Generic;
using Deployer.Service.Contract.Contexts;
using Deployer.Service.Contract.Helpers;

namespace Deployer.Service.Contract.Settings
{
    public interface IConf {
        string SessionKey { get; }
        Credentials Credentials { get; }
        Survey Survey { get; }
        SurveyPaths SurveyPath { get; }
        PackagePaths PackagePaths { get; }
        CommonPaths CommonPaths { get; }
        DbScriptsPaths DbScriptsPaths { get; }
        TemplatesPaths Templates { get; }
        ConnectionStrings ConnectionStrings { get; }
        IISInfo IIS { get; }
        IPackageManager PackageManager { get; }
        IIISManager IISManager { get; }
        IEnumerable<IUpdateRule> UpdateRules { get; }
    }

    public struct Credentials
    {
        public readonly string Domain;
        public readonly string Login;
        public readonly string Password;

        public Credentials(string domain, string login, string password) : this() {
            Domain = domain;
            Login = login;
            Password = password;
        }
    }

    public struct PackagePaths
    {
        public readonly string Survey;
        public readonly string Backup;

        public PackagePaths(string survey, string backup) : this() {
            Survey = survey;
            Backup = backup;
        }
    }

    public class SurveyPaths
    {
        public SurveyPaths(string survey, Func<string> getBinDir) {
            Survey = survey;
            GetBinDir = getBinDir;
        }

        public string Survey { get; private set; }
        public Func<string> GetBinDir { get; private set; }
    }

    public struct CommonPaths
    {
        public readonly string Surveys;
        public readonly string Uploads;
        public readonly string Backups;

        public CommonPaths(string surveys, string uploads, string backups) : this() {
            Surveys = surveys;
            Backups = backups;
            Uploads = uploads;
        }
    }

    public struct Survey
    {
        public readonly string Name;
        public readonly string DbName;
        public readonly DeployMode DeployMode;

        public Survey(string name, string dbName, DeployMode deployMode) : this() {
            Name = name;
            DbName = dbName;
            DeployMode = deployMode;
        }
    }

    public struct ConnectionStrings
    {
        public readonly string Master;
        public readonly string Deployer;

        public ConnectionStrings(string master, string deployer) : this() {
            Master = master;
            Deployer = deployer;
        }
    }

    public struct DbScriptsPaths
    {
        public readonly string Create;
        public readonly string Validate;
        public readonly string Delete;

        public DbScriptsPaths(string create, string validate, string delete) : this() {
            Create = create;
            Validate = validate;
            Delete = delete;
        }
    }

    public struct TemplatesPaths
    {
        public readonly string ConnectionString;
        public readonly string AdditionalBins;
        
        public TemplatesPaths(string connectionString, string additionalBins) : this() {
            ConnectionString = connectionString;
            AdditionalBins = additionalBins;
        }
    }

    public class IISInfo
    {
        public IISInfo(string version, string address, string appPool, IEnumerable<KeyValuePair<string, string>> dirsNeedIISAccess) {
            DirsNeedIISAccess = dirsNeedIISAccess;
            Version = version;
            Address = address;
            AppPool = appPool;
        }

        public string Version { get; private set; }
        public string AppPool { get; private set; }
        public string Address { get; private set; }
        public IEnumerable<KeyValuePair<string, string>> DirsNeedIISAccess { get; private set; }
    }
}