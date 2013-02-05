using System;
using System.Configuration;

namespace Deployer.Service.Core.Settings.Section
{
    public class UpdateRulesElement : ConfigurationElement
    {
        [ConfigurationProperty("modes", IsRequired = true)]
        [ConfigurationCollection(typeof(ModesCollection), AddItemName = "mode")]
        public ModesCollection Modes {
            get { return (ModesCollection) this["modes"]; }
        }

        [ConfigurationProperty("neverToUpdate", IsRequired = true)]
        [ConfigurationCollection(typeof(UpdateRulesCollection), AddItemName = "rule")]
        public UpdateRulesCollection NeverToUpdate {
            get { return (UpdateRulesCollection) this["neverToUpdate"]; }
        }
    }

    public class ModesCollection : ConfigurationElementCollection
    {
        public ModeElement this[int index] {
            get { return BaseGet(index) as ModeElement; }
        }

        protected override ConfigurationElement CreateNewElement() {
            return new ModeElement();
        }

        protected override object GetElementKey(ConfigurationElement element) {
            return ((ModeElement)element).Id;
        }
    }

    public class ModeElement : ConfigurationElement
    {
        [ConfigurationProperty("id", IsRequired = true)]
        public int Id {
            get { return Convert.ToInt32((object) this["id"]); }
        }

        [ConfigurationProperty("name", IsRequired = true)]
        public string Name {
            get { return this["name"].ToString(); }
        }

        [ConfigurationProperty("updateDir", IsRequired = true)]
        public string UpdateDir {
            get { return this["updateDir"].ToString(); }
        }

        [ConfigurationProperty("rules", IsRequired = false)]
        [ConfigurationCollection(typeof(UpdateRulesCollection), AddItemName = "rule")]
        public UpdateRulesCollection Rules {
            get { return (UpdateRulesCollection) this["rules"]; }
        }
    }

    public class UpdateRulesCollection : ConfigurationElementCollection
    {
        public UpdateRuleElement this[int index] {
            get { return BaseGet(index) as UpdateRuleElement; }
        }

        protected override ConfigurationElement CreateNewElement() {
            return new UpdateRuleElement();
        }

        protected override object GetElementKey(ConfigurationElement element) {
            return ((UpdateRuleElement)element).Value;
        }
    }

    public class UpdateRuleElement : ConfigurationElement
    {
        [ConfigurationProperty("inverted", IsRequired = false, DefaultValue = false)]
        public bool Inverted {
            get { return Convert.ToBoolean(this["inverted"]); }
        }

        [ConfigurationProperty("func", IsRequired = true, DefaultValue = "equals")]
        [RegexStringValidator(@"(startsWith|equals|endsWith)")]
        public string Func {
            get { return this["func"].ToString(); }
        }

        [ConfigurationProperty("value", IsRequired = true)]
        public string Value {
            get { return this["value"].ToString(); }
        }
    }
}