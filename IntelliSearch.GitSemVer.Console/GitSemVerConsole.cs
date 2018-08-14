using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace IntelliSearch.GitSemVer.Console
{
    using System;

    static class GitSemVerConsole
    {
        private static bool _debugMode;

        static void Main(string[] args)
        {
            var argList = args.ToList();
            if (argList.Any(i => i.ToLowerInvariant().Equals("help"))) Usage();
            var options = new Dictionary<string, List<string>>();

            string lastKey = null;
            foreach (var arg in argList)
            {
                if (arg.StartsWith("-"))
                {
                    lastKey = arg.ToUpperInvariant();
                    if (options.ContainsKey(lastKey)) Usage("Arguments for the same option must be placed together.");
                    options.Add(lastKey, new List<string>());
                }
                else
                {
                    if (lastKey == null) Usage("Options must start with -");
                    options[lastKey].Add(arg);
                }
            }

            var configurationFile = options.ContainsKey("-C") ? options["-C"].First() : @".\gitsemver.yml";
            var repoPath = options.ContainsKey("-R") ? options["-R"].First() : Environment.CurrentDirectory;
            var variables = options.ContainsKey("-V") ? options["-V"] : new List<string>();
            _debugMode = options.ContainsKey("-D");

            try
            {
                var gitSemVer = new GitSemVer(configurationFile, repoPath, variables.ToArray());

                var result = gitSemVer.Analyze();
                var resultAsJson = _debugMode 
                    ? JsonConvert.SerializeObject(result, Formatting.Indented) 
                    : JsonConvert.SerializeObject(result.Output, Formatting.Indented);
                
                Console.WriteLine(resultAsJson);
            }
            catch (Exception ex)
            {
                if (_debugMode)
                {
                    Usage(ex.ToString());
                }
                else
                {
                    Usage(ex.Message);
                }
            }
        }

        private static void Usage(string error = null)
        {
            Console.WriteLine();
            Console.WriteLine("GitSemVer (C) IntelliSearch Software AS");

            if (error != null)
            {
                Console.WriteLine();
                Console.Error.WriteLine($"*** Error: {error}");
            }

            Console.WriteLine();
            Console.WriteLine("Usage: gitsemver <-c configFile> <-r repoPath> <-v variables>");
            Console.WriteLine();
            Console.WriteLine("  -c configFile - The path to the configuration-file to use.");
            Console.WriteLine("  -r repoPath - The path to the repository to analyze.");
            Console.WriteLine("  -v variables - A list of variables. I.e. '-v Configuartion=Release, Server=BUILDSERVER'");
            Console.WriteLine("  -d - Add more details to output.");
            Console.WriteLine();

            if (error!= null) Environment.Exit(-1);
            Environment.Exit(0);
        }
    }
}
