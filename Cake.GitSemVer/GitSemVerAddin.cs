using Cake.Core;
using Cake.Core.Annotations;
using IntelliSearch.GitSemVer;
using IntelliSearch.GitSemVer.Configuration;

namespace Cake.GitSemVer
{
    public static class Addin
    {
        [CakeMethodAlias]
        public static Results GitSemVer(
            this ICakeContext context,
            GitSemVerConfiguration configuration
        )
        {
            return new IntelliSearch.GitSemVer.GitSemVer(
                configuration
            ).Analyze();
        }

        [CakeMethodAlias]
        public static Results GitSemVer(
            this ICakeContext context,
            GitSemVerConfiguration configuration,
            string repoPath
        )
        {
            return new IntelliSearch.GitSemVer.GitSemVer(
                    configuration,
                    repoPath
                )
                .Analyze();
        }

        [CakeMethodAlias]
        public static Results GitSemVer(
            this ICakeContext context,
            string configFilePath
        )
        {
            return new IntelliSearch.GitSemVer.GitSemVer(
                configFilePath
            ).Analyze();
        }

        [CakeMethodAlias]
        public static Results GitSemVer(
            this ICakeContext context,
            string configFilePath,
            string repoPath
        )
        {
            return new IntelliSearch.GitSemVer.GitSemVer(
                configFilePath,
                repoPath
            ).Analyze();
        }
    }
}