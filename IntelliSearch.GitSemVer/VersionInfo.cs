namespace IntelliSearch.GitSemVer
{
    /// <summary>
    /// Contains properties and methods that aids in creating a meaningful version for you
    /// Populated by the instance owner.
    /// <todo>
    ///     - Add method to constrain to NuGet v2 limitations.
    /// </todo>
    /// </summary>
    public class VersionInfo
    {
        /// <summary>
        /// Major component of version.
        /// </summary>
        public int Major { get; internal set; }

        /// <summary>
        /// Minor component of version.
        /// </summary>
        public int Minor { get; internal set; }

        /// <summary>
        /// Patch component of version.
        /// </summary>
        public int Patch { get; internal set; }

        /// <summary>
        /// The PreReleaseLabel according to branch in use and it's branch-settings.
        /// </summary>
        public string PreReleaseLabel { get; internal set; }

        /// <summary>
        /// The PreReleaseNumber is set according to version-source and merge- and commit-increments.
        /// </summary>
        public int PreReleaseNumber { get; internal set; }

        /// <summary>
        /// Generates a release SemVer version-only string.
        /// </summary>
        public string ReleaseVersion => $@"{Major}.{Minor}.{Patch}";

        /// <summary>
        /// Generates a prerelease SemVer version with pre-release information string.
        /// </summary>
        /// <param name="metadataPart">Additional strings that are to be used as metadata.</param>
        /// <returns>A pre-release version string.</returns>
        public string PreReleaseVersion(params string[] metadataPart)
        {
            var metadata = metadataPart.Length > 0 ? $"+{string.Join(".", metadataPart)}" : string.Empty;
            return $@"{ReleaseVersion}-{PreReleaseLabel}.{PreReleaseNumber}{metadata}";
        }
    }
}