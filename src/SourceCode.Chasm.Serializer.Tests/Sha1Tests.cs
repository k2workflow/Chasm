#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using Xunit;

namespace SourceCode.Chasm.IO.Tests
{
    public static class Sha1Tests
    {
        #region Methods

        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_Roundtrip_Sha1_Empty))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_Roundtrip_Sha1_Empty(IChasmSerializer ser)
        {
            var expected = Sha1.Empty;
            using (var buf = ser.Serialize(expected))
            {
                var actual = ser.DeserializeSha1(buf.Result);
                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_Roundtrip_Sha1))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_Roundtrip_Sha1(IChasmSerializer ser)
        {
            var expected = Sha1.Hash("a");
            using (var buf = ser.Serialize(expected))
            {
                var actual = ser.DeserializeSha1(buf.Result);
                Assert.Equal(expected, actual);
            }
        }

        #endregion
    }
}
