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

            var gitSemVer = new GitSemVer(configurationFile, repoPath);
            var result = gitSemVer.Analyze();

            var resultAsJson = JsonConvert.SerializeObject(result, Formatting.Indented);

            Console.WriteLine(resultAsJson);
            //Console.WriteLine($"PreRelease without metadata: {result.VersionInfo.PreReleaseVersion()}");
            //Console.WriteLine($"PreRelease with metadata: {result.VersionInfo.PreReleaseVersion(result.GitInfo.Branch, result.GitInfo.LastAuthor, result.GitInfo.LastCommitDate.ToShortDateString(), result.GitInfo.Head.Sha)}");
        }
    }
}
