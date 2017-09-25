using System;
using System.Collections.Generic;

namespace SourceCode.Chasm.IO.Bond.Wire
{
    internal static class CommitWireExtensions
    {
        public static int EstimateBytes(this CommitWire wire)
        {
            if (wire == null) return 0;

            // TreeId
            int len = Sha1.ByteLen;

            // Parents
            var count = wire.Parents?.Count ?? 0;
            if (count > 0)
                len += Sha1.ByteLen * count;

            // CommitUtc (ticks)
            len += sizeof(long);

            // CommitMessage
            len += (wire.CommitMessage?.Length ?? 0) * 3; // Utf8 is 1-3 bpc

            return len;
        }

        public static CommitWire Convert(this Commit model)
        {
            if (model == Commit.Empty) return null;

            // TreeId
            var treeId = model.TreeId.Sha1.Convert();

            // Parents
            var parents = new List<Sha1Wire>(model.Parents?.Count ?? 0);
            if (model.Parents != null)
                foreach (var parent in model.Parents)
                    parents.Add(parent.Sha1.Convert());

            var wire = new CommitWire
            {
                Parents = parents,
                TreeId = treeId,
                CommitUtc = model.CommitUtc,
                CommitMessage = model.CommitMessage
            };

            return wire;
        }

        public static Commit Convert(this CommitWire wire)
        {
            if (wire == null) return Commit.Empty;

            // TreeId
            var sha1 = wire.TreeId.Convert();
            var treeId = new TreeId(sha1);

            // Parents
            var parents = new List<CommitId>(wire.Parents?.Count ?? 0);
            if (wire.Parents != null)
            {
                foreach (var parent in wire.Parents)
                {
                    sha1 = parent.Convert();
                    parents.Add(new CommitId(sha1));
                }
            }

            // CommitUtc
            var commitUtc = new DateTime(wire.CommitUtc.Ticks, DateTimeKind.Utc);

            var model = new Commit(parents, treeId, commitUtc, wire.CommitMessage);
            return model;
        }
    }
}
