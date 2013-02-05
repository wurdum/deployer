using System;
using System.Collections.Generic;
using System.Linq;

namespace Deployer.Transport
{
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