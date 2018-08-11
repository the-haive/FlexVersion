using System.Collections.Generic;

namespace IntelliSearch.GitSemVer.Configuration
{
    /// <summary>
    /// Holds the full configuration that is to be used for generating the versioning information for your repo.
    /// </summary>
    public class GitSemVerConfiguration
    {
        /// <summary>
        /// A dictionary of all branch-settings.
        ///
        /// It should contain at least a default named "*", that is used as the basis for all branches.
        /// Each named branch inherits the defaults, but overwrites the defaults when specified.
        /// </summary>
        public Dictionary<string, BranchConfiguration> Branches { get; set; } = new Dictionary<string, BranchConfiguration>();
    }
}