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
        /// <param name="branch"></param>
        /// <param name="logToVersionSource"></param>
        public GitInfo(string branch, List<Commit> logToVersionSource)
        {
            Branch = branch;
            LogToVersionSource = logToVersionSource;
        }

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