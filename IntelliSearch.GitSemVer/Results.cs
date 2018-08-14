using System.Collections.Generic;
using IntelliSearch.GitSemVer.Configuration;

namespace IntelliSearch.GitSemVer
{
    /// <summary>
    /// Contains the results of the GitSemVer calculations.
    /// </summary>
    public class Results
    {
        /// <summary>
        /// The configuration values in use for the detected branch
        /// </summary>
        public KeyValuePair<string, BranchConfiguration> BranchConfiguration { get; internal set; }

        /// <summary>
        /// Information on the detected version-source.
        /// </summary>
        public VersionSource VersionSource { get; internal set; }

        /// <summary>
        /// Essential information about the git repo
        /// </summary>
        public GitInfo GitInfo { get; internal set; }

        /// <summary>
        /// Versioning information
        /// </summary>
        public Dictionary<string, string> Output { get; internal set; } = new Dictionary<string, string>();
    }

}