#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using Newtonsoft.Json;
using SourceCode.Chasm;

namespace SourceCode.Clay.Json
{
    internal static class JsonExtensions
    {
        #region Constants

        public const string JsonNull = "null";

        #endregion

        #region Methods

        public static Sha1? ReadSha1(this JsonReader jr)
        {
            var str = (string)jr.Value;
            if (string.IsNullOrEmpty(str))
                return null; // Caller can decide if they want null

            var sha1 = Sha1.Parse(str);
            return sha1;
        }

        #endregion
    }
}
