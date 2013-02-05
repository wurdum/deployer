using System;
using System.Collections.Generic;
using System.Linq;
using Deployer.Service.Contract.Helpers;
using Deployer.Service.Contract.Settings;
using Microsoft.Web.Administration;

namespace Deployer.Service.Core.Helpers.IISManagers
{
    public class IIS7Manager : IIISManager
    {
        private readonly IISInfo _iisInfo;
        private readonly string _surveyName;

        private string _surveyPath;

        public IIS7Manager(IISInfo iisInfo, string surveyName) {
            _iisInfo = iisInfo;
            _surveyName = surveyName;
        }

        public string SurveyPath {
            get { return _surveyPath ?? (_surveyPath = GetSurveyPath()); }
        }

        public bool HasSurvey() {
            var iis = new ServerManager();
            var site = iis.Sites.First(s => s.Name.Equals(_iisInfo.Address, StringComparison.InvariantCultureIgnoreCase));
            return site.Applications.Any(a => a.Path.Equals(GetAppPath(_surveyName)));
        }

        public void CreateSurvey(string surveyPath) {
            var root = _iisInfo.Address;
            var iis = new ServerManager();
            var site = iis.Sites.First(s => s.Name.Equals(root, StringComparison.InvariantCultureIgnoreCase));
            var app = site.Applications.Add(GetAppPath(_surveyName), surveyPath);
            app.ApplicationPoolName = _iisInfo.AppPool;
            iis.CommitChanges();
        }

        public void DeleteSurvey() {
            var root = _iisInfo.Address;
            var iis = new ServerManager();
            var site = iis.Sites.First(s => s.Name.Equals(root, StringComparison.InvariantCultureIgnoreCase));
            var app = site.Applications.FirstOrDefault(a => a.Path.Equals(GetAppPath(_surveyName), StringComparison.InvariantCultureIgnoreCase));
            if (app == null)
                throw new KeyNotFoundException("Survey '" + _surveyName + "' connot be deleted because it's was not found.");

            app.Delete();
            iis.CommitChanges();
        }

        private string GetSurveyPath() {
            var iis = new ServerManager();
            var site = iis.Sites.FirstOrDefault(s => s.Name.Equals(_iisInfo.Address, StringComparison.InvariantCultureIgnoreCase));
            var app = site.Applications.FirstOrDefault(a => a.Path.Equals(GetAppPath(_surveyName)));
            if (app == null)
                throw new KeyNotFoundException("Site with name '" + GetAppPath(_surveyName) + "' was not found");

            return app.VirtualDirectories["/"].PhysicalPath;
        }

        private string GetAppPath(string uri) {
            return "/" + uri;
        }
    }
}