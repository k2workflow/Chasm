#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using Newtonsoft.Json;
using SourceCode.Clay.Json;
using System.IO;

namespace SourceCode.Chasm.IO.Json.Wire
{
    internal static class CommitIdExtensions
    {
        #region Constants

        // Naming follows convention in ProtoSerializer

        private const string _id = "id";

        #endregion

        #region Read

        private static CommitId ReadCommitIdImpl(this JsonReader jr)
        {
            Sha1 sha1 = default;

            // Switch
            return jr.ReadObject(n =>
            {
                switch (n)
                {
                    case _id:
                        sha1 = jr.ReadSha1() ?? default;
                        break;
                }
            },

            // Factory
            () => sha1 == default ? default : new CommitId(sha1));
        }

        public static CommitId ReadCommitId(this string json)
        {
            if (string.IsNullOrEmpty(json)) return default;
            if (json == JsonExtensions.JsonNull) return default;

            using (var tr = new StringReader(json))
            using (var jr = new JsonTextReader(tr))
            {
                jr.DateParseHandling = DateParseHandling.None;

                var model = jr.ReadCommitIdImpl();
                return model;
            }
        }

        #endregion

        #region Write

        public static string Write(this CommitId model)
        {
            if (model == default) return JsonExtensions.JsonNull;

            // Perf: No need to use JsonWriter for a simple scalar
            var json = "{ \"" + _id + "\": \"" + model.Sha1.ToString("N") + "\" }";
            return json;
        }

        #endregion
    }
}
