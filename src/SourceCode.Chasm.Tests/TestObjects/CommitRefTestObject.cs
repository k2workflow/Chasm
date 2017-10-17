#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using SourceCode.Chasm.Tests.Helpers;

namespace SourceCode.Chasm.Tests.TestObjects
{
    public static class CommitRefTestObject
    {
        #region Fields

        public static readonly CommitRef Random = new CommitRef(
            RandomHelper.String,
            CommitIdTestObject.Random);

        #endregion
    }
}
