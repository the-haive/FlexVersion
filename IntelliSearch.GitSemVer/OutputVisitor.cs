using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

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
            var result = VisitChildren(context);
            var method = context.children[0].GetText().Substring(1); // Skip $
            var args = context.children[2].GetText().Split(',');
            switch (method.ToLowerInvariant())
            {
                case "length":
                    if (args.Length != 1) throw new ArgumentException("Error: The Length method takes 1 argument.");
                    return args[0].Length.ToString();

                case "substring":
                    if (args.Length != 3) throw new ArgumentException("Error: The Substring method takes 3 arguments.");
                    if (!int.TryParse(args[1], out var from)) throw new ArgumentException("Error: The Substring method's 2nd argument must be convertable to an integer.");
                    if (!int.TryParse(args[2], out var to)) throw new ArgumentException("Error: The Substring method's 3rd argument must be convertable to an integer.");
                    return args[0].Substring(from, to);

                case "ifnotempty":
                    if (args.Length != 2) throw new ArgumentException("Error: The IfNotEmpty method takes 2 arguments.");
                    return string.IsNullOrWhiteSpace(args[0]) ? string.Empty : args[1];

                case "ifnotemptyelse":
                    if (args.Length != 3) throw new ArgumentException("Error: The IfNotEmptyElse method takes 3 arguments.");
                    return string.IsNullOrWhiteSpace(args[0]) ? args[2] : args[1];

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