using System.Collections.Generic;

namespace IntelliSearch.GitSemVer.Configuration
{
    /// <summary>
    /// Defines how to identify version-source merges.
    /// </summary>
    public class MergeMatchConfiguration
    {
        /// <summary>
        /// A dictionary of branch-names, where the key and values both represent defined branches.
        /// </summary>
        public Dictionary<string, string> FromTo { get; set; }

        /// <summary>
        /// The pattern that identifies merges as being version-sources.
        /// </summary>
        public string FromToPattern { get; set; }

        /// <summary>
        /// The pattern that is used to detect content in the merge-message.
        /// </summary>
        public string MatchPattern { get; set; }
    }
}