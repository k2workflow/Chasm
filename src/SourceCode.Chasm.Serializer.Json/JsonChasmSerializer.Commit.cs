using System;
using System.Buffers;
using System.Text;
using SourceCode.Chasm.Serializer.Json.Wire;

namespace SourceCode.Chasm.Serializer.Json
{
    partial class JsonChasmSerializer // .Commit
    {
        public override Memory<byte> Serialize(Commit model)
        {
            string json = model.Write();

            int length = Encoding.UTF8.GetMaxByteCount(json.Length); // Utf8 is 1-4 bpc

            Memory<byte> rented = Rent(length);
            length = Encoding.UTF8.GetBytes(json, rented.Span);

            Memory<byte> slice = rented.Slice(0, length);
            return slice;
        }

        public override Commit DeserializeCommit(ReadOnlySpan<byte> span)
        {
            if (span.Length == 0) throw new ArgumentNullException(nameof(span));

            string json = Encoding.UTF8.GetString(span);

            Commit model = json.ReadCommit();
            return model;
        }
    }
}
