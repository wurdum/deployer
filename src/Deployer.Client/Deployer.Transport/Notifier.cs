using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Deployer.Client.DeployServiceReference;

namespace Deployer.Transport
{
    public class Notifier
    {
        private Dictionary<string, ActionStepContext> _expectedSteps;
        private TableRenderer _renderer;

        public void NotifyAction(ActionContext context) {
            Console.WriteLine();
            _renderer = new TableRenderer(context.Name, new[] {
                new Tuple<string, int>("Status", 15),
                new Tuple<string, int>("Action", 20)
            });

            _expectedSteps = context.Steps.ToDictionary<ActionStepContext, string>(s => s.Name);
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
}