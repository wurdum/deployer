using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using System.Web.Mvc;
using Deployer.Service.Data;
using Deployer.Service.WebHost.Models;
using NLog;
using NLog.Layouts;
using NLog.Targets;
using DeployMode = Deployer.Service.Contract.Contexts.DeployMode;

namespace Deployer.Service.WebHost.Controllers
{
    public class HomeController : Controller
    {
        public string SettingsFile {
            get { return Path.Combine(HostingEnvironment.ApplicationPhysicalPath, @"bin/DepSettings.xml"); }
        }

        public ActionResult Index() {
            var connString = ConfigurationManager.ConnectionStrings["deployer"].ConnectionString;
            
            List<DeployRecord> deployRecords;
            using (var context = new DbDataContext(connString))
                deployRecords = (from deploy in context.Deploys
                                 let exceptions = context.Exceptions.Where(e => deploy.Id == e.DeployId).ToList()
                                 orderby deploy.Id descending
                                 select new DeployRecord {
                                     Name = deploy.SurveyName,
                                     User = deploy.User,
                                     Key = deploy.SessionKey,
                                     Start = deploy.StartUtc,
                                     End = deploy.EndUtc,
                                     Status = (DeployStatusModel)deploy.Status,
                                     Mode = (DeployMode)deploy.Mode,
                                     Exceptions = exceptions.Select(e => ExceptionRecord.FromXml(e.ExceptionData)).ToList()
                                 }).Take(30).ToList();

            return View(deployRecords);
        }

        public ActionResult Log(string file) {
            if (LogManager.Configuration == null)
                throw new InvalidOperationException("Logger was not found");

            var logEntries = LogManager.Configuration.AllTargets.Cast<FileTarget>()
                .Select(f => {
                    var pathInQuotes = SimpleLayout.Evaluate(f.FileName.ToString());
                    var path = pathInQuotes.Substring(1, pathInQuotes.Length - 2);
                    return new { f.Name, Path = path };
                }).ToList();

            if (logEntries.Count == 0)
                throw new InvalidOperationException("Log entries was not found");

            var active = string.IsNullOrEmpty(file) || logEntries.All(l => l.Name != file) ? logEntries.First().Name : file;
            return View(new LogModel {
                Active = active,
                LogEntries = logEntries.Select(l => l.Name),
                Content = ReadLog(logEntries.Single(l => l.Name.Equals(active)).Path)
            });
        }

        private static IEnumerable<string> ReadLog(string path) {
            var showLines = new List<string>();
            if (!System.IO.File.Exists(path))
                return showLines;

            var logLines = System.IO.File.ReadAllLines(path).ToArray();
            var cursor = 0;
            var buffer = string.Empty;
            do {
                buffer += logLines[cursor] + Environment.NewLine;
                if (cursor + 1 >= logLines.Length) {
                    showLines.Add(buffer);
                    break;
                }

                if (string.IsNullOrEmpty(logLines[++cursor]) || !Char.IsDigit(logLines[cursor][0]))
                    continue;
                
                showLines.Add(buffer);
                buffer = string.Empty;
            } while (cursor < logLines.Length);

            showLines.Reverse();
            return showLines.Take(100);
        }
    }
}