using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace IntelliSearch.GitSemVer
{
    /// <summary>
    /// Holds generic, but versioning-relevant, information on the git-repo.
    /// </summary>
    public class GitInfo
    {
        /// <summary>
        /// Creates a new GitInfo instance based on the provided params.
        /// </summary>
        /// <param name="repoPath"></param>
        /// <param name="branch"></param>
        /// <param name="logToVersionSource"></param>
        public GitInfo(string repoPath, string branch, List<Commit> logToVersionSource)
        {
            Path = repoPath;
            Branch = branch;
            LogToVersionSource = logToVersionSource;
        }

        /// <summary>
        /// The path to the git-repo that was analyzed.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Author of last commit
        /// </summary>
        public string LastAuthor => Head.Author;

        /// <summary>
        /// Branchname
        /// </summary>
        public string Branch { get; }

        /// <summary>
        /// Commit date of last commit
        /// </summary>
        public DateTime LastCommitDate => Head.CommitDate;

        /// <summary>
        /// The last commit
        /// </summary>
        [JsonIgnore]
        public Commit Head => LogToVersionSource.FirstOrDefault();

        /// <summary>
        /// All commits upto and including the VersionSourceCommit
        /// </summary>
        public List<Commit> LogToVersionSource { get; }

    }
}