using Newtonsoft.Json;
using SourceCode.Clay.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SourceCode.Chasm.Serializer.Json.Wire
{
    internal static class TreeNodeMapExtensions
    {
        public static TreeNodeMap ReadTreeNodeMap(this JsonReader jr)
        {
            if (jr == null) throw new ArgumentNullException(nameof(jr));

            IReadOnlyList<TreeNode> list = jr.ReadArray(() => jr.ReadTreeNode());

            var tree = new TreeNodeMap(list);
            return tree;
        }

        public static TreeNodeMap ReadTreeNodeMap(this string json)
        {
            if (json == null || json == JsonConstants.JsonNull) return default;

            using (var tr = new StringReader(json))
            using (var jr = new JsonTextReader(tr))
            {
                jr.DateParseHandling = DateParseHandling.None;

                TreeNodeMap model = ReadTreeNodeMap(jr);
                return model;
            }
        }

        public static void Write(this JsonTextWriter jw, TreeNodeMap model)
        {
            if (jw == null) throw new ArgumentNullException(nameof(jw));

            if (model == default)
            {
                jw.WriteNull();
                return;
            }

            jw.WriteStartArray();
            {
                for (int i = 0; i < model.Count; i++)
                    jw.Write(model[i]);
            }
            jw.WriteEndArray();
        }

        public static string Write(this TreeNodeMap model)
        {
            if (model == default) return JsonConstants.JsonNull;

            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb))
            using (var jw = new JsonTextWriter(sw))
            {
                Write(jw, model);
            }

            return sb.ToString();
        }
    }
}
