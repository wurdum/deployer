using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Deployer.Client.DeployServiceReference;

namespace Deployer.Client
{
    public class DeployNotifier
    {
        private Dictionary<string, ActionStepContext> _expectedSteps;
        private TableRenderer _renderer;

        public void NotifyAction(ActionContext context) {
            Console.WriteLine();
            _renderer = new TableRenderer(context.Name, new[] {
                new Tuple<string, int>("Status", 15),
                new Tuple<string, int>("Action", 20)
            });

            _expectedSteps = context.Steps.ToDictionary(s => s.Name);
            foreach (var step in context.Steps)
                _renderer.RenderRow(new[] {
                    new Tuple<ConsoleColor, string>(ConsoleColor.White, step.Status.ToString()), 
                    new Tuple<ConsoleColor, string>(ConsoleColor.White, step.Name),
                });
        }

        public void NotifyActionStep(ActionStepContext context) {
            _expectedSteps[context.Name].Status = context.Status;
            _renderer.ClearCursorPos();
            foreach (var step in _expectedSteps.Select(s => s.Value))
                _renderer.RenderRow(new[] {
                    new Tuple<ConsoleColor, string>(Colorize(step.Status), step.Status.ToString()), 
                    new Tuple<ConsoleColor, string>(ConsoleColor.White, step.Name),
                });
        }

        public void NotifyError(ErrorContext context) {
            Console.WriteLine();
            Console.WriteLine("Exception is received.");
            try {
                RenderException(context.Exception);
            } catch {
                Console.WriteLine("During exception rendering was obtained unhandled exception. For more information, check the log.");
            }
        }

        public void NotifyFault(UnhandledException exception) {
            Console.WriteLine();
            Console.WriteLine("During the deploy was obtained and unhandled exception. For more information, check the log.");
            RenderException(exception);
        }

        public void NotifyClose() {
            Console.WriteLine();
            Console.WriteLine("Deploy is finished.");
        }

        private static void RenderException(UnhandledException ex) {
            var exceptionRenderer = new TableRenderer("Exception details:", new[] { new Tuple<string, int>(ex.Source, Console.WindowWidth - 6), });

            TableRenderer.SplitString(ex.Message, Console.WindowWidth - 9).ToList().ForEach(l => exceptionRenderer.RenderRow(new[] { l }));
            TableRenderer.SplitString(ex.StackTrace, Console.WindowWidth - 9).ToList().ForEach(l => exceptionRenderer.RenderRow(new[] { l }));

            if (!string.IsNullOrEmpty(ex.InnerExMessage))
                TableRenderer.SplitString(ex.InnerExMessage, Console.WindowWidth - 9).ToList().ForEach(l => exceptionRenderer.RenderRow(new[] { l }));
        }

        private static void RenderException(XElement ex) {
            if (ex.Name == "innerexception")
                ex = ex.Elements().Single();

            var exType = ex.Elements().Single(e => e.Name == "type").Value;
            var exMessage = ex.Elements().Single(e => e.Name == "message").Value;
            var exStacktrace = ex.Elements().Single(e => e.Name == "stacktrace").Value;
            var exInnerexception = ex.Elements().Single(e => e.Name == "innerexception");

            var exceptionRenderer = new TableRenderer("Exception details:", new[] {new Tuple<string, int>(exType, Console.WindowWidth - 6),});

            TableRenderer.SplitString(exMessage, Console.WindowWidth - 9).ToList().ForEach(l => exceptionRenderer.RenderRow(new[] {l}));
            TableRenderer.SplitString(exStacktrace, Console.WindowWidth - 9).ToList().ForEach(l => exceptionRenderer.RenderRow(new[] {l}));

            if (string.IsNullOrEmpty(exInnerexception.Value))
                return;

            Console.WriteLine();
            Console.WriteLine("Inner exception:");
            RenderException(exInnerexception);
        }

        private ConsoleColor Colorize(StepStatus status) {
            switch (status) {
                case StepStatus.None:
                    return ConsoleColor.Gray;
                case StepStatus.InProgress:
                    return ConsoleColor.White;
                case StepStatus.Failed:
                    return ConsoleColor.Red;
                case StepStatus.Complete:
                    return ConsoleColor.Green;
                default: throw new ArgumentException("Unknown value was received.", "status");
            }
        }
    }

    public class TableRenderer
    {
        private readonly Tuple<string, int>[] _headers;
        private readonly string _title;
        private readonly string _rowTemplate;
        private bool _headerIsRendered;
        private int _tableHeight;
        private int _totalWidth;

        public TableRenderer(string title, Tuple<string, int>[] headers) {
            _title = title;
            _headers = headers;
            _totalWidth = _headers.Sum(h => h.Item2) + (_headers.Count() - 1) * 3;
            _rowTemplate = "| " + String.Join(" | ", _headers.Select((h, i) => "{" + i + ",-" + h.Item2 + "}").ToArray()) + " |";
        }

        public void RenderRow(string[] rowValues) {
            RenderRow(rowValues.Select(r => new Tuple<ConsoleColor, string>(ConsoleColor.White, r)).ToArray());
        }

        public void RenderRow(Tuple<ConsoleColor, string>[] rowValues) {
            if (!_headerIsRendered) {
                RenderHeader();
                _headerIsRendered = true;
            }

            Console.SetCursorPosition(0, Console.CursorTop - 1);
            Console.Write("| ");
            for (var i = 0; i < rowValues.Length; i++) {
                Console.ForegroundColor = rowValues[i].Item1;
                Console.Write("{0,-" + _headers[i].Item2 + "}", rowValues[i].Item2);
                Console.ForegroundColor = ConsoleColor.White;
                if (i + 1 != rowValues.Length)
                    Console.Write(" | ");
            }
            Console.Write(" |\r\n");
            
            Console.WriteLine("| {0} |", "-".PadRight(_totalWidth, '-'));
            _tableHeight += 1;
        }

        public void ClearCursorPos() {
            Console.SetCursorPosition(0, Console.CursorTop - _tableHeight);
            _headerIsRendered = false;
            _tableHeight = 0;
        }

        private void RenderHeader() {
            Console.WriteLine("| {0} |", "-".PadRight(_totalWidth, '-'));
            Console.WriteLine("| {0, -" + _totalWidth + "} |", _title);
            Console.WriteLine("| {0} |", "-".PadRight(_totalWidth, '-'));
            Console.WriteLine(_rowTemplate, _headers.Select(h => h.Item1).ToArray());
            Console.WriteLine("| {0} |", "-".PadRight(_totalWidth, '-'));
            Console.WriteLine("| {0} |", "-".PadRight(_totalWidth, '-'));
            _tableHeight += 6;
        }

        public static IEnumerable<string> SplitString(string str, int length) {
            var result = new List<string>();
            foreach (var line in str.Split(new[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries))
                result.AddRange(SplitLine(line, length));

            return result;
        }

        public static IEnumerable<string> SplitLine(string str, int length) {
            var lines = new List<string>();
            do {
                if (str.Length < length) {
                    lines.Add(str);
                    return lines;
                }

                int space;
                if (((space = str.LastIndexOf(' ', length)) != -1) ||
                    ((space = str.IndexOf(' ', length)) != -1)) {
                    lines.Add(str.Substring(0, space));
                } else {
                    lines.Add(str);
                    return lines;
                }

                str = str.Substring(space, str.Length - space);
            } while (true);
        }
    }
}