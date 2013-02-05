using System.Collections.Generic;
using System.IO;
using System.DirectoryServices;
using System.Linq;
using Deployer.Service.Contract.Helpers;
using Deployer.Service.Contract.Settings;

namespace Deployer.Service.Core.Helpers.IISManagers
{
    public class IIS6Manager : IIISManager
    {
        private const string NodeSchema = "IIsWebVirtualDir";
        private const bool InProcFlag = true;
        private const int AppIsolatedLvl = 2;

        private readonly IISInfo _iisInfo;
        private readonly string _surveyName;

        private string _surveyPath;

        public IIS6Manager(IISInfo iisInfo, string surveyName) {
            _iisInfo = iisInfo;
            _surveyName = surveyName;
        }

        public string SurveyPath {
            get { return _surveyPath ?? (_surveyPath = GetSurveyPath()); }
        }

        public bool HasSurvey() {
            return DirectoryEntry.Exists(PathCombine(_iisInfo.Address, _surveyName));
        }

        public void CreateSurvey(string surveyPath) {
            var site = new DirectoryEntry(_iisInfo.Address);
            var app = site.Children.Add(_surveyName, NodeSchema);
            app.Properties["Path"][0] = surveyPath;
            app.Properties["AppPoolId"][0] = _iisInfo.AppPool;
            app.CommitChanges();
            app.Invoke("AppCreate", InProcFlag);
            app.Properties["AppIsolated"][0] = AppIsolatedLvl;
            app.Properties["AppFriendlyName"][0] = _surveyName;
            app.CommitChanges();
        }

        public void DeleteSurvey() {
            var site =new DirectoryEntry(_iisInfo.Address);
            var app = site.Children.Cast<DirectoryEntry>().SingleOrDefault(vd => vd.Name == _surveyName && vd.Properties["KeyType"].Value.ToString().Trim() == NodeSchema);
            if (app == null)
                throw new KeyNotFoundException("Survey '" + _surveyName + "' connot be deleted because it's was not found.");

            site.Invoke("Delete", new[] { app.SchemaClassName, _surveyName });
            site.CommitChanges();
        }

        private string PathCombine(params string[] paths) {
            var resultPath = Path.Combine(paths);
            return resultPath.Replace('\\', '/');
        }

        private string GetSurveyPath() {
            var appUri = PathCombine(_iisInfo.Address, _surveyName);
            if (!DirectoryEntry.Exists(appUri))
                throw new KeyNotFoundException("Site with name '" + appUri + "' was not found");

            var surveySite = new DirectoryEntry(appUri);
            return surveySite.Properties["Path"].Value.ToString();
        }
    }
}