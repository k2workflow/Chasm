using System;
using System.Diagnostics;
using Newtonsoft.Json;
using SourceCode.Clay;
using SourceCode.Clay.Json;

namespace SourceCode.Chasm.Serializer.Json.Wire
{
    internal static class TreeNodeExtensions
    {
        private const string Name = "name";
        private const string Kind = "kind";
        private const string NodeId = "nodeId";
        private const string Data = "data";

        public static TreeNode ReadTreeNode(this JsonReader jr)
        {
            Debug.Assert(jr != null);

            string name = default;
            NodeKind kind = default;
            Sha1 sha1 = default;
            ReadOnlyMemory<byte>? data = null;

            jr.ReadObject(n =>
            {
                switch (n)
                {
                    case Name:
                        name = (string)jr.Value;
                        return true;

                    case Kind:
                        kind = jr.AsEnum<NodeKind>(true);
                        return true;

                    case NodeId:
                        sha1 = jr.ReadSha1();
                        return true;

                    case Data:
                        var base64 = (string)jr.Value;
                        if (base64 != null)
                            data = Convert.FromBase64String(base64);
                        return true;
                }

                return false;
            });

            return (name == null && kind == default && sha1 == default & data == null) ? default : new TreeNode(name, kind, sha1, data);
        }

        public static void Write(this JsonWriter jw, TreeNode model)
        {
            Debug.Assert(jw != null);

            if (model == default)
            {
                jw.WriteNull();
                return;
            }

            jw.WriteStartObject();
            {
                // Name
                jw.WritePropertyName(Name);
                jw.WriteValue(model.Name);

                // Kind
                jw.WritePropertyName(Kind);
                jw.WriteValue(model.Kind.ToString());

                // NodeId
                jw.WritePropertyName(NodeId);
                jw.WriteValue(model.Sha1.ToString("n"));

                // Data
                if (model.Data != null)
                {
                    jw.WritePropertyName(Data);
                    var base64 = Convert.ToBase64String(model.Data.Value.Span, Base64FormattingOptions.None);
                    jw.WriteValue(base64);
                }
            }
            jw.WriteEndObject();
        }
    }
}
