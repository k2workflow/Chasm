#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using Newtonsoft.Json;
using SourceCode.Clay.Json;
using System.IO;
using System.Text;

namespace SourceCode.Chasm.IO.Json.Wire
{
    internal static class TreeNodeMapExtensions
    {
        #region Read

        private static TreeNodeMap ReadTreeImpl(this JsonReader jr)
        {
            var list = jr.ReadArray(() => jr.ReadNode());

            var tree = new TreeNodeMap(list);
            return tree;
        }

        public static TreeNodeMap ReadTree(this string json)
        {
            if (string.IsNullOrEmpty(json)) return default;
            if (json == JsonExtensions.JsonNull) return default;

            using (var tr = new StringReader(json))
            using (var jr = new JsonTextReader(tr))
            {
                jr.DateParseHandling = DateParseHandling.None;

                var model = jr.ReadTreeImpl();
                return model;
            }
        }

        #endregion

        #region Write

        public static string Write(this TreeNodeMap model)
        {
            if (model == default) return JsonExtensions.JsonNull;

            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb))
            using (var jw = new JsonTextWriter(sw))
            {
                jw.WriteStartArray();
                {
                    for (var i = 0; i < model.Count; i++)
                        jw.Write(model[i]);
                }
                jw.WriteEndArray();
            }

            return sb.ToString();
        }

        #endregion
    }
}
