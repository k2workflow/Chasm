using SourceCode.Clay;

namespace SourceCode.Chasm.Serializer.Text.Wire
{
    internal static class CommitIdExtensions
    {
        public static string Convert(this CommitId model)
            => model.Sha1.ToString("n");

        public static CommitId ConvertCommitId(this string wire)
        {
            if (string.IsNullOrWhiteSpace(wire)) return default;

            var sha1 = Sha1.Parse(wire);

            var model = new CommitId(sha1);
            return model;
        }
    }
}
