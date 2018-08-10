using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace IntelliSearch.GitSemVer
{
    /// <summary>
    /// GitSemVer is a class that facilitates creating SemVer versions for your git repo, based on provided settings.
    /// </summary>
    public class GitSemVer
    {
        // ReSharper disable once InconsistentNaming
        //private const string RFC2822Format = "ddd dd MMM HH:mm:ss yyyy K";
        private readonly string _repoPath;
        private readonly Settings _settings;

        /// <summary>
        /// Initialize GitSemVer with repoPath and given settings-object.
        /// If repo-path is null current working directory is assumed.
        /// If settings is null a default Settings-object is created.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="repoPath"></param>
        public GitSemVer(Settings settings, string repoPath = null)
        {
            _repoPath = repoPath ?? Environment.CurrentDirectory;
            _settings = settings;
        }

        /// <summary>
        /// Initialize the repo with settings from a Yaml-file.
        /// If repo-path is null current working directory is assumed.
        /// If settings is null a default Settings-object is created.
        /// </summary>
        /// <param name="settingsAsYamlFilePath"></param>
        /// <param name="repoPath"></param>
        public GitSemVer(string settingsAsYamlFilePath, string repoPath = null)
        {
            _repoPath = repoPath ?? Environment.CurrentDirectory;

            if (!File.Exists(settingsAsYamlFilePath)) throw new ArgumentException($"The settings-file cannot be found at '{settingsAsYamlFilePath}'");

            var deserializer = new YamlDotNet.Serialization.Deserializer();
            _settings = deserializer.Deserialize<Settings>(File.OpenText(settingsAsYamlFilePath));
        }

        /// <summary>
        /// Analyzes the branch using the settings given to identify versioning information as well as providing some git repo information.
        /// </summary>
        /// <returns>A result object with various GitSemVer-relevant versioning information.</returns>
        public Result Analyze()
        {
            var result = new Result();

            using (var repo = new Repository(_repoPath))
            {
                // To know which settings to use, we need to know the branch
                var branchName = repo.Branches.First(b => b.IsCurrentRepositoryHead).FriendlyName;

                var branchSettings = _settings.For(branchName, out var settingsBranchName);

                var tags = new Dictionary<string, (string tagName, string annotationMessage, int major, int minor, int patch)>();
                foreach (var tag in repo.Tags)
                {
                    var match = Regex.Match(tag.FriendlyName, branchSettings.TagPattern);

                    if (match.Success)
                    {
                        tags.Add(tag.Target.Sha,
                            (
                                tag.FriendlyName,
                                tag.IsAnnotated ? tag.Annotation.Message : string.Empty,
                                int.Parse(match.Groups["major"].Value),
                                int.Parse(match.Groups["minor"].Value),
                                int.Parse(match.Groups["patch"].Value)
                            )
                        );
                    }
                }

                int major = 0;
                int minor = 0;
                int patch = 0;
                int pre = 0;

                var activeCommits = new List<Commit>();

                // Iterate commits until we find a version-source
                var commitFilter = new CommitFilter
                {
                    SortBy = CommitSortStrategies.Time,
                    // ReSharper disable once PossibleInvalidOperationException
                    FirstParentOnly = branchSettings.IterateFirstParentOnly.Value
                };

                var commits = branchSettings.MaxCommitsToAnalyze == null ||
                              branchSettings.MaxCommitsToAnalyze.Value <= 0
                    ? repo.Commits.QueryBy(commitFilter)
                    : repo.Commits.QueryBy(commitFilter).Take(branchSettings.MaxCommitsToAnalyze.Value);

                foreach (var c in commits)
                {
                    VersionSourceType versionSource = VersionSourceType.None;

                    if (tags.ContainsKey(c.Sha))
                    {
                        // This is the version-source
                        major = tags[c.Sha].major;
                        minor = tags[c.Sha].minor;
                        patch = tags[c.Sha].patch;
                        versionSource = VersionSourceType.Tag;
                    }

                    var isMerge = c.Parents.Count() > 1;
                    if (versionSource == VersionSourceType.None && isMerge)
                    {
                        // Based on merge-message, what branch is the source?
                        string fromBranchName = string.Empty;
                        var fromBranchMatch = Regex.Match(c.Message, branchSettings.MergeSourceBranchPattern);
                        if (fromBranchMatch.Success)
                        {
                            fromBranchName = fromBranchMatch.Groups["from"].Value;
                        }

                        if (string.IsNullOrWhiteSpace(fromBranchName))
                        {
                            // TODO: Warning that the merge was unidentifyable
                        }


                        // The mergemessage is required to have at least two version-numbers in order to be considered a version-source
                        // Ref: https://regex101.com/r/qTlaPw/1
                        var versionMatch = Regex.Match(fromBranchName,
                            @"(?<major>\d+)\.(?<minor>\d+)(?>\.(?<patch>\d+))?");
                        if (versionMatch.Success)
                        {
                            // This is the version-source
                            major = int.Parse(versionMatch.Groups["major"].Value);
                            minor = int.Parse(versionMatch.Groups["minor"].Value);
                            patch = int.Parse(versionMatch.Groups["patch"].Value);
                            versionSource = VersionSourceType.Tag;
                        }
                    }

                    var commit = new Commit
                    {
                        Sha = c.Sha,
                        Message = c.Message,
                        MessageShort = c.MessageShort,
                        Author = c.Author.ToString(),
                        CommitDate = c.Committer.When.DateTime,
                        IsMerge = isMerge
                    };

                    activeCommits.Add(commit);

                    if (versionSource != VersionSourceType.None)
                    {
                        // This commit is the version-source. We should add partial results break.
                        result.BranchSettings = new KeyValuePair<string, BranchSettings>(settingsBranchName, branchSettings);
                        result.VersionSource = new VersionSource(commit, versionSource, tags[c.Sha].tagName, tags[c.Sha].annotationMessage, major, minor, patch);
                        result.GitInfo = new GitInfo(branchName, activeCommits);
                        break;
                    }
                }

                // Iterate backwards to do bumping of the version, but dont do the last item, as that was the versionSource 
                // TODO: When fallback is supported, also include last item
                for (var i = activeCommits.Count() - 2; i >= 0; i--)
                {
                    var c = activeCommits[i];

                    if (c.IsMerge)
                    {
                        // Find relevant OnMerge
                        var mergeBumpAction = BumpAction.None;
                        foreach (var branch in _settings.Branches)
                        {
                            var match = Regex.Match(c.Message, branch.Value.Regex);
                            if (!match.Success) continue;

                            // This is the source-branch of the merge
                            if (branchSettings.OnMerge.ContainsKey(branch.Key))
                            {
                                mergeBumpAction = branchSettings.OnMerge.First(b => b.Key == branch.Key).Value;
                                break;
                            }

                            if (branchSettings.OnMerge.ContainsKey("*"))
                            {
                                mergeBumpAction = branchSettings.OnMerge.First(b => b.Key == "*").Value;
                            }
                        }
                        // Bump merge
                        BumpVersion(mergeBumpAction, ref major, ref minor, ref patch, ref pre);
                        continue;
                    }

                    // Bump Commit
                    BumpVersion(branchSettings.OnCommit ?? BumpAction.None, ref major, ref minor, ref patch, ref pre);
                }

                result.VersionInfo = new VersionInfo
                {
                    Major = major,
                    Minor = minor,
                    Patch = patch,
                    PreReleaseLabel = branchSettings.Label,
                    PreReleaseNumber = pre,
                };
            }

            return result;
        }

        private void BumpVersion(BumpAction onCommitValue, ref int major, ref int minor, ref int patch, ref int pre)
        {
            switch (onCommitValue)
            {
                case BumpAction.BumpMajor:
                    major++;
                    minor = 0;
                    patch = 0;
                    pre = 0;
                    break;
                case BumpAction.BumpMinor:
                    minor++;
                    patch = 0;
                    pre = 0;
                    break;
                case BumpAction.BumpPatch:
                    patch++;
                    pre = 0;
                    break;
                case BumpAction.BumpPre:
                    pre++;
                    break;
                case BumpAction.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(onCommitValue), onCommitValue, null);
            }

        }
    }
}