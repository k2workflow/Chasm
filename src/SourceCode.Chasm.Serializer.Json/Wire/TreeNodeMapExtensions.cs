#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using Newtonsoft.Json;
using SourceCode.Clay.Json;
using System;
using System.IO;
using System.Text;

namespace SourceCode.Chasm.IO.Json.Wire
{
    internal static class TreeNodeMapExtensions
    {
        #region Read

        public static TreeNodeMap ReadTreeNodeMap(this JsonReader jr)
        {
            if (jr == null) throw new ArgumentNullException(nameof(jr));

            var list = jr.ReadArray(() => jr.ReadTreeNode());

            var tree = new TreeNodeMap(list);
            return tree;
        }

        public static TreeNodeMap ReadTreeNodeMap(this string json)
        {
            if (json == null || json == JsonConstants.Null) return default;

            using (var tr = new StringReader(json))
            using (var jr = new JsonTextReader(tr))
            {
                jr.DateParseHandling = DateParseHandling.None;

                var model = ReadTreeNodeMap(jr);
                return model;
            }
        }

        #endregion

        #region Write

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
                for (var i = 0; i < model.Count; i++)
                    jw.Write(model[i]);
            }
            jw.WriteEndArray();
        }

        public static string Write(this TreeNodeMap model)
        {
            if (model == default) return JsonConstants.Null;

            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb))
            using (var jw = new JsonTextWriter(sw))
            {
                Write(jw, model);
            }

            return sb.ToString();
        }

        #endregion
    }
}
