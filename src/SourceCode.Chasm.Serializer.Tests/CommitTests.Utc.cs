#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using System;
using Xunit;

namespace SourceCode.Chasm.IO.Tests
{
    public static partial class CommitTests // .Utc
    {
        #region Constants

        private static readonly DateTimeOffset Utc1 = new DateTimeOffset(new DateTime(new DateTime(2000, 1, 1).Ticks, DateTimeKind.Utc));

        #endregion

        #region Methods

        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_Roundtrip_Commit_Utc))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_Roundtrip_Commit_Utc(IChasmSerializer ser)
        {
            var expected = new Commit(new CommitId?(), default, new Audit("bob", Utc1), new Audit("mary", Utc1), null, default);
            using (var buf = ser.Serialize(expected))
            {
                var actual = ser.DeserializeCommit(buf.Result);
                Assert.Equal(expected, actual);
            }
        }

        #endregion
    }
}
