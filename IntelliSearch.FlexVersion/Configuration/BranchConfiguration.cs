using System.Collections.Generic;

namespace IntelliSearch.FlexVersion.Configuration
{
    /// <summary>
    /// BranchConfiguration holds information about a branch's configuration.
    /// </summary>
    public class BranchConfiguration
    {
        /// <summary>
        /// Decides what part of the version to bump - af any, for commits after the version-source.
        /// </summary>
        public Dictionary<string, string> OnCommit { get; set; }

        ///// <summary>
        ///// When a merge has been identified (see MergeSourceBranchPattern), then the actual major, minor and patch parts are extracted
        ///// from the message.
        /////
        ///// Various tools may use different patterns for the merge message. Sometimes also the author can decide to override the
        ///// merge-message themselves.
        /////
        ///// The recommended strategy is to identify how the automatic tools name the branches and make sure that they match. Also, train
        ///// developers to not override that part of the message, and instead add more info later in the message if needed.
        ///// In any case, the MergePattern should include the version parts you are lookign for.
        ///// Supported version parts are: major, minor, patch.
        ///// (?<major>\d+)\.(?>(?<minor>\d+)\.)?(?>(?<patch>\d+))?
        ///// </summary>
        //public string MergePattern { get; set; } //= @"(?<major>\d+)\.(?<minor>\d+)\.(?<patch>\d+)";
        /// <summary>
        /// Decides what part of the version to bump - af any, for merges after the version-source. OnMerge differentiates from OnCommit by
        /// having a dictionary of actions. The key in the dictionary is the name of the branch in the settings, the value is the BumpAction.
        ///
        /// This is to facilitate the possibility to have different version-bumping strategies depending on which branch is being merged in.
        /// </summary>
        public Dictionary<string, Dictionary<string, string>> OnMerge { get; set; }

        /// <summary>
        /// This regexPattern defines how the current branch is identified.
        /// </summary>
        public string Regex { get; set; }

        /// <summary>
        /// Contains the configuration templates for how output is to be generated. 
        /// </summary>
        public Dictionary<string, string> Output { get; set; }

        /// <summary>
        /// Defines how version-sources are to be detected and handled.
        /// </summary>
        public VersionSourceConfiguration VersionSource { get; set; }
    }
}