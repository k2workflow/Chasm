#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using SourceCode.Clay.Json;
using System.Json;

namespace SourceCode.Chasm.IO.Json.Wire
{
    internal static class CommitIdWireExtensions
    {
        #region Constants

        // Naming follows convention in ProtoSerializer

        private const string _id = "id";

        #endregion

        #region Methods

        public static JsonObject Convert(this CommitId model)
        {
            var wire = new JsonObject
            {
                [_id] = model.Sha1.ToString("N")
            };

            return wire;
        }

        public static CommitId ConvertCommitId(this JsonObject wire)
        {
            if (wire == null) return default;

            var jv = wire.GetValue(_id, JsonType.String, false);
            var sha1 = Sha1.Parse(jv);

            var model = new CommitId(sha1);
            return model;
        }

        public static CommitId ParseCommitId(this string json)
        {
            var wire = json.ParseJsonObject();

            var model = wire.ConvertCommitId();
            return model;
        }

        #endregion
    }
}
