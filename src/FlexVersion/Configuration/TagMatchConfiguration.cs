namespace IntelliSearch.FlexVersion.Configuration
{
    public class TagMatchConfiguration
    {
        /// <summary>
        /// Whether or not only annontated tags should be checked, or if also so called lightweight tags are to be examined.
        ///
        /// For organisations where tags (or at least versioning tags) must be annotated then this should be true.
        /// For organisations where the type of tag is not important then this should be false.
        /// </summary>
        public bool? AnnotatedTagsOnly { get; set; } //= false;

        /// <summary>
        /// The pattern that is used to detect content in the tag-name.
        /// </summary>
        public string MatchPattern { get; set; }
    }
}