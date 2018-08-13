using Cake.Core;
using Cake.Core.Annotations;
using IntelliSearch.GitSemVer;

namespace Cake.GitSemVer
{
    public static class Addin
    {
        [CakeMethodAlias]
        public static ResultOld GitSemVer(this ICakeContext context, Settings settings) => new IntelliSearch.GitSemVer.GitSemVerOld(settings).Analyze();
        public static ResultOld GitSemVer(this ICakeContext context, string settings) => new IntelliSearch.GitSemVer.GitSemVerOld(settings).Analyze();
    }
}
