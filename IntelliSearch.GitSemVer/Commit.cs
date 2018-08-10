using System;

namespace IntelliSearch.GitSemVer
{
    /// <summary>
    /// A simplified representation of commits, including only parts that seems relevant versioning purposes.
    /// </summary>
    public class Commit
    {
        /// <summary>
        /// The author of the commit.
        /// </summary>
        public string Author { get; internal set; }

        /// <summary>
        /// The datetime for the commit.
        /// </summary>
        public DateTime CommitDate { get; internal set; }

        /// <summary>
        /// Whether or not the commit is a merge or not.
        /// </summary>
        public bool IsMerge { get; internal set; }

        /// <summary>
        /// The full commitMessage
        /// </summary>
        public string Message { get; internal set; }

        /// <summary>
        /// The first line of the git message (susually used as a title/subject).
        /// </summary>
        public string MessageShort { get; internal set; }

        /// <summary>
        /// The sha is a unique commit identifier.
        /// </summary>
        public string Sha { get; internal set; }
    }
}