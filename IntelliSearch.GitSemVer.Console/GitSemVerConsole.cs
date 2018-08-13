using System.Linq;
using Newtonsoft.Json;

namespace IntelliSearch.GitSemVer.Console
{
    using System;

    class GitSemVerConsole
    {
        static void Main(string[] args)
        {
            var configurationFile = args.Length > 0 ? args[0] : @".\gitsemver.yml";
            var repoPath = args.Length > 1 ? args[1] : Environment.CurrentDirectory;

            var variables = args.Skip(2);
            var gitSemVer = new GitSemVer(configurationFile, repoPath, variables.ToArray());
            var result = gitSemVer.Analyze();

            var resultAsJson = JsonConvert.SerializeObject(result, Formatting.Indented);

            Console.WriteLine(resultAsJson);
        }
    }
}
