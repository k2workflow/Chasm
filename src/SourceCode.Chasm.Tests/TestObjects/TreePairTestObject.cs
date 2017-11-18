#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using SourceCode.Chasm.Tests.Helpers;
using System.Collections.Generic;

namespace SourceCode.Chasm.Tests.TestObjects
{
    public static class TreePairTestObject
    {
        #region Fields

        public static readonly KeyValuePair<string, TreeNode> Random = new KeyValuePair<string, TreeNode>(
            RandomHelper.String,
            TreeMapIdTestObject.Random);

        #endregion
    }
}
