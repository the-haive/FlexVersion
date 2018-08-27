using Newtonsoft.Json;
using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IntelliSearch.FlexVersion.Console
{
    internal static class FlexVersionConsole
    {
        private static readonly Logger Logger = LogManager.GetLogger("Example");

        public static void Main(string[] args)
        {
            SetupLogging();

            ParseOptions(args, out var configurationFile, out var repoPath, out var variables, out var diagnostic);

            try
            {
                Logger.Trace("Starting to run FlexVersion analysis...");
                var flexVersion = new FlexVersion(configurationFile, repoPath, variables.ToArray());
                var result = flexVersion.Analyze();
                Logger.Trace("Done analysing.");

                System.Console.WriteLine(diagnostic
                    ? JsonConvert.SerializeObject(result, Formatting.Indented)
                    : JsonConvert.SerializeObject(result.Output, Formatting.Indented));
            }
            catch (Exception ex)
            {
                Logger.Fatal(diagnostic ? ex.ToString() : ex.Message);
                Environment.Exit(-1);
            }
        }

        private static void ConfigureLogLevel(string logLevel)
        {
            var validOptions = string.Join(",", LogLevel.AllLevels);
            if (string.IsNullOrWhiteSpace(logLevel))
            {
                Logger.Error($"Missing LogLevel value.");
                Usage(-2);
            }

            try
            {
                var level = LogLevel.FromString(logLevel);

                foreach (var rule in LogManager.Configuration.LoggingRules)
                {
                    rule.SetLoggingLevels(level, LogLevel.Fatal);
                }

                //Call to update existing Loggers created with GetLogger() or GetCurrentClassLogger()
                LogManager.ReconfigExistingLoggers();
            }
            catch
            {
                Logger.Error($"Invalid LogLevel value.");
                Usage(-2);
            }
        }

        private static void ParseOptions(IEnumerable<string> args, out string configurationFile, out string repoPath, out List<string> arguments, out bool diagnostic)
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
                    if (options.ContainsKey(lastKey))
                    {
                        Logger.Error("Arguments for the same option must be placed together.");
                        Usage(-2);
                    }
                    options.Add(lastKey, new List<string>());
                }
                else
                {
                    if (lastKey == null)
                    {
                        Logger.Error("Options must start with -");
                        Usage(-2);
                    }
                    options[lastKey].Add(arg);
                }
            }

            if (options.ContainsKey("-L")) ConfigureLogLevel(options["-L"].FirstOrDefault());
            configurationFile = options.ContainsKey("-C") ? options["-C"].First() : @".\flexversion.yml";
            repoPath = options.ContainsKey("-R") ? options["-R"].First() : Environment.CurrentDirectory;
            arguments = options.ContainsKey("-A") ? options["-A"] : new List<string>();
            diagnostic = options.ContainsKey("-D");
        }

        private static void SetupLogging()
        {
            // Step 1. Create configuration object
            var config = new LoggingConfiguration();

            // Step 2. Create targets
            var consoleTarget = new ColoredConsoleTarget("target1")
            {
                Layout = @"${level}> ${message} ${exception}"
            };

            config.AddTarget(consoleTarget);


            // Step 3. Define rules
            config.AddRuleForAllLevels(consoleTarget);

            // Step 4. Activate the configuration
            LogManager.Configuration = config;
            ConfigureLogLevel(LogLevel.Info.ToString());
        }

        private static void Usage(int exitCode = 0)
        {
            var validLogLevels = string.Join("|", LogLevel.AllLevels);

            System.Console.WriteLine();
            System.Console.WriteLine("FlexVersion (C) IntelliSearch Software AS");
            System.Console.WriteLine();
            System.Console.WriteLine("Usage: FlexVersion.exe [-c configFile] [-r repoPath] [-a arguments] [-l logLevel] [-d]");
            System.Console.WriteLine();
            System.Console.WriteLine(@"  -c configFile # The path to the configuration-file to use. Default: ./flexVersion.yml");
            System.Console.WriteLine(@"  -r repoPath # The path to the repository to analyze. Default: ./");
            System.Console.WriteLine(@"  -a arguments # A list of key=value arguments to be used in the construction of the outputs. I.e. '-a Configuration=Release Server=BUILDSERVER'. Default: None");
            System.Console.WriteLine($"  -l <{validLogLevels}> # Adjust loglevel. Default: Info");
            System.Console.WriteLine(@"  -d # Turn on diagnostic-mode, adding stacktrace for errors and dumping the full result-object.");
            System.Console.WriteLine();

            Environment.Exit(exitCode);
        }
    }
}