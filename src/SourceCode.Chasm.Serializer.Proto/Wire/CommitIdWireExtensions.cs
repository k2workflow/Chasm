#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

namespace SourceCode.Chasm.IO.Proto.Wire
{
    internal static class CommitIdWireExtensions
    {
        #region Methods

        public static CommitIdWire Convert(this CommitId model)
        {
            var wire = new CommitIdWire
            {
                Sha1 = model.Sha1.Convert()
            };

            return wire;
        }

        public static CommitId Convert(this CommitIdWire wire)
        {
            if (wire == null) return default;

            var sha1 = wire.Sha1.Convert();

            var model = new CommitId(sha1);
            return model;
        }

        #endregion
    }
}
