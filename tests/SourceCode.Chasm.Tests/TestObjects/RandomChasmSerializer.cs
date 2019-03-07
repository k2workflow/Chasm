using System;
using System.Buffers;
using SourceCode.Chasm.Serializer;

namespace SourceCode.Chasm.Tests.TestObjects
{
    public class RandomChasmSerializer : IChasmSerializer
    {
        private static readonly MemoryPool<byte> s_pool = MemoryPool<byte>.Shared;

        public Commit DeserializeCommit(ReadOnlySpan<byte> span)
            => CommitTestObject.Random;

        public CommitId DeserializeCommitId(ReadOnlySpan<byte> span)
            => CommitIdTestObject.Random;

        public TreeNodeMap DeserializeTree(ReadOnlySpan<byte> span)
            => TreeNodeMapTestObject.Random;

        public IMemoryOwner<byte> Serialize(TreeNodeMap model)
            => s_pool.Rent(1);

        public IMemoryOwner<byte> Serialize(CommitId model)
            => s_pool.Rent(1);

        public IMemoryOwner<byte> Serialize(Commit model)
            => s_pool.Rent(1);
    }
}
