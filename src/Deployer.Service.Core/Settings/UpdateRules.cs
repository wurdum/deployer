using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Deployer.Service.Contract.Contexts;
using Deployer.Service.Contract.Settings;
using Deployer.Service.Core.Settings.Section;

namespace Deployer.Service.Core.Settings
{
    public enum FuncType { StartsWith = 1, EndsWith, Equals }

    public class UpdateRulesBuilder
    {
        public static Dictionary<string, FuncType> _funcTypes = new Dictionary<string, FuncType> {
            {"startsWith", FuncType.StartsWith}, {"equals", FuncType.Equals}, {"endsWith", FuncType.EndsWith}
        };

        private readonly DeployerConfigurationSection _confSection;
        private Dictionary<DeployMode, List<UpdateRule>> _rules;
        
        public UpdateRulesBuilder(DeployerConfigurationSection confSection) {
            _confSection = confSection;
            _rules = BuildRules();
        }

        public IEnumerable<IUpdateRule> this[DeployMode deployMode] {
            get { return _rules[deployMode]; }
        }

        private Dictionary<DeployMode, List<UpdateRule>> BuildRules() {
            var rules = new Dictionary<DeployMode, List<UpdateRule>>();
            var generalRules = _confSection.UpdateRules.NeverToUpdate.ToUpdateRules().ToList();
            foreach (var mode in Enum.GetValues(typeof (DeployMode)).Cast<DeployMode>())
                rules[mode] = new List<UpdateRule>(generalRules);

            foreach (ModeElement mode in _confSection.UpdateRules.Modes)
                rules[(DeployMode)mode.Id].AddRange(mode.Rules.ToUpdateRules());
            
            return rules;
        }

        public static FuncType ToFunc(string funcTypeStr) {
            FuncType funcType;
            if (_funcTypes.TryGetValue(funcTypeStr, out funcType))
                return funcType;
            throw new ArgumentException("Unlnown argument value is received.", "funcTypeStr");
        }
    }

    public static class UpdateRulesBuilderUtils
    {
        public static IEnumerable<UpdateRule> ToUpdateRules(this UpdateRulesCollection coll) {
            return coll.Cast<UpdateRuleElement>().Select(rule => new UpdateRule(UpdateRulesBuilder.ToFunc(rule.Func), rule.Value, rule.Inverted));
        }
    }

    public class UpdateRule : IUpdateRule
    {
        private readonly Func<string, string, bool> _checkFunction;
        private readonly string _ruleArgument;
        private readonly bool _isInverted;

        public UpdateRule(FuncType type, string ruleArgument, bool isInverted) {
            _ruleArgument = ruleArgument;
            _isInverted = isInverted;
            switch (type) {
                case FuncType.StartsWith:
                    _checkFunction = (s, r) => FixPathSeparators(s).ToLower().StartsWith(FixPathSeparators(r));
                    break;
                case FuncType.EndsWith:
                    _checkFunction = (s, r) => FixPathSeparators(s).ToLower().EndsWith(FixPathSeparators(r));
                    break;
                case FuncType.Equals:
                    _checkFunction = (s, r) => FixPathSeparators(s).Equals(FixPathSeparators(r), 
                        StringComparison.InvariantCultureIgnoreCase);
                    break;
                default: throw new ArgumentException("Unlnown argument value is received.", "type");
            }
        }

        public bool Satisfies(string filePath) {
            var checkFunction = _checkFunction(filePath, _ruleArgument);
            return checkFunction != _isInverted;
        }

        private string FixPathSeparators(string pathChunk) {
            return pathChunk.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);
        }
    }
}