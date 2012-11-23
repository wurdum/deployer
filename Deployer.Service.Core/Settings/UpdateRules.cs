using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Deployer.Service.Contract.Contexts;
using Deployer.Service.Contract.Settings;
using NLog;

namespace Deployer.Service.Core.Settings
{
    public enum FuncType { StartsWith = 1, EndsWith, Equals }

    public class UpdateRulesBuilder
    {
        private static Dictionary<string, FuncType> _funcTypes = new Dictionary<string, FuncType> {
            {"startsWith", FuncType.StartsWith}, {"equals", FuncType.Equals}, {"endsWith", FuncType.EndsWith}
        };
        private static Dictionary<DeployMode, List<UpdateRule>> _rulesCache;
        private readonly string _rulesFile;
        private DateTime _lastUpdated;

        public UpdateRulesBuilder(string rulesFile) {
            if (string.IsNullOrEmpty(rulesFile) || !File.Exists(rulesFile))
                throw new ArgumentException("Specified file with rules doesn't exists.", "rulesFile");

            _rulesFile = rulesFile;
            UpdateRulesChache();
        }

        public IEnumerable<UpdateRule> GetRules(DeployMode mode) {
            if (_rulesCache == null || _lastUpdated < File.GetLastWriteTimeUtc(_rulesFile))
                UpdateRulesChache();

            return _rulesCache[mode];
        }

        private void UpdateRulesChache() {
            _lastUpdated = File.GetLastWriteTimeUtc(_rulesFile);
            _rulesCache = new Dictionary<DeployMode, List<UpdateRule>>();

            using (var reader = XmlReader.Create(_rulesFile)) {
                var updatesRulesRoot = XDocument.Load(reader).Root.Elements().First(e => e.Name == "UpdateRules");
                var generalRulesRoot = updatesRulesRoot.Elements().First(e => e.Name == "neverToUpdate");
                var modesRulesRoots = updatesRulesRoot.Elements().Where(mr => mr.Name != "neverToUpdate");
                var generalRules = generalRulesRoot.Elements().Select(rule => GetRuleFromXElement(rule)).ToList();
                
                foreach (var mode in Enum.GetValues(typeof(DeployMode)).Cast<DeployMode>()) {
                    _rulesCache[mode] = new List<UpdateRule>(generalRules);
                }

                foreach (var mode in modesRulesRoots) {
                    var updateMode = (DeployMode) Int32.Parse(GetAttributeByName(mode, "id").Value);
                    var modeRules = mode.Elements().Select(rule => GetRuleFromXElement(rule)).ToList();
                    _rulesCache[updateMode].AddRange(modeRules);
                }
            }
        }

        private UpdateRule GetRuleFromXElement(XElement element) {
            var attribute = GetAttributeByName(element, "modifier");
            return new UpdateRule(ToFunc(element.Attribute("func").Value), element.Value, attribute != null && attribute.Value == "not");
        }

        private XAttribute GetAttributeByName(XElement rule, string attrName) {
            return rule.Attributes().FirstOrDefault(a => a.Name == attrName);
        }

        private FuncType ToFunc(string funcTypeStr) {
            FuncType funcType;
            if (_funcTypes.TryGetValue(funcTypeStr, out funcType))
                return funcType;
            throw new ArgumentException("Unlnown argument value is received.", "funcTypeStr");
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