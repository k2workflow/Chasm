#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using Xunit;
using TreePair = System.Collections.Generic.KeyValuePair<string, SourceCode.Chasm.TreeNode>;

namespace SourceCode.Chasm.IO.Tests
{
    public static class TreeNodeTests
    {
        #region Methods

        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_WriteRead_NullTreeMap))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_WriteRead_NullTreeMap(IChasmSerializer ser)
        {
            var expected = new TreeMap();

            using (var seg = ser.Serialize(expected))
            {
                var actual = ser.DeserializeTreeMap(seg.Result);

                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_WriteRead_EmptyTreeMap))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_WriteRead_EmptyTreeMap(IChasmSerializer ser)
        {
            var expected = new TreeMap(new TreePair[0]);

            using (var seg = ser.Serialize(expected))
            {
                var actual = ser.DeserializeTreeMap(seg.Result);

                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_WriteRead_TreeMap))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_WriteRead_TreeMap(IChasmSerializer ser)
        {
            var node0 = new TreeNode(NodeKind.Blob, Sha1.Hash("abc")).CreateMap("a");
            var node1 = new TreeNode(NodeKind.Map, Sha1.Hash("def")).CreateMap("b");
            var node2 = new TreeNode(NodeKind.Map, Sha1.Hash("hij")).CreateMap("c");
            var expected = new TreeMap(node0, node1, node2);

            using (var seg = ser.Serialize(expected))
            {
                var actual = ser.DeserializeTreeMap(seg.Result);

                Assert.Equal(expected, actual);
            }
        }

        #endregion
    }
}
