using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace IntelliSearch.FlexVersion.Console
{
    static class FlexVersionConsole
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

            var configurationFile = options.ContainsKey("-C") ? options["-C"].First() : @".\flexversion.yml";
            var repoPath = options.ContainsKey("-R") ? options["-R"].First() : Environment.CurrentDirectory;
            var variables = options.ContainsKey("-V") ? options["-V"] : new List<string>();
            _debugMode = options.ContainsKey("-D");

            try
            {
                var flexVersion = new FlexVersion(configurationFile, repoPath, variables.ToArray());

                var result = flexVersion.Analyze();
                var resultAsJson = _debugMode 
                    ? JsonConvert.SerializeObject(result, Formatting.Indented) 
                    : JsonConvert.SerializeObject(result.Output, Formatting.Indented);
                
                System.Console.WriteLine(resultAsJson);
            }
            catch (Exception ex)
            {
                Usage(_debugMode ? ex.ToString() : ex.Message);
            }
        }

        private static void Usage(string error = null)
        {
            System.Console.WriteLine();
            System.Console.WriteLine("FlexVersion (C) IntelliSearch Software AS");

            if (error != null)
            {
                System.Console.WriteLine();
                System.Console.Error.WriteLine($"*** Error: {error}");
            }

            System.Console.WriteLine();
            System.Console.WriteLine("Usage: FlexVersion.exe <-c configFile> <-r repoPath> <-v variables>");
            System.Console.WriteLine();
            System.Console.WriteLine("  -c configFile - The path to the configuration-file to use.");
            System.Console.WriteLine("  -r repoPath - The path to the repository to analyze.");
            System.Console.WriteLine("  -v variables - A list of variables. I.e. '-v Configuartion=Release, Server=BUILDSERVER'");
            System.Console.WriteLine("  -d - Add more details to output.");
            System.Console.WriteLine();

            if (error!= null) Environment.Exit(-1);
            Environment.Exit(0);
        }
    }
}
