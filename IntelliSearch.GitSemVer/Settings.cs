using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace IntelliSearch.GitSemVer
{
    /// <summary>
    /// Holds the full settings configuration that is to be used for generating the versioning information for your repo.
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// A dictionary of all branch-settings.
        ///
        /// It should contain at least a default named "*", that is used as the basis for all branches.
        /// Each named branch inherits the defaults, but overwrites the defaults when specified.
        /// </summary>
        public Dictionary<string, BranchSettings> Branches { get; set; } = new Dictionary<string, BranchSettings>();

        /// <summary>
        /// Gets the branch-settings for the given branchName.
        /// </summary>
        /// <param name="branchName">The branchName as given from git.</param>
        /// <param name="settingsBranchName">Sets this to the branchname key in the configuration.</param>
        /// <returns>The active branch-settings to be used.</returns>
        public BranchSettings For(string branchName, out string settingsBranchName)
        {
            if (!Branches.Any())
            {
                throw new ArgumentException("There are no branch-settings defined. Branch-settings (including one for '*' is required.");
            }

            if (!Branches.ContainsKey("*"))
            {
                throw new ArgumentException("No default branch-setting is defined. Add a branch named '*'.");
            }

            // Iterate all branches, except the default.
            var defaultBranch = Branches.First(i => i.Key == "*");

            KeyValuePair<string, BranchSettings> currentBranchEntry;

            foreach (var keyValuePair in Branches.Where(i => i.Key != "*"))
            {
                var match = Regex.Match(branchName, keyValuePair.Value.Regex, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    // This is the branch we want.
                    currentBranchEntry = keyValuePair;
                    break;
                }
            }

            settingsBranchName = currentBranchEntry.Value == null
                ? defaultBranch.Key
                : currentBranchEntry.Key;

            var currentBranchSettings = currentBranchEntry.Value == null
                ? defaultBranch.Value
                : BranchSettings.Merge(defaultBranch.Value, currentBranchEntry.Value);

            VerifyRequired(branchName, currentBranchSettings);

            return currentBranchSettings;
        }

        private void VerifyRequired(string branchName, BranchSettings branchSettings)
        {
            if (branchSettings == null)
            {
                throw new ArgumentNullException(nameof(branchSettings));
            }

            if (branchSettings.IterateFirstParentOnly == null)
            {
                throw new ArgumentException($"Required branch-setting IterateFirstParent for '{branchName}' is not defined.");
            }

            if (string.IsNullOrWhiteSpace(branchSettings.TagPattern))
            {
                throw new ArgumentException($"Required branch-setting TagPattern for '{branchName}' is not defined.");
            }
        }
    }
}