using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using IntelliSearch.GitSemVer.Configuration;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace IntelliSearch.GitSemVer
{
    // https://regex101.com/r/ysJPjT/2/

    /// <summary>
    /// GitSemVer is a class that facilitates creating SemVer versions for your git repo, based on provided settings.
    /// </summary>
    public class GitSemVer
    {
        private readonly string _repoPath;
        private readonly GitSemVerConfiguration _gitSemVerConfiguration;
        private Dictionary<string, string> _arguments = new Dictionary<string, string>();

        /// <summary>
        /// Initialize GitSemVer with repoPath and given configuration-object.
        /// 
        /// </summary>
        /// <param name="gitSemVerConfiguration">The config-object to use.</param>
        /// <param name="repoPath">If null the current working directory is assumed.</param>
        public GitSemVer(GitSemVerConfiguration gitSemVerConfiguration, string repoPath = null, params string[] arguments)
        {
            _repoPath = repoPath ?? Environment.CurrentDirectory;
            _gitSemVerConfiguration = gitSemVerConfiguration;
            foreach (var argument in arguments)
            {
                var match = Regex.Match(argument, @"^(?<Variable>\w+)=(?<Value>\w+)$");
                if (match.Success)
                {
                    _arguments.Add(match.Groups["Variable"].Value, match.Groups["Value"].Value);
                }
            }
        }

        /// <summary>
        /// Initialize the repo with configuration from a Yaml-file.
        /// </summary>
        /// <param name="configurationAsYamlFilePath">If null it defaults to 'gitsemver.yml'</param>
        /// <param name="repoPath">If null current working directory is assumed.</param>
        /// <param name="arguments"></param>
        public GitSemVer(string configurationAsYamlFilePath = null, string repoPath = null, params string[] arguments)
        {
            _repoPath = repoPath ?? Environment.CurrentDirectory;
            configurationAsYamlFilePath = configurationAsYamlFilePath ?? @".\gitsemver.yml";

            if (!File.Exists(configurationAsYamlFilePath)) throw new ArgumentException($"The configuration-file cannot be found at '{configurationAsYamlFilePath}'");

            var deserializer = new Deserializer();
            var parser = new MergingParser(new Parser(File.OpenText(configurationAsYamlFilePath)));
            _gitSemVerConfiguration = deserializer.Deserialize<GitSemVerConfiguration>(parser);

            foreach (var argument in arguments)
            {
                var match = Regex.Match(argument, @"^(?<Variable>\w+)=(?<Value>\w+)$");
                if (match.Success)
                {
                    _arguments.Add(match.Groups["Variable"].Value, match.Groups["Value"].Value);
                }
            }
        }

        /// <summary>
        /// Analyzes the branch using the configuration given to identify versioning information as well as providing some git repo information.
        /// </summary>
        /// <returns>A result object with various GitSemVer-relevant versioning information.</returns>
        public Result Analyze()
        {
            var result = new Result();

            // ReSharper disable once InconsistentNaming
            var params_VS_Match_ = new Dictionary<string, string>();

            var activeCommits = new List<Commit>();

            BranchConfiguration branchConfig;

            using (var repo = new Repository(_repoPath))
            {
                var branchName = repo.Branches.First(b => b.IsCurrentRepositoryHead).FriendlyName;
                result.BranchConfiguration = BranchConfig(branchName, _gitSemVerConfiguration.Branches);
                branchConfig = result.BranchConfiguration.Value;

                // First we iterate all tags to find which of them that are candidates
                var tagCandidates =
                    new Dictionary<string,
                        (
                        LibGit2Sharp.Tag tag,
                        Dictionary<string, string> matchParts
                        )
                    >();

                if (branchConfig.VersionSource.Order.ContainsKey(VersionSourceType.Tag))
                {

                    foreach (var tag in repo.Tags)
                    {
                        var regex = new Regex(branchConfig.VersionSource.TagMatch.MatchPattern, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
                        var match = regex.Match(tag.FriendlyName);

                        if (!match.Success) continue;
                        
                        // Was match. Lets get the matches.
                        var matchParts = new Dictionary<string, string>();
                        var groupNames = regex.GetGroupNames().Where(i => i != "0");
                        foreach (var groupName in groupNames)
                        {
                            var groupValue = match.Groups[groupName].Value;
                            if (string.IsNullOrWhiteSpace(groupValue)) continue;
                            matchParts.Add(groupName, groupValue);
                        }
                        tagCandidates.Add(tag.Target.Sha,
                            (
                                tag,
                                matchParts
                            )
                        );
                    }
                }

                // Iterate commits until we find a version-source
                var commitFilter = new CommitFilter
                {
                    SortBy = CommitSortStrategies.Time,
                    // ReSharper disable once PossibleInvalidOperationException
                    FirstParentOnly = branchConfig.VersionSource.IterateFirstParentOnly.Value
                };

                var commits = branchConfig.VersionSource.MaxCommitsToAnalyze == null ||
                              branchConfig.VersionSource.MaxCommitsToAnalyze.Value <= 0
                    ? repo.Commits.QueryBy(commitFilter)
                    : repo.Commits.QueryBy(commitFilter).Take(branchConfig.VersionSource.MaxCommitsToAnalyze.Value);

                foreach (var c in commits)
                {
                    var commit = new Commit
                    {
                        Sha = c.Sha,
                        Message = c.Message,
                        MessageShort = c.MessageShort,
                        Author = c.Author.ToString(),
                        CommitDate = c.Committer.When.DateTime,
                        IsMerge = c.Parents.Count() > 1,
                    };

                    var versionSource = VersionSourceType.None;

                    foreach (var type in branchConfig.VersionSource.Order)
                    {
                        if (type.Key == VersionSourceType.Tag)
                        {
                            if (!tagCandidates.ContainsKey(c.Sha)) continue;

                            // This is the version-source
                            // Store the param variables
                            commit.Tag = new Tag(tagCandidates[c.Sha].tag);

                            foreach (var matchPart in tagCandidates[c.Sha].matchParts)
                            {
                                params_VS_Match_.Add(matchPart.Key, matchPart.Value);
                            }
                            versionSource = VersionSourceType.Tag;
                        }
                        else
                        {
                            if (versionSource != VersionSourceType.None || !commit.IsMerge) continue;

                            // Based on merge-message, what branch is the source?
                            var fromBranchName = string.Empty;
                            var toBranchName = string.Empty;
                            var fromBranchMatch = Regex.Match(c.Message, branchConfig.VersionSource.MergeMatch.MatchPattern);
                            if (fromBranchMatch.Success)
                            {
                                fromBranchName = fromBranchMatch.Groups["From"].Value;
                                toBranchName = fromBranchMatch.Groups["To"].Value;
                            }

                            FindBranchFromTo(fromBranchName, toBranchName, out var fromBranchConfigName, out var toBranchConfigName);
                            commit.FromBranchConfigName = fromBranchConfigName;
                            commit.ToBranchConfigName = toBranchConfigName;

                            foreach (var fromTo in branchConfig.VersionSource.MergeMatch.FromTo)
                            {
                                if (fromTo.Key != commit.FromBranchConfigName || fromTo.Value != commit.ToBranchConfigName) continue;

                                var regex = new Regex(branchConfig.VersionSource.TagMatch.MatchPattern, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
                                var match = regex.Match(c.MessageShort);

                                if (!match.Success) continue;

                                // This is the version-source
                                // Store the param-variables
                                var matchParts = regex.GetGroupNames().ToDictionary(groupName => groupName, groupName => match.Groups[groupName].Value);
                                foreach (var matchPart in matchParts)
                                {
                                    params_VS_Match_.Add(matchPart.Key, matchPart.Value);
                                }

                                versionSource = VersionSourceType.Merge;
                            }
                        }
                    }

                    activeCommits.Add(commit);

                    if (versionSource == VersionSourceType.None) continue;

                    // This commit is the version-source. We should add partial results break.
                    result.VersionSource = new VersionSource(commit, versionSource, params_VS_Match_);
                    result.GitInfo = new GitInfo(_repoPath, branchName, activeCommits);
                    break;
                }
            }

            // Find actions by iterating from oldest and to newest, executing actions per commit as we go along.
            // The oldest item is the version-source, and should not execute any actions.
            Dictionary<string, string> actions = null;
            for (var i = activeCommits.Count() - 2; i >= 0; i--)
            {
                var c = activeCommits[i];

                if (c.IsMerge)
                {
                    // Find OnMerge actions
                    // if OnMerge.Key has entry that matches this merge's from (or fallback *) then add actions
                    actions = branchConfig.OnMerge.ContainsKey(c.FromBranchConfigName)
                        ? branchConfig.OnMerge[c.FromBranchConfigName].ToDictionary(a => a.Key, a => a.Value)
                        : (
                            branchConfig.OnMerge.ContainsKey("*")
                                ? branchConfig.OnMerge["*"].ToDictionary(a => a.Key, a => a.Value)
                                : null
                        );
                }
                else
                {
                    if (branchConfig.OnCommit.Any())
                    {
                        // Add any OnCommit actions
                        actions = branchConfig.OnCommit.ToDictionary(a => a.Key, a => a.Value);
                    }
                }

                if (actions == null) continue;

                // Execute commit-actions
                foreach (var action in actions)
                {
                    // Match the actions
                    var match = Regex.Match(action.Value, @"^(?<Op>[+-=])(?<Value>\d+)$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        var op = match.Groups["Op"].Value;
                        var value = match.Groups["Value"].Value;
                        if (op == "=")
                        {
                            params_VS_Match_[action.Key] = value;
                        }
                        else
                        {
                            var temp = 0;
                            if (params_VS_Match_.ContainsKey(action.Key)) temp = int.Parse(params_VS_Match_[action.Key]);
                            temp = temp + int.Parse(match.Value);
                            params_VS_Match_[action.Key] = temp.ToString();
                        }
                    }
                    else
                    {
                        throw new ArgumentException(
                            "The merge-action was not understood. Should match '<[+-=]><number>'.");
                    }
                }
            }


            // TODO: Handle Output generation.

            //var BranchName = result.GitInfo.BranchName;
            //var DateTimeNow = DateTime.Now.ToString("YYMMDD-HHmmss");

            List<string> helperCommands = new List<string>{"IfNotEmpty", "SubString", "Length" };

            var outputParams = new Dictionary<string, string>();
            foreach (var output in branchConfig.Results.Output)
            {
                var resValue = output.Value;

                // Do typed variable expansion
                foreach (Match match in Regex.Matches(resValue, @"<(?<Type>\w+):(?<Var>.+?)>"))
                {
                    var type = match.Groups["Type"].Value;
                    var var = match.Groups["Var"].Value;
                    string temp;
                    switch (type)
                    {
                        case "Match":
                            temp = params_VS_Match_.ContainsKey(var) ? params_VS_Match_[var] : $"['{var}' not found]";
                            resValue = resValue.Replace(match.Value, temp);
                            break;
                        case "Env":
                            temp = Environment.GetEnvironmentVariable(match.Groups["Var"].Value);
                            if (string.IsNullOrWhiteSpace(temp)) temp = $"['{var}' not found]";
                            resValue = resValue.Replace(match.Value, temp);
                            break;
                        case "Arg":
                            temp = _arguments.ContainsKey(var) ? _arguments[match.Groups["Var"].Value] : $"['{var}' not found]";
                            resValue = resValue.Replace(match.Value, temp);
                            break;
                        case "VS":
                            temp = $"['{type}' not implemented]";
                            resValue = resValue.Replace(match.Value, temp);
                            break;
                        case "Common":
                            temp = $"['{type}' not implemented]";
                            resValue = resValue.Replace(match.Value, temp);
                            break;
                        case "Head":
                            temp = $"['{type}' not implemented]";
                            resValue = resValue.Replace(match.Value, temp);
                            break;
                    }
                }

                // Replace references to other so far generated output-vars.
                foreach (Match match in Regex.Matches(resValue, @"<(?<Var>.+?)>"))
                {
                    var var = match.Groups["Var"].Value;
                    var temp = outputParams.ContainsKey(var) ? outputParams[match.Groups["Var"].Value] : $"['{var}' not found]";
                    resValue = resValue.Replace(match.Value, temp);
                }

                // TODO: Do functions
                string FindFunction(string data)
                {
                    // First find if there are at least one function in there 
                    var match = Regex.Match(data, @"\$\w+\(.*\)");
                    if (!match.Success) return data;

                    // Find outermost parenthesis
                    var 

                        var func = match.Groups["Func"].Value;
                        var args = match.Groups["Args"].Value;

                        // Check if there are more functions called within
                        var res = FindFunction(args);
                        if (res != args)
                        {
                            data = data.Replace(args, res);
                        }

                        string temp;
                        switch (func)
                        {
                            case "$Length":
                                temp = args.Length.ToString();
                                data = data.Replace(match.Value, temp);
                                break;
                            case "$IfNotEmpty":
                                data = data.Replace(match.Value, $"[{func}({args}) not implemented]");
                                break;
                            case "$Substring":
                                data = data.Replace(match.Value, $"[{func}({args}) not implemented]");
                                break;
                            default:
                                data = data.Replace(match.Value, $"[{func}({args}) is unknown]");
                                break;
                        }

                    }
                    return data;
                }

                resValue = FindFunction(resValue);

                outputParams.Add(output.Key, resValue);

            }

            //// First replace all variables with values

            //    // Do all VS_Match_ replacements
            //    foreach (Match match in Regex.Matches(output.Value, @"(<VS_Match_(?<Var>.+?\b)>)"))
            //    {
            //        var key = match.Groups["Var"].Value;
            //        var value = params_VS_Match_.ContainsKey(key) ? params_VS_Match_[key] : "<NA>";
            //        resValue = resValue.Replace(match.Value, value);
            //    }

            //    // Do all Env_ replacements
            //    foreach (Match match in Regex.Matches(output.Value, @"(<Env_(?<Var>.+?\b)>)"))
            //    {
            //        var value = Environment.GetEnvironmentVariable(match.Groups["Var"].Value);
            //        resValue = resValue.Replace(match.Value, value);
            //    }

            //    // Do all Arg_ replacements
            //    foreach (Match match in Regex.Matches(output.Value, @"(<Arg_(?<Var>.+?\b)>)"))
            //    {
            //        var key = match.Groups["Var"].Value;
            //        var value = _arguments.ContainsKey(key) ? _arguments[match.Groups["Var"].Value] : "<NA>";
            //        resValue = resValue.Replace(match.Value, value);
            //    }


            result.Output = outputParams;

            return result;
        }

        private void FindBranchFromTo(string fromBranchName, string toBranchName, out string from, out string to)
        {
            from = null;
            to = null;
            foreach (var branch in _gitSemVerConfiguration.Branches)
            {
                if (string.IsNullOrWhiteSpace(branch.Value.Regex)) continue;

                var regex = new Regex(branch.Value.Regex, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
                if (from == null)
                {
                    var match = regex.Match(fromBranchName);
                    if (match.Success) from = branch.Key;
                }
                if (to == null)
                {
                    var match = regex.Match(toBranchName);
                    if (match.Success) to = branch.Key;
                }
            }
        }

        private static KeyValuePair<string, BranchConfiguration> BranchConfig(string branchName, Dictionary<string, BranchConfiguration> branches)
        {
            foreach (var entry in branches)
            {
                if (string.IsNullOrWhiteSpace(entry.Value.Regex)) continue;

                var match = Regex.Match(branchName, entry.Value.Regex, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
                if (!match.Success) continue;
                return new KeyValuePair<string, BranchConfiguration>(entry.Key, entry.Value);
            }

            if (branches.ContainsKey("*"))
            {
                return new KeyValuePair<string, BranchConfiguration>("*", branches["*"]);
            }

            throw new ArgumentException($"Unable to find a branch-definiton for '{branchName}' in the configuration. The required '*' fallback definition was also not found. Please correct the configuration and try again.");
        }

    }
}