using Cake.Core;
using Cake.Core.Annotations;
using IntelliSearch.GitSemVer;

namespace Cake.GitSemVer
{
    public static class Addin
    {
        [CakeMethodAlias]
        public static Result GitSemVer(this ICakeContext context, Settings settings) => new IntelliSearch.GitSemVer.GitSemVer(settings).Analyze();
        public static Result GitSemVer(this ICakeContext context, string settings) => new IntelliSearch.GitSemVer.GitSemVer(settings).Analyze();
    }
}
