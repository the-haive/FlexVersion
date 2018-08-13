using System.Collections.Generic;

namespace IntelliSearch.GitSemVer
{
    /// <summary>
    /// Contains the results of the GitSemVer calculations.
    /// </summary>
    public class ResultOld
    {
        /// <summary>
        /// The configuration values in use for the detected branch
        /// </summary>
        public KeyValuePair<string, BranchSettings> BranchSettings { get; internal set; }

        /// <summary>
        /// Information on the detected version-source.
        /// </summary>
        public VersionSource VersionSource { get; set; }

        /// <summary>
        /// Essential information about the git repo
        /// </summary>
        public GitInfo GitInfo { get; internal set; }

        /// <summary>
        /// Versioning information
        /// </summary>
        public VersionInfo VersionInfo { get; internal set; }
    }
}