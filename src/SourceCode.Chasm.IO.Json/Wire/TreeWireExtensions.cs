using SourceCode.Clay.Json;
using System.Json;

namespace SourceCode.Chasm.IO.Json.Wire
{
    internal static class TreeWireExtensions
    {
        public static JsonValue Convert(this TreeNodeList model)
        {
            if (model.Count == 0) return default;

            var items = new JsonValue[model.Count];
            for (var i = 0; i < items.Length; i++)
                items[i] = model[i].Convert();

            var wire = new JsonArray(items);
            return wire;
        }

        public static TreeNodeList ConvertTree(this JsonArray wire)
        {
            if (wire == null || wire.Count == 0) return default;

            var nodes = new TreeNode[wire.Count];
            for (var i = 0; i < nodes.Length; i++)
                nodes[i] = ((JsonObject)wire[i]).ConvertTreeNode();

            var model = new TreeNodeList(nodes);
            return model;
        }

        public static TreeNodeList ParseTree(this string json)
        {
            var wire = json.ParseJsonArray();

            var model = wire.ConvertTree();
            return model;
        }
    }
}
