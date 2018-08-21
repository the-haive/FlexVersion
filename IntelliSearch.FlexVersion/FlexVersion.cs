using Antlr4.Runtime;
using IntelliSearch.FlexVersion.Configuration;
using IntelliSearch.FlexVersion.Logging;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using Parser = YamlDotNet.Core.Parser;

// ReSharper disable InconsistentNaming

namespace IntelliSearch.FlexVersion
{
    /// <summary>
    /// FlexVersion is a class that facilitates creating SemVer versions for your git repo, based on provided settings.
    /// </summary>
    public class FlexVersion
    {
        private static readonly ILog Logger = LogProvider.For<FlexVersion>();
        private readonly FlexVersionConfiguration _flexVersionConfiguration;
        private readonly Dictionary<string, string> _paramArgs = new Dictionary<string, string>();
        private readonly string _repoPath;

        /// <summary>
        /// Initialize FlexVersion with repoPath and given configuration-object.
        ///
        /// </summary>
        /// <param name="flexVersionConfiguration">The config-object to use.</param>
        /// <param name="repoPath">If null the current working directory is assumed.</param>
        /// <param name="arguments">any string arguments that may be used when generating variables.</param>
        public FlexVersion(FlexVersionConfiguration flexVersionConfiguration, string repoPath = null, params string[] arguments)
        {
            _repoPath = repoPath ?? Environment.CurrentDirectory;
            Logger.Debug($"Repo-location: {repoPath}");

            _flexVersionConfiguration = flexVersionConfiguration;

            ParseArguments(arguments);
        }

        /// <summary>
        /// Initialize the repo with configuration from a Yaml-file.
        /// </summary>
        /// <param name="configurationAsYamlFilePath">If null it defaults to 'flexversion.yml'</param>
        /// <param name="repoPath">If null current working directory is assumed.</param>
        /// <param name="arguments"></param>
        public FlexVersion(string configurationAsYamlFilePath = null, string repoPath = null, params string[] arguments)
        {
            _repoPath = repoPath ?? Environment.CurrentDirectory;
            configurationAsYamlFilePath = configurationAsYamlFilePath ?? @".\flexversion.yml";

            if (!File.Exists(configurationAsYamlFilePath)) { throw new ArgumentException($"The configuration-file cannot be found at '{configurationAsYamlFilePath}'"); }

            var deserializer = new Deserializer();
            var parser = new MergingParser(new Parser(File.OpenText(configurationAsYamlFilePath)));
            _flexVersionConfiguration = deserializer.Deserialize<FlexVersionConfiguration>(parser);

            ParseArguments(arguments);
        }

        /// <summary>
        /// Analyzes the branch using the configuration given to identify versioning information as well as providing some git repo information.
        /// </summary>
        /// <returns>A result object with various FlexVersion-relevant versioning information.</returns>
        public Results Analyze()
        {
            var result = new Results();

            var paramMatch = new Dictionary<string, string>();

            var activeCommits = new List<Commit>();

            Logger.Trace("Finding VersionSource...");
            var branchConfig = FindVersionSource(result, paramMatch, activeCommits);

            Logger.Trace("Executing commit/merge actions...");
            ExecuteActions(paramMatch, activeCommits, branchConfig);

            Logger.Trace("Generating output...");
            GenerateOutputVariables(result, paramMatch, branchConfig);

            return result;
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

        private static void ExecuteActions(Dictionary<string, string> paramMatch_, List<Commit> activeCommits, BranchConfiguration branchConfig)
        {
            // Find actions by iterating from oldest and to newest, executing actions per commit as we go along.
            // The oldest item is the version-source, and should not execute any actions.
            Dictionary<string, string> actions = null;
            using (LogProvider.OpenNestedContext("Execute actions"))
            {
                for (var i = activeCommits.Count() - 2; i >= 0; i--)
                {
                    var c = activeCommits[i];

                    if (c.IsMerge)
                    {
                        // Find OnMerge actions
                        // if OnMerge.Key has entry that matches this merge's from (or fallback *) then add actions
                        if (!string.IsNullOrWhiteSpace(c.FromBranchConfigName)
                            && branchConfig.OnMerge.ContainsKey(c.FromBranchConfigName))
                            actions = branchConfig.OnMerge[c.FromBranchConfigName].ToDictionary(a => a.Key, a => a.Value);
                        else
                            actions = branchConfig.OnMerge.ContainsKey("*")
                                          ? branchConfig.OnMerge["*"].ToDictionary(a => a.Key, a => a.Value)
                                          : null;
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

                    // Execute actions
                    foreach (var action in actions)
                    {
                        using (LogProvider.OpenNestedContext($"Handling action '{action.Key}': '{action.Value}'"))
                        {
                            // Match the action
                            var match = Regex.Match(action.Value, @"^(?<Op>[+-=])(?<Value>\d+)$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
                            if (match.Success)
                            {
                                var op = match.Groups["Op"].Value;
                                var value = match.Groups["Value"].Value;
                                if (op == "=")
                                {
                                    Logger.Debug($"Setting '{action.Key}' => {value}");
                                    paramMatch_[action.Key] = value;
                                }
                                else
                                {
                                    var temp = 0;
                                    if (paramMatch_.ContainsKey(action.Key)) temp = int.Parse(paramMatch_[action.Key]);
                                    var intValue = int.Parse(match.Value) * (op == "-" ? -1 : 1);
                                    temp = temp + intValue;
                                    Logger.Debug($"Setting '{action.Key}' {op}= {value} => {temp}");
                                    paramMatch_[action.Key] = temp.ToString();
                                }
                            }
                            else
                            {
                                Logger.Error("The action was not understood. Should match '<[+-=]><number>'.");
                                throw new ArgumentException("The action was not understood. Should match '<[+-=]><number>'.");
                            }
                        }
                    }
                }
            }
        }

        private void FindBranchFromTo(string fromBranchName, string toBranchName, out string from, out string to)
        {
            from = null;
            to = null;
            foreach (var branch in _flexVersionConfiguration.Branches)
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

        private BranchConfiguration FindVersionSource(Results result, Dictionary<string, string> paramMatch_, List<Commit> activeCommits)
        {
            BranchConfiguration branchConfig;
            using (var repo = new Repository(_repoPath))
            {
                var branchName = repo.Branches.First(b => b.IsCurrentRepositoryHead).FriendlyName;
                result.BranchConfiguration = BranchConfig(branchName, _flexVersionConfiguration.Branches);
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
                                paramMatch_.Add(matchPart.Key, matchPart.Value);
                            }
                            versionSource = VersionSourceType.Tag;
                        }
                        else
                        {
                            if (versionSource != VersionSourceType.None || !commit.IsMerge) continue;

                            // Based on merge-message, what branch is the source?
                            var fromBranchName = string.Empty;
                            var toBranchName = string.Empty;
                            var fromBranchMatch = Regex.Match(c.Message, branchConfig.VersionSource.MergeMatch.FromToPattern);
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
                                    paramMatch_.Add(matchPart.Key, matchPart.Value);
                                }

                                versionSource = VersionSourceType.Merge;
                            }
                        }
                    }

                    activeCommits.Add(commit);

                    if (versionSource == VersionSourceType.None) continue;

                    // This commit is the version-source. We should add partial results break.
                    result.VersionSource = new VersionSource(commit, versionSource, paramMatch_);
                    break;
                }
                if (result.VersionSource == null) throw new ApplicationException("Could not find a version-source. No tags or merges qualified to be a version-source. If there are matches, then please adapt the config to allow it to match. If there are no merges that should be used as a version-source, then please add a tag to the git-repo, somewhere in the history.");
                result.GitInfo = new GitInfo(_repoPath, branchName, activeCommits);
            }

            return branchConfig;
        }

        private void GenerateOutputVariables(Results result, Dictionary<string, string> paramMatch, BranchConfiguration branchConfig)
        {
            // Handle output variables
            var gitInfo = new Dictionary<string, string>();
            var paramHead = new Dictionary<string, string>();
            var paramVS = new Dictionary<string, string>();

            gitInfo.Add("BranchName", result.GitInfo.BranchName);
            gitInfo.Add("ShortBranchName", result.GitInfo.ShortBranchName);
            gitInfo.Add("Path", result.GitInfo.Path);

            paramHead.Add("Author", result.GitInfo.LastAuthor);
            paramHead.Add("Date", result.GitInfo.LastCommitDate.ToString(CultureInfo.InvariantCulture));
            paramHead.Add("Sha", result.GitInfo.Head.Sha);
            paramHead.Add("Message", result.GitInfo.Head.Message);
            paramHead.Add("MessageShort", result.GitInfo.Head.MessageShort);

            paramVS.Add("CommitAuthor", result.VersionSource.Commit.Author);
            paramVS.Add("CommitDateTime", result.VersionSource.Commit.CommitDate.ToString(CultureInfo.InvariantCulture));
            paramVS.Add("CommitSha", result.VersionSource.Commit.Sha);
            paramVS.Add("CommitMessage", result.VersionSource.Commit.Message);
            paramVS.Add("CommitMessageShort", result.VersionSource.Commit.MessageShort);
            paramVS.Add("Message", result.VersionSource.Message);
            paramVS.Add("MessageShort", result.VersionSource.MessageShort);

            foreach (var output in branchConfig.Output)
            {
                var inputStream = new AntlrInputStream(output.Value);
                var lexer = new OutputLexer(inputStream);
                var commonTokenStream = new CommonTokenStream(lexer);
                var parser = new OutputParser(commonTokenStream);

                parser.RemoveErrorListeners();
                parser.AddErrorListener(new OutputErrorListener()); // add ours

                var visitor = new OutputVisitor(output.Key, output.Value, _paramArgs, gitInfo, paramHead, paramMatch, result.Output, paramVS);
                var parseOutput = visitor.Visit(parser.start());

                result.Output.Add(output.Key, parseOutput);
            }

            // Strip away temporary items, unless we are in diagnostic mode
            var res = new Dictionary<string, string>();
            foreach (var output in result.Output)
            {
                if (output.Key.StartsWith("_"))
                {
                    Logger.Debug($"Stripping temporary variable: {output.Key}='{output.Value}'");
                    continue;
                }
                res.Add(output.Key, output.Value);
            }

            result.Output = res;
        }

        private void ParseArguments(string[] arguments)
        {
            foreach (var argument in arguments)
            {
                var match = Regex.Match(argument, @"^(?<Variable>\w+)=(?<Value>\w+)$");
                if (match.Success)
                {
                    _paramArgs.Add(match.Groups["Variable"].Value, match.Groups["Value"].Value);
                }
            }
            Logger.Debug($"Argument variables detected: {string.Join(",", _paramArgs.Keys)}");
        }
    }
}