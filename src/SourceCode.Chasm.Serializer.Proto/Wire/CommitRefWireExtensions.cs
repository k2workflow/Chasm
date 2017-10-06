#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

namespace SourceCode.Chasm.IO.Proto.Wire
{
    internal static class CommitRefWireExtensions
    {
        #region Methods

        public static CommitRefWire Convert(this CommitRef model)
        {
            if (model == CommitRef.Empty) return new CommitRefWire();

            var wire = new CommitRefWire
            {
                CommitId = model.CommitId.Sha1.Convert()
            };

            return wire;
        }

        public static CommitRef Convert(this CommitRefWire wire)
        {
            if (wire == null) return default;

            var sha1 = wire.CommitId.Convert();
            var commitId = new CommitId(sha1);

            var model = new CommitRef(commitId);
            return model;
        }

        #endregion
    }
}
