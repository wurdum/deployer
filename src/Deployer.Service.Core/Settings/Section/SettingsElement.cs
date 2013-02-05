using System.Configuration;

namespace Deployer.Service.Core.Settings.Section
{
    public class SettingsElement : ConfigurationElement
    {
        [ConfigurationProperty("credentials", IsRequired = false)]
        public CredentialsElement Credentials {
            get { return (CredentialsElement) this["credentials"]; }
        }

        [ConfigurationProperty("iis", IsRequired = true)]
        public IISElement IIS {
            get { return (IISElement) this["iis"]; }
        }

        [ConfigurationProperty("scriptsPaths", IsRequired = true)]
        public ScriptsPathsElement ScriptsPaths {
            get { return (ScriptsPathsElement) this["scriptsPaths"]; }
        }

        [ConfigurationProperty("templates", IsRequired = true)]
        public TemplatesElement Templates {
            get { return (TemplatesElement) this["templates"]; }
        }

        [ConfigurationProperty("paths", IsRequired = true)]
        public PathsElement Paths {
            get { return (PathsElement) this["paths"]; }
        }
    }

    public class CredentialsElement : ConfigurationElement
    {
        [ConfigurationProperty("domain", IsRequired = false)]
        public string Domain {
            get { return this["domain"].ToString(); }
        }

        [ConfigurationProperty("login", IsRequired = false)]
        public string Login {
            get { return this["login"].ToString(); }
        }

        [ConfigurationProperty("password", IsRequired = false)]
        public string Password {
            get { return this["password"].ToString(); }
        }
    }

    public class IISElement : ConfigurationElement
    {
        [ConfigurationProperty("version", IsRequired = true, DefaultValue = "7")]
        [RegexStringValidator(@"(6|7)")]
        public string Version {
            get { return this["version"].ToString(); }
        }

        [ConfigurationProperty("appPool", IsRequired = true)]
        public string ApplicationPool {
            get { return this["appPool"].ToString(); }
        }

        [ConfigurationProperty("address", IsRequired = true)]
        public string Address {
            get { return this["address"].ToString(); }
        }

        [ConfigurationProperty("dirsWithIISAccess", IsRequired = false)]
        [ConfigurationCollection(typeof(DirsWithIISAccessElement), AddItemName = "dir")]
        public DirsWithIISAccessElement DirsWithIISAccess {
            get { return (DirsWithIISAccessElement)this["dirsWithIISAccess"]; }
        }
    }

    public class DirsWithIISAccessElement : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement() {
            return new DirElement();
        }

        protected override object GetElementKey(ConfigurationElement element) {
            return ((DirElement) element).Name;
        }
    }

    public class DirElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name {
            get { return this["name"].ToString(); }
        }

        [ConfigurationProperty("user", IsRequired = true)]
        public string User {
            get { return this["user"].ToString(); }
        }
    }

    public class ScriptsPathsElement : ConfigurationElement
    {
        [ConfigurationProperty("dbCreate", IsRequired = true)]
        public string DbCreate {
            get { return this["dbCreate"].ToString(); }
        }

        [ConfigurationProperty("dbValidate", IsRequired = true)]
        public string DbValidate {
            get { return this["dbValidate"].ToString(); }
        }

        [ConfigurationProperty("dbDelete", IsRequired = true)]
        public string DbDelete {
            get { return this["dbDelete"].ToString(); }
        }
    }

    public class TemplatesElement : ConfigurationElement
    {
        [ConfigurationProperty("connectionStrings", IsRequired = true)]
        public string ConnectionStrings {
            get { return this["connectionStrings"].ToString(); }
        }
    }

    public class PathsElement : ConfigurationElement
    {
        [ConfigurationProperty("surveys", IsRequired = true)]
        public string Surveys {
            get { return this["surveys"].ToString(); }
        }

        [ConfigurationProperty("uploads", IsRequired = true)]
        public string Uploads {
            get { return this["uploads"].ToString(); }
        }

        [ConfigurationProperty("backups", IsRequired = true)]
        public string Backups {
            get { return this["backups"].ToString(); }
        }

        [ConfigurationProperty("additionalResources", IsRequired = true)]
        public AdditionalResourcesElement AdditionalResources {
            get { return (AdditionalResourcesElement)this["additionalResources"]; }
        }
    }

    public class AdditionalResourcesElement : ConfigurationElement
    {
        [ConfigurationProperty("bins", IsRequired = true)]
        public string Bins {
            get { return this["bins"].ToString(); }
        }
    }
}