#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using Google.Protobuf;
using SourceCode.Chasm.IO.Proto.Wire;
using SourceCode.Clay.Buffers;
using System;

namespace SourceCode.Chasm.IO.Proto
{
    partial class ProtoChasmSerializer // .CommitRef
    {
        #region Serialize

        public BufferSession Serialize(CommitRef model)
        {
            var wire = model.Convert();

            var size = wire.CalculateSize();
            var buffer = BufferSession.RentBuffer(size);

            using (var cos = new CodedOutputStream(buffer))
            {
                wire.WriteTo(cos);

                var segment = new ArraySegment<byte>(buffer, 0, (int)cos.Position);

                var session = new BufferSession(buffer, segment);
                return session;
            }
        }

        #endregion

        #region Deserialize

        public CommitRef DeserializeCommitRef(string name, ReadOnlySpan<byte> span)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            if (span.IsEmpty) return new CommitRef(name, CommitId.Empty);

            var wire = new CommitRefWire();
            wire.MergeFrom(span.ToArray()); // TODO: Perf

            var model = wire.Convert(name);
            return model;
        }

        #endregion
    }
}
