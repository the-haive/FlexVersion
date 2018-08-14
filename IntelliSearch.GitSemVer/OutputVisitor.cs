using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Antlr4.Runtime.Tree;

namespace IntelliSearch.GitSemVer
{
    internal class OutputVisitor : OutputBaseVisitor<string>
    {
        private readonly Dictionary<string, string> _arg;
        private readonly Dictionary<string, string> _common;
        private readonly Dictionary<string, string> _head;
        private readonly Dictionary<string, string> _match;
        private readonly Dictionary<string, string> _output;
        private readonly Dictionary<string, string> _vs;

        public OutputVisitor(
            Dictionary<string, string> arg,
            Dictionary<string, string> common,
            Dictionary<string, string> head,
            Dictionary<string, string> match,
            Dictionary<string, string> output,
            Dictionary<string, string> vs
        )
        {
            _arg = arg;
            _common = common;
            _head = head;
            _match = match;
            _output = output;
            _vs = vs;
        }

        public override string VisitFunction(global::OutputParser.FunctionContext context)
        {
            // TODO: Fix the return of , (to something that is easy to split on)
            var args = new List<string>();
            var paramContext = ((IRuleNode)context.GetChild(2)).RuleContext; // 2nd index is always the params
            var res = new StringBuilder();
            for (var i = 0; i < paramContext.ChildCount; i++)
            {
                var child = paramContext.GetChild(i);
                if (child.ChildCount == 0)
                {
                    args.Add(res.ToString());
                    res.Clear();
                    continue;
                }
                res.Append(VisitChildren(((IRuleNode) child).RuleContext));
            }
            if (res.Length > 0)
            {
                args.Add(res.ToString());
            }

            var method = context.children[0].GetText().Substring(1); // Skip the $ character
            switch (method.ToLowerInvariant())
            {
                case "add":
                    if (args.Count != 2) throw new ArgumentException("Error: The Add method takes 2 arguments.");
                    if (!int.TryParse(args[0].Trim(), out var aa)) throw new ArgumentException("Error: The Add method's 1nd argument must be convertable to an integer.");
                    if (!int.TryParse(args[1].Trim(), out var ab)) throw new ArgumentException("Error: The Add method's 2rd argument must be convertable to an integer.");
                    return (aa+ab).ToString();

                case "sub":
                    if (args.Count != 2) throw new ArgumentException("Error: The Sub method takes 2 arguments.");
                    if (!int.TryParse(args[0].Trim(), out var sa)) throw new ArgumentException("Error: The Sub method's 1nd argument must be convertable to an integer.");
                    if (!int.TryParse(args[1].Trim(), out var sb)) throw new ArgumentException("Error: The Sub method's 2rd argument must be convertable to an integer.");
                    return (sa - sb).ToString();

                case "mul":
                    if (args.Count != 2) throw new ArgumentException("Error: The Mul method takes 2 arguments.");
                    if (!int.TryParse(args[0].Trim(), out var ma)) throw new ArgumentException("Error: The Mul method's 1nd argument must be convertable to an integer.");
                    if (!int.TryParse(args[1].Trim(), out var mb)) throw new ArgumentException("Error: The Mul method's 2rd argument must be convertable to an integer.");
                    return (ma * mb).ToString();

                case "div":
                    if (args.Count != 2) throw new ArgumentException("Error: The Mul method takes 2 arguments.");
                    if (!int.TryParse(args[0].Trim(), out var da)) throw new ArgumentException("Error: The Div method's 1nd argument must be convertable to an integer.");
                    if (!int.TryParse(args[1].Trim(), out var db)) throw new ArgumentException("Error: The Div method's 2rd argument must be convertable to an integer.");
                    return (da / db).ToString();

                case "length":
                    if (args.Count != 1) throw new ArgumentException("Error: The Length method takes 1 argument.");
                    return args[0].Length.ToString();

                case "padleft":
                    if (args.Count != 3) throw new ArgumentException("Error: The PadLeft method takes 3 arguments.");
                    if (!int.TryParse(args[1].Trim(), out var totalWidth)) throw new ArgumentException("Error: The PadLeft method's 2nd argument must be convertable to an integer.");
                    if (args[2].Length != 1) throw new ArgumentException("Error: The PadLeft method's 3rd argument must be a single character.");
                    return args[0].PadLeft(totalWidth, args[2][0]);

                case "substring":
                    if (args.Count != 3) throw new ArgumentException("Error: The Substring method takes 3 arguments.");
                    if (!int.TryParse(args[1].Trim(), out var from)) throw new ArgumentException("Error: The Substring method's 2nd argument must be convertable to an integer.");
                    if (!int.TryParse(args[2].Trim(), out var to)) throw new ArgumentException("Error: The Substring method's 3rd argument must be convertable to an integer.");
                    if (from + to > args[0].Length) to = args[0].Length - from;
                    return args[0].Substring(from, to);

                case "replaceifempty":
                    if (args.Count != 2) throw new ArgumentException("Error: The ReplaceIfEmpty method takes 2 arguments.");
                    return string.IsNullOrWhiteSpace(args[0]) ? args[1] : args[0];

                case "ifempty":
                    if (args.Count != 2) throw new ArgumentException("Error: The IfEmpty method takes 2 arguments.");
                    return string.IsNullOrWhiteSpace(args[0]) ? args[1] : string.Empty;

                case "replaceifnotempty":
                    if (args.Count != 2) throw new ArgumentException("Error: The ReplaceIfNotEmpty method takes 2 arguments.");
                    return !string.IsNullOrWhiteSpace(args[0]) ? args[1] : args[0];

                case "ifnotempty":
                    if (args.Count != 2) throw new ArgumentException("Error: The IfNotEmpty method takes 2 arguments.");
                    return !string.IsNullOrWhiteSpace(args[0]) ? args[1] : string.Empty;

                case "ifemptyelse":
                    if (args.Count != 3) throw new ArgumentException("Error: The IfEmptyElse method takes 3 arguments.");
                    return string.IsNullOrWhiteSpace(args[0]) ? args[1] : args[2];

                case "ifnotemptyelse":
                    if (args.Count != 3) throw new ArgumentException("Error: The IfNotEmptyElse method takes 3 arguments.");
                    return !string.IsNullOrWhiteSpace(args[0]) ? args[1] : args[2];

                default:
                    throw new ArgumentException($"Error: Unknown method '{method}'.");
            }
        }

        public override string VisitText(global::OutputParser.TextContext context)
        {
            return context.GetText();
        }

        public override string VisitVariable(global::OutputParser.VariableContext context)
        {
            var text = context.GetText();
            var inner = text.Substring(1, text.Length - 2);
            var match = Regex.Match(inner, @"(?<Type>\w+):(?<Key>.+)",
                RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

            if (match.Success)
            {
                var type = match.Groups["Type"].Value;
                var key = match.Groups["Key"].Value;
                switch (type.ToLowerInvariant())
                {
                    case "arg":
                        if (_arg.ContainsKey(key))
                        {
                            return _arg[key];
                        }

                        throw new ArgumentException($"Arg:'{key}' was not found.");
                    case "common":
                        if (_common.ContainsKey(key))
                        {
                            return _common[key];
                        }

                        throw new ArgumentException($"Common:'{key}' was not found.");
                    case "head":
                        if (_head.ContainsKey(key))
                        {
                            return _head[key];
                        }

                        throw new ArgumentException($"Head:'{key}' was not found.");
                    case "match":
                        if (_match.ContainsKey(key))
                        {
                            return _match[key];
                        }

                        throw new ArgumentException($"Match:'{key}' was not found.");
                    case "vs":
                        if (_vs.ContainsKey(key))
                        {
                            return _vs[key];
                        }

                        throw new ArgumentException($"VS:'{key}' was not found.");
                    case "env":
                        return Environment.GetEnvironmentVariable(key);

                    default:
                        throw new ArgumentException($"The type of the variable '{text}' is not recognized.");
                }
            }

            return _output.ContainsKey(inner)
                ? _output[inner]
                : throw new ArgumentException($"The varaiable '{text}' is not recognized as a variable.");
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