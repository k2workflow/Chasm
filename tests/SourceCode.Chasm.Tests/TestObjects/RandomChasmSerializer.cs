using System;
using System.Buffers;
using SourceCode.Chasm.Serializer;

namespace SourceCode.Chasm.Tests.TestObjects
{
    public class RandomChasmSerializer : IChasmSerializer
    {
        private static readonly MemoryPool<byte> s_pool = MemoryPool<byte>.Shared;

        public TreeNodeMap DeserializeTree(ReadOnlySpan<byte> span)
            => TreeNodeMapTestObject.Random;

        public IMemoryOwner<byte> Serialize(TreeNodeMap model)
            => s_pool.Rent(1);
    }
}
