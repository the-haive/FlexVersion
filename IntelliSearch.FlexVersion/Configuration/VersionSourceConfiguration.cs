using System.Collections.Generic;

namespace IntelliSearch.FlexVersion.Configuration
{
    /// <summary>
    /// Defines how version-sources are to be detected and handled.
    /// </summary>
    public class VersionSourceConfiguration
    {
        /// <summary>
        /// List of version-sources to use in prioritized order.
        /// </summary>
        public Dictionary<VersionSourceType, string> Order { get; set; }

        /// <summary>
        /// Whether or not to iterate all commits or only the first parent of a merge.
        ///
        /// In the FlexVersion context it is easier to control which tags that should be considered. This means that
        /// "micromanagement" of a version in a branch doesn't pollute into other branches when merged.
        ///
        /// The recommended choice is to only follow first parents, as wll commits in a branch are a part of the merge commit itself,
        /// thus these commits makes no difference.
        /// </summary>
        public bool? IterateFirstParentOnly { get; set; }

        /// <summary>
        /// In normal cases FlexVersion will find a tag or branch to use as the version-source fairly quickly. However, when that is not the
        /// case, it will continue to walk the commits backwards in time. The log is iterated quite fast, so normally it will be able to
        /// complete the full iteration in "sensible time". But, if it turns out that this is very slow, then you can stop iteration at a
        /// certain number of commits.
        ///
        /// The recoomended setting is to keep this at null or less than or equal to 0, which means that there are no limitations. Instead
        /// create a tag, somewhere to stop the iteration if it takes too long.
        /// </summary>
        public int? MaxCommitsToAnalyze { get; set; }

        /// <summary>
        /// Defines how to identify version-source merges.
        /// </summary>
        public MergeMatchConfiguration MergeMatch { get; set; }

        /// <summary>
        /// Defines how to identify version-source tags.
        /// </summary>
        public TagMatchConfiguration TagMatch { get; set; }
    }
}