#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using System;
using Xunit;

namespace SourceCode.Chasm.IO.Tests
{
    public static partial class CommitTests
    {
        #region Methods

        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_Roundtrip_Commit_Default))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_Roundtrip_Commit_Default(IChasmSerializer ser)
        {
            var expected = Commit.Empty;
            using (var buf = ser.Serialize(expected))
            {
                var actual = ser.DeserializeCommit(buf.Result);
                Assert.Equal(expected, actual);
            }
        }

        #endregion
    }
}
