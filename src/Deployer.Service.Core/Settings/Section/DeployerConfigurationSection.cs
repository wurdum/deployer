using System;
using System.Configuration;

namespace Deployer.Service.Core.Settings.Section
{
    public class DeployerConfigurationSection : ConfigurationSection
    {
        public static string SectionName {
            get { return "deployer"; }
        }

        public static DeployerConfigurationSection Instance {
            get {
                var section = ConfigurationManager.GetSection(SectionName) as DeployerConfigurationSection;
                if (section == null)
                    throw new NullReferenceException("Section '" + SectionName +"' was not found");

                return section;
            }
        }

        [ConfigurationProperty("settings", IsRequired = true)]
        public SettingsElement Settings { 
            get { return (SettingsElement) this["settings"]; }
        }

        [ConfigurationProperty("updateRules", IsRequired = true)]
        public UpdateRulesElement UpdateRules {
            get { return (UpdateRulesElement) this["updateRules"]; }
        }
    }
}