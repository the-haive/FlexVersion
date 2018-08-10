using Newtonsoft.Json;

namespace IntelliSearch.GitSemVer.Console
{
    using System;

    class GitSemVerConsole
    {
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                var settingsFile = args[0];
                var repoPath = args.Length > 1 ? args[1] : null;

                var gitSemVer = new GitSemVer(settingsFile, repoPath);
                var result = gitSemVer.Analyze();

                var resultAsJson = JsonConvert.SerializeObject(result, Formatting.Indented);

                Console.WriteLine($"Repo at'{repoPath}'. Results:");
                Console.WriteLine(resultAsJson);
                Console.WriteLine($"PreRelease without metadata: {result.VersionInfo.PreReleaseVersion()}");
                Console.WriteLine($"PreRelease with metadata: {result.VersionInfo.PreReleaseVersion(result.GitInfo.Branch, result.GitInfo.LastAuthor, result.GitInfo.LastCommitDate.ToShortDateString(), result.GitInfo.Head.Sha)}");
            }
            else
            {
                Console.WriteLine("This program needs arguments a repo and max commits to iterate.");
            }
        }
    }
}
