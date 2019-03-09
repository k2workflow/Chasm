using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using SourceCode.Clay.Json;

namespace SourceCode.Chasm.Serializer.Json.Wire
{
    internal static class TreeNodeMapExtensions
    {
        public static TreeNodeMap ReadTreeNodeMap(this JsonReader jr)
        {
            Debug.Assert(jr != null);

            IReadOnlyList<TreeNode> list = jr.ReadArray(() => jr.ReadTreeNode());

            return new TreeNodeMap(list);
        }

        public static TreeNodeMap ReadTreeNodeMap(this string json)
        {
            if (json == null || json == JsonConstants.JsonNull) return default;

            using (var tr = new StringReader(json))
            using (var jr = new JsonTextReader(tr))
            {
                jr.DateParseHandling = DateParseHandling.None;

                return ReadTreeNodeMap(jr);
            }
        }

        public static void Write(this JsonTextWriter jw, TreeNodeMap model)
        {
            Debug.Assert(jw != null);

            if (model.Count == 0)
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
