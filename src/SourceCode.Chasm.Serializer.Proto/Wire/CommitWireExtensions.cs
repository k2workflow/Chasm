using System;
using Google.Protobuf.WellKnownTypes;

namespace SourceCode.Chasm.Serializer.Proto.Wire
{
    internal static class CommitWireExtensions
    {
        public static CommitWire Convert(this Commit model)
        {
            var wire = new CommitWire
            {
                // TreeId
                TreeId = model.TreeId.Convert()
            };

            // Parents
            switch (model.Parents.Count)
            {
                case 0:
                    break;

                case 1:
                    {
                        Sha1Wire sha1 = model.Parents[0].Sha1.Convert();
                        wire.Parents.Add(sha1);
                    }
                    break;

                default:
                    {
                        foreach (CommitId parent in model.Parents)
                        {
                            Sha1Wire sha1 = parent.Sha1.Convert();
                            wire.Parents.Add(sha1);
                        }
                    }
                    break;
            }

            // Author
            wire.Author = model.Author.Convert();

            // Committer
            wire.Committer = model.Committer.Convert();

            // Message
            wire.Message = new StringValue { Value = model.Message };

            return wire;
        }

        public static Commit Convert(this CommitWire wire)
        {
            if (wire == null) return default;

            // TreeId
            TreeId? treeId = wire.TreeId.ConvertTree();

            // Parents
            CommitId[] parents = Array.Empty<CommitId>();
            if (wire.Parents != null && wire.Parents.Count > 0)
            {
                int i = 0;
                parents = new CommitId[wire.Parents.Count];
                foreach (Sha1Wire parent in wire.Parents)
                {
                    // Parents always have values as you simply
                    // don't include nulls in the list
                    parents[i++] = parent.ConvertCommit().Value;
                }
            }

            // Author
            Audit author = Audit.Empty;
            if (wire.Author != null)
            {
                author = wire.Author.Convert();
            }

            // Committer
            Audit committer = Audit.Empty;
            if (wire.Committer != null)
            {
                committer = wire.Committer.Convert();
            }

            // Message
            string message = wire.Message?.Value;

            return new Commit(parents, treeId, author, committer, message);
        }
    }
}
