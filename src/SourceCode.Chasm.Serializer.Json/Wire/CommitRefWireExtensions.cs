#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using SourceCode.Clay.Json;
using System;
using System.Json;

namespace SourceCode.Chasm.IO.Json.Wire
{
    internal static class CommitRefWireExtensions
    {
        #region Constants

        private const string _sha1 = "sha1";

        #endregion

        #region Methods

        public static JsonObject Convert(this CommitRef model)
        {
            if (model == CommitRef.Empty) return default; // null

            var wire = new JsonObject
            {
                [_sha1] = model.CommitId.Sha1.ToString("N")
            };

            return wire;
        }

        public static CommitRef ConvertCommitRef(this JsonObject wire, string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            if (wire == null) return new CommitRef(name, CommitId.Empty);

            // Sha1
            var jv = wire.GetValue(_sha1, JsonType.String, false);
            var sha1 = Sha1.Parse(jv);

            var commitId = new CommitId(sha1);

            var model = new CommitRef(name, commitId);
            return model;
        }

        public static CommitRef ParseCommitRef(this string json, string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

            var wire = json.ParseJsonObject();

            var model = ConvertCommitRef(wire, name);
            return model;
        }

        #endregion
    }
}
