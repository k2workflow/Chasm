using Google.Protobuf.WellKnownTypes;
using System.Collections.Generic;

namespace SourceCode.Chasm.IO.Proto.Wire
{
    internal static class CommitWireExtensions
    {
        // Windows epoch 1601-01-01T00:00:00Z is 11,644,473,600 seconds before Unix epoch 1970-01-01T00:00:00Z
        private const long epochOffset = 11_644_473_600;

        public static CommitWire Convert(this Commit model)
        {
            if (model == Commit.Empty) return null;

            // TreeId
            var treeId = model.TreeId.Sha1.Convert();

            // CommitUtc
            // Convert System.DateTime to Google.Protobuf.WellKnownTypes.Timestamp
            var ts = new Timestamp
            {
                Seconds = (model.CommitUtc.Ticks / System.TimeSpan.TicksPerSecond) - epochOffset,
                Nanos = (int)(model.CommitUtc.Ticks % System.TimeSpan.TicksPerSecond) * 100 // Windows tick is 100 nanoseconds
            };

            var wire = new CommitWire
            {
                TreeId = treeId,
                CommitUtc = ts,
                CommitMessage = model.CommitMessage
            };

            // Parents
            if (model.Parents != null)
            {
                foreach (var parent in model.Parents)
                {
                    var sha1 = new Sha1Wire
                    {
                        Blit0 = parent.Sha1.Blit0,
                        Blit1 = parent.Sha1.Blit1,
                        Blit2 = parent.Sha1.Blit2
                    };

                    wire.Parents.Add(sha1);
                }
            }

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
            // Convert Google.Protobuf.WellKnownTypes.Timestamp to System.DateTime
            var ticks = (wire.CommitUtc.Seconds + epochOffset) * System.TimeSpan.TicksPerSecond;
            ticks += wire.CommitUtc.Nanos / 100; // Windows tick is 100 nanoseconds
            var utc = new System.DateTime(ticks, System.DateTimeKind.Utc);

            var model = new Commit(parents, treeId, utc, wire.CommitMessage);
            return model;
        }
    }
}
