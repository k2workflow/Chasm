#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using Newtonsoft.Json;
using SourceCode.Clay.Json;
using System;
using System.IO;

namespace SourceCode.Chasm.IO.Json.Wire
{
    internal static class Sha1Extensions
    {
        #region Methods

        /// <summary>
        /// Reads a <see cref="string"/> and, if not <see langword="null"/>, parses it as a <see cref="Sha1"/>.
        /// </summary>
        /// <param name="jr"></param>
        /// <returns></returns>
        public static Sha1? ReadSha1(this JsonReader jr)
        {
            if (jr == null) throw new ArgumentNullException(nameof(jr));

            var str = (string)jr.Value;
            if (string.IsNullOrEmpty(str))
                return null; // Caller decides how to handle null

            var sha1 = Sha1.Parse(str);
            return sha1;
        }

        public static Sha1? ReadSha1(this string json)
        {
            if (json == null || json == JsonConstants.Null) return null;

            using (var tr = new StringReader(json))
            using (var jr = new JsonTextReader(tr))
            {
                jr.DateParseHandling = DateParseHandling.None;

                var model = ReadSha1(jr);
                return model;
            }
        }

        #endregion
    }
}
