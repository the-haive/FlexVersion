namespace IntelliSearch.GitSemVer
{
    public class Tag
    {
        public Tag(LibGit2Sharp.Tag tag)
        {
            FriendlyName = tag.FriendlyName;
            IsAnnotated = tag.IsAnnotated;
            AnnotatedMessage = IsAnnotated ? tag.Annotation.Message : string.Empty;
            TargetSha = tag.Target.Sha;
        }

        public string AnnotatedMessage { get; internal set; }
        public string FriendlyName { get; internal set; }
        public bool IsAnnotated { get; internal set; }
        public string TargetSha { get; internal set; }
    }
}