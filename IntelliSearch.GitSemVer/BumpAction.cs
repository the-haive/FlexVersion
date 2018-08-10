using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace IntelliSearch.GitSemVer
{
    /// <summary>
    /// Decides which part of the version that should increment, if any.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum BumpAction
    {
        None,
        BumpMajor,
        BumpMinor,
        BumpPatch,
        BumpPre
        // Considering this for the future:
        //UseBranchMajorMinorPatchLabel,
        //UseBranchMajorMinorPatch,
        //UseBranchMajorMinor,
        //UseBranchMajor,
        //UseBranchMinorPatchLabel,
        //UseBranchMinorPatch,
        //UseBranchMinor,
        //UseBranchPatchLabel,
        //UseBranchPatch,
        //UseBranchLabel,
    }
}