using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace IntelliSearch.GitSemVer
{
    /// <summary>
    /// Holds information on the type of version-source that is encountered.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum VersionSourceType
    {
        /// <summary>
        /// Should normally never happen. Is used internally before finding the actual type.
        /// </summary>
        None,

        /// <summary>
        /// The version-source is based on a merge-commit from another branch.
        /// </summary>
        Merge,

        /// <summary>
        /// The version-source is based on a tagged commit.
        /// </summary>
        Tag
    }
}