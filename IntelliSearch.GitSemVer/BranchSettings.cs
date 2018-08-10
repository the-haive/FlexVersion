using System.Collections.Generic;

namespace IntelliSearch.GitSemVer
{
    /// <summary>
    /// BranchSettings holds information about a branch's configuration.
    /// </summary>
    public class BranchSettings
    {
        /// <summary>
        /// Whether or not only annontated tags should be checked, or if also so called lightweight tags are to be examined.
        ///
        /// For organisations where tags (or at least versioning tags) must be annotated then this should be true.
        /// For organisations where the type of tag is not important then this should be false.
        /// </summary>
        public bool? AnnotatedTagsOnly { get; set; } //= false;

        /// <summary>
        /// Whether or not to iterate all commits or only the first parent of a merge.
        ///
        /// In the GitSemVer context it is easier to control which tags that should be considered. This means that
        /// "micromanagement" of a version in a branch doesn't pollute into other branches when merged.
        ///
        /// The recommended choice is to only follow first parents, as wll commits in a branch are a part of the merge commit itself,
        /// thus these commits makes no difference.
        /// </summary>
        public bool? IterateFirstParentOnly { get; set; } //= true

        /// <summary>
        /// This is the label that is to be used as the PreReleaseLabel for the currrent branch.
        ///
        /// In order to specify versions as older or newer in a SemVer pre-release context, both the PreReleaseLabel and the
        /// PreReleaseNumber will decide which package is newer. But, when creating packages from multiple branches it is hard to
        /// create packages that are "sorted" correctly. With GitSemVer you can use Labels to control this. This means that you can
        /// also make sure that pre-release versions from specific branches will not be "newer" than others for the same
        /// Major.Minor.Patch version.
        ///
        /// GitFlow example follows:
        /// develop:
        ///   Label: alpha
        /// release:
        ///   Label: beta
        /// feature:
        ///   Label: 0dev
        ///
        /// By setting the label to 0dev the version will never be considered "newer" than develop or release. We could have used "aaa",
        /// which "predates" both alpha and beta. This method suggests using the starting 0 to make sure that it predates the other labels.
        /// Given that the other labels are not given 0 plus something that predates alpha as a label, that is. SemVer specifically allows
        /// 0-9, a-z and A-Z *only*. Since it is very unlikely that any Label would use a number, by prefixing with 0 it will
        /// predate any other pre-release. At the same time it will not affect the final release version (which does not include the
        /// pre-release part).
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// In normal cases GitSemVer will find a tag or branch to use as the version-source fairly quickly. However, when that is not the
        /// case, it will continue to walk the commits backwards in time. The log is iterated quite fast, so normally it will be able to
        /// complete the full iteration in "sensible time". But, if it turns out that this is very slow, then you can stop iteration at a
        /// certain number of commits.
        ///
        /// The recoomended setting is to keep this at null or less than or equal to 0, which means that there are no limitations. Instead
        /// create a tag, somewhere to stop the iteration if it takes too long.
        /// </summary>
        public int? MaxCommitsToAnalyze { get; set; } //= 0;

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
        /// This is the pattern that is checked for while iterating the commits.
        ///
        /// Various tools may have different patterns here. The important part is that the regex needs to create group named 'from', so that
        /// GitSemVer can understand the name of the branch being merged in.
        /// 
        /// Which branch is being merged in decide how OnMerge is handled, as there can be different strategies depending on which branch is
        /// merged in.
        /// Sample: https://regex101.com/r/OpDMdv/2/
        /// </summary>
        public string MergeSourceBranchPattern { get; set; } //= @"Merged?\s+(?>(?>remote-tracking)|(?>branch)\s+)?(?<from>.+)\s+into\s+.*";

        /// <summary>
        /// Decides what part of the version to bump - af any, for commits after the version-source.
        /// </summary>
        public BumpAction? OnCommit { get; set; }

        /// <summary>
        /// Decides what part of the version to bump - af any, for merges after the version-source. OnMerge differentiates from OnCommit by
        /// having a dictionary of actions. The key in the dictionary is the name of the branch in the settings, the value is the BumpAction.
        ///
        /// This is to facilitate the possibility to have different version-bumping strategies depending on which branch is being merged in.
        /// </summary>
        public Dictionary<string, BumpAction> OnMerge { get; set; }

        /// <summary>
        /// This regexPattern defines how the current branch is identified.
        /// </summary>
        public string Regex { get; set; }

        /// <summary>
        /// When a tag has been identified as the version-source then this pattern is used to extract the major, minor and patch parts.
        ///
        /// Choose a pattern that you agree on internally in the organization. It should contain named groups that identify the wanted
        /// parts. Supported version parts are: major, minor, patch.
        /// </summary>
        public string TagPattern { get; set; } //= @"^v?(?<major>\d+)\.(?<minor>\d+)\.(?<patch>\d+).*$";

        /// <summary>
        /// Merges two branch-settings to produce the sum of the two.
        /// </summary>
        /// <param name="defaultSettings"></param>
        /// <param name="overrideSettings"></param>
        /// <returns></returns>
        public static BranchSettings Merge(BranchSettings defaultSettings, BranchSettings overrideSettings)
        {
            return new BranchSettings
            {
                AnnotatedTagsOnly = overrideSettings.AnnotatedTagsOnly ?? defaultSettings.AnnotatedTagsOnly,
                IterateFirstParentOnly = overrideSettings.IterateFirstParentOnly ?? defaultSettings.IterateFirstParentOnly,
                Label = overrideSettings.Label ?? defaultSettings.Label,
                MaxCommitsToAnalyze = overrideSettings.MaxCommitsToAnalyze ?? defaultSettings.MaxCommitsToAnalyze,
                MergeSourceBranchPattern = overrideSettings.MergeSourceBranchPattern ?? defaultSettings.MergeSourceBranchPattern,
                OnCommit = overrideSettings.OnCommit ?? defaultSettings.OnCommit,
                OnMerge = overrideSettings.OnMerge ?? defaultSettings.OnMerge,
                Regex = overrideSettings.Regex ?? defaultSettings.Regex,
                TagPattern = overrideSettings.TagPattern ?? defaultSettings.TagPattern,
            };
        }
    }
}