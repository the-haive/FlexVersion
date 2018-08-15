using System.Collections.Generic;

namespace IntelliSearch.FlexVersion.Configuration
{
    /// <summary>
    /// Contains the configuration on how results are to be generated.
    /// </summary>
    public class ResultsConfiguration
    {
        public string DateTimeFormat { get; set; }
        /// <summary>
        /// Used to clean the result output strings.
        /// </summary>
        public CleanOutputConfiguration CleanOutput { get; set; }

        /// <summary>
        /// A list of outputs to generate.
        /// </summary>
        public Dictionary<string, string> Output { get; set; }
    }
}