using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Deployer.Service.WebHost.Models
{
    public class ExceptionRecord
    {
        public string Type { get; set; }
        public string Message { get; set; }
        public List<string> Stacktrace { get; set; }

        public ExceptionRecord InnerEception { get; set; }

        public static ExceptionRecord FromXml(XElement exceptionData) {
            if (exceptionData == null)
                return null;

            return new ExceptionRecord {
                Type = exceptionData.XPathSelectElement("type[1]").Value,
                Message = exceptionData.XPathSelectElement("message[1]").Value,
                Stacktrace = exceptionData.XPathSelectElement("stacktrace[1]").Value
                    .Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList(),
                InnerEception = !string.IsNullOrEmpty(exceptionData.XPathSelectElement("innerexception[1]").Value)
                    ? FromXml(exceptionData.XPathSelectElement("innerexception[1]").Elements().First())
                    : null
            };
        }
    }
}