#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

namespace SourceCode.Chasm.IO.Text.Wire
{
    internal static class CommitIdWireExtensions
    {
        #region Methods

        public static string Convert(this CommitId model)
        {
            var wire = model.Sha1.ToString("N");
            return wire;
        }

        public static CommitId ConvertCommitId(this string wire)
        {
            if (string.IsNullOrWhiteSpace(wire)) return default;

            var sha1 = Sha1.Parse(wire);

            var model = new CommitId(sha1);
            return model;
        }

        #endregion
    }
}
