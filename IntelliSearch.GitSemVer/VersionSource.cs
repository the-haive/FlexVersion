using System.Collections.Generic;

namespace IntelliSearch.GitSemVer
{
    /// <summary>
    /// This class holds relevant information in regards to the actual version-source found.
    /// </summary>
    public class VersionSource
    {
        /// <summary>
        ///  Creates an instance of the version-source.
        /// </summary>
        /// <param name="commit">The comit for the version-source.</param>
        /// <param name="type">The type of version-source, i.e. tag, branch (Future: fallback from config)</param>
        /// <param name="matchParts">The detected match-grops in the source tag or source merge</param>
        public VersionSource(Commit commit, VersionSourceType type, Dictionary<string, string> matchParts)
        {
            Commit = commit;
            Type = type;
            MatchParts = matchParts;
        }

        public Dictionary<string, string> MatchParts { get;}

        /// <summary>
        /// The commit for the versionSource, if versionsource is either tag or branch. If the source is the fallback then this is null.
        /// </summary>
        public Commit Commit { get; }

        /// <summary>
        /// Differentiates on what kind of version-source this is. Was it found due to a match from a branchname, or was it via a tag.
        /// </summary>
        public VersionSourceType Type { get; }

        public string Message => Commit.Tag != null ? (Commit.Tag.IsAnnotated ? Commit.Tag.AnnotatedMessage : string.Empty) : Commit.Message;

        public string MessageShort => Commit.Tag != null ? Commit.Tag.FriendlyName : Commit.Message;
    }
}