using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Data;

using Antlr4.Runtime.Tree;

using IntelliSearch.FlexVersion.Logging;

namespace IntelliSearch.FlexVersion
{
    internal class OutputVisitor : OutputBaseVisitor<string>
    {
        private static readonly ILog Logger = LogProvider.For<OutputVisitor>();
        private readonly Dictionary<string, string> _arg;
        private readonly Dictionary<string, string> _gitInfo;
        private readonly Dictionary<string, string> _head;
        private readonly Dictionary<string, string> _match;
        private readonly Dictionary<string, string> _output;
        private readonly Dictionary<string, string> _vs;
        private readonly string _templateValue;
        private readonly string _templateName;

        public OutputVisitor(
            string templateName,
            string templateValue,
            Dictionary<string, string> arg,
            Dictionary<string, string> gitInfo,
            Dictionary<string, string> head,
            Dictionary<string, string> match,
            Dictionary<string, string> output,
            Dictionary<string, string> vs)
        {
            _templateName = templateName;
            _templateValue = templateValue;
            _arg = arg;
            _gitInfo = gitInfo;
            _head = head;
            _match = match;
            _output = output;
            _vs = vs;
        }

        public override string VisitFunction(global::OutputParser.FunctionContext context)
        {
            var args = new List<string>();
            var paramContext = ((IRuleNode)context.GetChild(2)).RuleContext;
            var res = new StringBuilder();
            bool lastWasComma = false;
            bool lastHasValue = false;
            for (var i = 0; i < paramContext.ChildCount; i++)
            {
                var child = paramContext.GetChild(i);
                if (child.ChildCount == 0)
                {
                    args.Add(res.ToString());
                    res.Clear();
                    lastWasComma = true;
                    lastHasValue = false;
                    continue;
                }

                lastWasComma = false;
                lastHasValue = true;
                res.Append(VisitChildren(((IRuleNode) child).RuleContext));
            }

            if (lastHasValue || lastWasComma)
            {
                args.Add(res.ToString());
            }

            var method = context.children[0].GetText().Substring(1); // Skip the $ character
            string errMsg;
            switch (method.ToLowerInvariant())
            {
                case "if":
                    if (args.Count != 3) { errMsg = "The $If method takes 3 arguments."; break; }
                    if (!bool.TryParse(args[0].Trim(), out var expr)) { errMsg = "The $If method's 1st argument must be convertable to a boolean."; break; }
                    return expr ? args[1] : args[2];

                case "ifnot":
                    if (args.Count != 3) { errMsg = "The $IfNot method takes 3 arguments."; break; }
                    if (!bool.TryParse(args[0].Trim(), out var exprNot)) { errMsg = "The $IfNot method's 1st argument must be convertable to a boolean."; break; }
                    return !exprNot ? args[1] : args[2];

                case "ifblank":
                    if (args.Count != 3) { errMsg = "The $IfBlank method takes 3 arguments."; break; }
                    return string.IsNullOrWhiteSpace(args[0]) ? args[1] : args[2];

                case "ifnotblank":
                    if (args.Count != 3) { errMsg = "The $IfNotBlank method takes 3 arguments."; break; }
                    return !string.IsNullOrWhiteSpace(args[0]) ? args[1] : args[2];

                case "equal":
                    if (args.Count != 2) { errMsg = "The $Equal method takes 2 arguments."; break; }
                    return (args[0] == args[1]).ToString();

                case "notequal":
                    if (args.Count != 2) { errMsg = "The $NotEqual method takes 2 arguments."; break; }
                    return (args[0] != args[1]).ToString();

                case "index":
                    if (args.Count != 2) { errMsg = "The $Index method takes 2 arguments."; break; }
                    return args[0].IndexOf(args[1], StringComparison.Ordinal).ToString();

                case "lastindex":
                    if (args.Count != 2) { errMsg = "The $LastIndex method takes 2 arguments."; break; }
                    return args[0].LastIndexOf(args[1], StringComparison.Ordinal).ToString();

                case "length":
                    if (args.Count != 1) { errMsg = "The $Length method takes 1 argument."; break; }
                    return args[0].Length.ToString();

                case "calc":
                    if (args.Count != 1) { errMsg = "The $Calc method takes 1 argument."; break; }
                    return new DataTable().Compute(args[0], null).ToString();

                case "regexmatch":
                    if (args.Count != 2) { errMsg = "The $RegexMatch method takes 2 arguments."; break; }
                    return Regex.Match(args[0], args[1]).Success.ToString();

                case "regexreplace":
                    if (args.Count != 3) { errMsg = "The $RegexReplace method takes 3 arguments."; break; }
                    return Regex.Replace(args[0], args[1], args[2]);

                case "padleft":
                    if (args.Count != 3) { errMsg = "The $PadLeft method takes 3 arguments."; break; }
                    if (!int.TryParse(args[1].Trim(), out var totalWidthLeft)) { errMsg = "The $PadLeft method's 2nd argument must be convertable to an integer."; break; }
                    if (args[2].Length != 1) { errMsg = "The $PadLeft method's 3rd argument must be a single character."; break; }
                    return args[0].PadLeft(totalWidthLeft, args[2][0]);

                case "padright":
                    if (args.Count != 3) { errMsg = "The $PadRight method takes 3 arguments."; break; }
                    if (!int.TryParse(args[1].Trim(), out var totalWidthRight)) { errMsg = "The $PadRight method's 2nd argument must be convertable to an integer."; break; }
                    if (args[2].Length != 1) { errMsg = "The $PadRight method's 3rd argument must be a single character."; break; }
                    return args[0].PadRight(totalWidthRight, args[2][0]);

                case "substring":
                    if (args.Count != 3) { errMsg = "The $Substring method takes 3 arguments."; break; }
                    if (!int.TryParse(args[1].Trim(), out var from)) { errMsg = "The $Substring method's 2nd argument must be convertable to an integer."; break; }
                    if (!int.TryParse(args[2].Trim(), out var to)) { errMsg = "The $Substring method's 3rd argument must be convertable to an integer."; break; }
                    if (from + to > args[0].Length) to = args[0].Length - from;
                    return args[0].Substring(from, to);

                case "trim":
                    if (args.Count != 1) { errMsg = "The $Trim method takes 1 argument."; break; }
                    return args[0].Trim();

                case "trimstart":
                    if (args.Count != 1) { errMsg = "The $TrimStart method takes 1 argument."; break; }
                    return args[0].TrimStart();

                case "trimend":
                    if (args.Count != 1) { errMsg = "The $TrimEnd method takes 1 argument."; break; }
                    return args[0].TrimEnd();

                case "datetime":
                    if (args.Count != 2) { errMsg = "The $DateTime method takes 2 arguments."; break; }
                    if (!DateTime.TryParse(args[0], out var dateTime)) { errMsg = "The $DateTime method's 1st parameter was not convertable to a DateTime."; break; }
                    return dateTime.ToString(args[1]);

                case "datetimenow":
                    if (args.Count != 1) { errMsg = "The $DateTimeNow method takes 1 argument."; break; }
                    return DateTime.Now.ToString(args[0]);

                case "env":
                    if (args.Count != 1) { errMsg = "The $Env method takes 1 argument."; break; }
                    var env = Environment.GetEnvironmentVariable(args[0]) ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(env)) Logger.Warn($"$Env({args[0]}) is empty. This may or may not be intentional. Used in: {_templateName}='{_templateValue}'");
                    return env;

                case "arg":
                    if (args.Count != 1) { errMsg = "The $Arg method takes 1 argument."; break; }
                    var arg = _arg.ContainsKey(args[0]) ? _arg[args[0]] : string.Empty;
                    if (string.IsNullOrWhiteSpace(arg)) Logger.Warn($"$Arg({args[0]}) is empty. This may or may not be intentional. Used in: {_templateName}='{_templateValue}'");
                    return arg;

                case "match":
                    if (args.Count != 1) { errMsg = "The $Match method takes 1 argument."; break; }
                    var match = _match.ContainsKey(args[0]) ? _match[args[0]] : string.Empty;
                    if (string.IsNullOrWhiteSpace(match)) Logger.Warn($"$Match({args[0]}) is empty. This may or may not be intentional. Used in: {_templateName}='{_templateValue}'");
                    return match;

                case "versionsource":
                    if (args.Count != 1) { errMsg = "The $VersionSource method takes 1 argument."; break; }
                    var vs = _vs.ContainsKey(args[0]) ? _vs[args[0]] : null;
                    if (vs == null) { errMsg = $"$VersionSource({args[0]}) is undefined."; break; }
                    return vs;

                case "gitinfo":
                    if (args.Count != 1) { errMsg = "The $GitInfo method takes 1 argument."; break; }
                    var gi = _gitInfo.ContainsKey(args[0]) ? _gitInfo[args[0]] : null;
                    if (gi == null) { errMsg = $"$GitInfo({args[0]}) is undefined."; break; }
                    return gi;

                case "head":
                    if (args.Count != 1) { errMsg = "The $Head method takes 1 argument."; break; }
                    var head = _head.ContainsKey(args[0]) ? _head[args[0]] : null;
                    if (head == null) { errMsg = $"$Head({args[0]}) is undefined."; break; }
                    return head;

                default:
                    errMsg = $"Unknown method $'{method}({string.Join(",", args)})'.";
                    break;
            }

            Logger.Error($"{errMsg} Used in: {_templateName}='{_templateValue}'");
            return $"{method}({string.Join(",", args)})";
        }

        public override string VisitText(global::OutputParser.TextContext context)
        {
            return context.GetText();
        }

        public override string VisitVariable(global::OutputParser.VariableContext context)
        {
            var text = context.GetText();
            var inner = text.Substring(1, text.Length - 2);

            if (_output.ContainsKey(inner)) return _output[inner];

            Logger.Error($"The statement '<{text}>' is not recognixed as a supported variable. Used in: {_templateName}='{_templateValue}')");
            return $"{text}";
        }

        protected override string AggregateResult(string aggregate, string nextResult)
        {
            if (aggregate == null)
            {
                return nextResult;
            }

            if (nextResult == null)
            {
                return aggregate;
            }

            return $"{aggregate}{nextResult}";
        }
    }
}