#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using Xunit;
using TreePair = System.Collections.Generic.KeyValuePair<string, SourceCode.Chasm.TreeNode>;

namespace SourceCode.Chasm.IO.Tests
{
    public static class TreeMapTests
    {
        #region Constants

        private static readonly TreePair Node1 = new TreeNode(new BlobId(Sha1.Hash(nameof(Node1)))).CreateMap(nameof(Node1));
        private static readonly TreePair Node2 = new TreeNode(new BlobId(Sha1.Hash(nameof(Node2)))).CreateMap(nameof(Node2));
        private static readonly TreePair Node3 = new TreeNode(new BlobId(Sha1.Hash(nameof(Node3)))).CreateMap(nameof(Node3));

        #endregion

        #region Methods

        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_Roundtrip_TreeMap_Default))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_Roundtrip_TreeMap_Default(IChasmSerializer ser)
        {
            TreeMap expected = default;
            using (var buf = ser.Serialize(expected))
            {
                var actual = ser.DeserializeTreeMap(buf.Result);
                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_Roundtrip_TreeMap_Empty))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_Roundtrip_TreeMap_Empty(IChasmSerializer ser)
        {
            var expected = new TreeMap();
            using (var buf = ser.Serialize(expected))
            {
                var actual = ser.DeserializeTreeMap(buf.Result);
                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_Roundtrip_TreeMap_Null))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_Roundtrip_TreeMap_Null(IChasmSerializer ser)
        {
            var expected = new TreeMap(null);
            using (var buf = ser.Serialize(expected))
            {
                var actual = ser.DeserializeTreeMap(buf.Result);
                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_Roundtrip_TreeMap_Empty_Array))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_Roundtrip_TreeMap_Empty_Array(IChasmSerializer ser)
        {
            var expected = new TreeMap();
            using (var buf = ser.Serialize(expected))
            {
                var actual = ser.DeserializeTreeMap(buf.Result);
                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_Roundtrip_TreeMap_1_Node))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_Roundtrip_TreeMap_1_Node(IChasmSerializer ser)
        {
            var expected = new TreeMap(Node1);
            using (var buf = ser.Serialize(expected))
            {
                var actual = ser.DeserializeTreeMap(buf.Result);
                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_Roundtrip_TreeMap_2_Nodes))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_Roundtrip_TreeMap_2_Nodes(IChasmSerializer ser)
        {
            var expected = new TreeMap(Node1, Node2);
            using (var buf = ser.Serialize(expected))
            {
                var actual = ser.DeserializeTreeMap(buf.Result);
                Assert.Equal(expected, actual);
            }
        }

        [Trait("Type", "Unit")]
        [Theory(DisplayName = nameof(ChasmSerializer_Roundtrip_TreeMap_3_Nodes))]
        [ClassData(typeof(TestData))]
        public static void ChasmSerializer_Roundtrip_TreeMap_3_Nodes(IChasmSerializer ser)
        {
            var expected = new TreeMap(Node1, Node2, Node3);
            using (var buf = ser.Serialize(expected))
            {
                var actual = ser.DeserializeTreeMap(buf.Result);
                Assert.Equal(expected, actual);
            }
        }

        #endregion
    }
}
