using Cake.Core;
using Cake.Core.Annotations;
using IntelliSearch.FlexVersion;
using IntelliSearch.FlexVersion.Configuration;

namespace Cake.FlexVersion
{
    public static class Addin
    {
        [CakeMethodAlias]
        public static Results FlexVersion(
            this ICakeContext context,
            FlexVersionConfiguration configuration
        )
        {
            return new IntelliSearch.FlexVersion.FlexVersion(
                configuration
            ).Analyze();
        }

        [CakeMethodAlias]
        public static Results FlexVersion(
            this ICakeContext context,
            FlexVersionConfiguration configuration,
            string repoPath
        )
        {
            return new IntelliSearch.FlexVersion.FlexVersion(
                    configuration,
                    repoPath
                )
                .Analyze();
        }

        [CakeMethodAlias]
        public static Results FlexVersion(
            this ICakeContext context,
            string configFilePath
        )
        {
            return new IntelliSearch.FlexVersion.FlexVersion(
                configFilePath
            ).Analyze();
        }

        [CakeMethodAlias]
        public static Results FlexVersion(
            this ICakeContext context,
            string configFilePath,
            string repoPath
        )
        {
            return new IntelliSearch.FlexVersion.FlexVersion(
                configFilePath,
                repoPath
            ).Analyze();
        }
    }
}