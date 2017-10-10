#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using Google.Protobuf.WellKnownTypes;
using System;

namespace SourceCode.Chasm.IO.Proto.Wire
{
    internal static class CommitWireExtensions
    {
        #region Constants

        // Windows epoch 1601-01-01T00:00:00Z is 11,644,473,600 seconds before Unix epoch 1970-01-01T00:00:00Z
        private const long epochOffset = 11_644_473_600;

        #endregion

        #region Methods

        public static CommitWire Convert(this Commit model)
        {
            if (model == Commit.Empty)
                return new CommitWire() { Message = string.Empty };

            var wire = new CommitWire();

            // Parents
            switch (model.Parents.Count)
            {
                case 0:
                    break;

                case 1:
                    {
                        var sha1 = model.Parents[0].Sha1.Convert();
                        wire.Parents.Add(sha1);
                    }
                    break;

                default:
                    {
                        foreach (var parent in model.Parents)
                        {
                            var sha1 = parent.Sha1.Convert();
                            wire.Parents.Add(sha1);
                        }
                    }
                    break;
            }

            // Utc
            wire.Utc = new Timestamp
            {
                // Convert System.DateTime to Google.Protobuf.WellKnownTypes.Timestamp
                Seconds = (model.Utc.Ticks / TimeSpan.TicksPerSecond) - epochOffset,
                Nanos = (int)(model.Utc.Ticks % TimeSpan.TicksPerSecond) * 100 // Windows tick is 100 nanoseconds
            };

            // Message
            wire.Message = model.Message;

            // TreeId
            wire.TreeId = model.TreeId.Sha1.Convert();

            return wire;
        }

        public static Commit Convert(this CommitWire wire)
        {
            if (wire == null) return default;

            // TreeId
            var sha1 = wire.TreeId.Convert();
            var treeId = new TreeId(sha1);

            // Parents
            var parents = Array.Empty<CommitId>();
            if (wire.Parents != null && wire.Parents.Count > 0)
            {
                var i = 0;
                parents = new CommitId[wire.Parents.Count];
                foreach (var parent in wire.Parents)
                {
                    sha1 = parent.Convert();
                    parents[i++] = new CommitId(sha1);
                }
            }

            // Utc
            var utc = default(DateTime);
            if (wire.Utc != null)
            {
                // Convert Google.Protobuf.WellKnownTypes.Timestamp to System.DateTime
                var ticks = (wire.Utc.Seconds + epochOffset) * TimeSpan.TicksPerSecond;
                ticks += wire.Utc.Nanos / 100; // Windows tick is 100 nanoseconds
                utc = new DateTime(ticks, DateTimeKind.Utc);
            }

            var model = new Commit(parents, treeId, utc, wire.Message);
            return model;
        }

        #endregion
    }
}
