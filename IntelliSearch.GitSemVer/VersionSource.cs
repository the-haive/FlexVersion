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
        /// <param name="tagName">The tagName for the source, if tag.</param>
        /// <param name="tagMessage">The tagMessage for the source, if tag and annotated.</param>
        /// <param name="major">The major version part.</param>
        /// <param name="minor">The minor version part.</param>
        /// <param name="patch">The patch version part.</param>
        public VersionSource(Commit commit, VersionSourceType type, string tagName, string tagMessage, int major, int minor, int patch)
        {
            Commit = commit;
            Major = major;
            Minor = minor;
            Patch = patch;
            TagName = tagName;
            TagMessage = tagMessage;
            Type = type;
        }

        /// <summary>
        /// The commit for the versionSource, if versionsource is either tag or branch. If the source is the fallback then this is null.
        /// </summary>
        public Commit Commit { get; }

        /// <summary>
        /// The detected Major version from the message.
        /// </summary>
        public int Major { get; }

        /// <summary>
        /// The detected Minor version from the message.
        /// </summary>
        public int Minor { get; }

        /// <summary>
        /// The detected Patch version from the message.
        /// </summary>
        public int Patch { get; }

        /// <summary>
        /// The tag message, if the versionsource is a tag and if it is an annotated tag.
        /// </summary>
        public string TagMessage { get; set; }

        /// <summary>
        /// The tag name, if the versionsource is a tag.
        /// </summary>
        public string TagName { get; set; }

        /// <summary>
        /// Differentiates on what kind of version-source this is. Was it found due to a match from a branchname, or was it via a tag.
        /// </summary>
        public VersionSourceType Type { get; }
    }
}