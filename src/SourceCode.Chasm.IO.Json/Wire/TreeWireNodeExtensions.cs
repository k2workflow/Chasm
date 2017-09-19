using SourceCode.Clay.Json;
using System;
using System.Json;

namespace SourceCode.Chasm.IO.Json.Wire
{
    internal static class TreeWireNodeExtensions
    {
        private const string _name = "name";
        private const string _kind = "kind";
        private const string _sha1 = "sha1";

        public static JsonObject Convert(this TreeNode model)
        {
            if (model == TreeNode.Empty) return default;

            var wire = new JsonObject
            {
                [_name] = model.Name,
                [_kind] = model.Kind.ToString(),
                [_sha1] = model.Sha1.ToString("N")
            };

            return wire;
        }

        public static TreeNode ConvertTreeNode(this JsonObject wire)
        {
            if (wire == null) return default;

            var name = (string)wire.GetValue(_name, JsonType.String, false);

            var jv = wire.GetValue(_kind, JsonType.String, false);
            var kind = (NodeKind)Enum.Parse(typeof(NodeKind), jv, true);

            jv = wire.GetValue(_sha1, JsonType.String, false);
            var sha1 = Sha1.Parse(jv);

            var model = new TreeNode(name, kind, sha1);
            return model;
        }

        public static TreeNode ParseTreeNode(this string json)
        {
            var wire = json.ParseJsonObject();

            var model = wire.ConvertTreeNode();
            return model;
        }
    }
}
