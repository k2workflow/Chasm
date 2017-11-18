#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using SourceCode.Chasm.Tests.Helpers;

namespace SourceCode.Chasm.Tests.TestObjects
{
    public static class CommitTestObject
    {
        #region Fields

        public static readonly Commit Random = new Commit(
            CommitIdTestObject.Random,
            TreeMapIdTestObject.Random,
            AuditTestObject.Random,
            AuditTestObject.Random,
            RandomHelper.String);

        #endregion
    }
}
