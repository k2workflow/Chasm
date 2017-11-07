#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using Newtonsoft.Json;

namespace SourceCode.Chasm.IO.Json.Wire
{
    internal static class Sha1Extensions
    {
        #region Constants

        public const string JsonNull = "null";

        #endregion

        #region Methods

        /// <summary>
        /// Reads a <see cref="string"/> and, if not <see langword="null"/>, parses it as a <see cref="Sha1"/>.
        /// </summary>
        /// <param name="jr"></param>
        /// <returns></returns>
        public static Sha1? ReadSha1(this JsonReader jr)
        {
            var str = (string)jr.Value;
            if (string.IsNullOrEmpty(str))
                return null; // Caller decides how to handle null

            var sha1 = Sha1.Parse(str);
            return sha1;
        }

        #endregion
    }
}
