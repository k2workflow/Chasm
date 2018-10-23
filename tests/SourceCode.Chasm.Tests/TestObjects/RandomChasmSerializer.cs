using System;
using System.Buffers;
using SourceCode.Chasm.Serializer;

namespace SourceCode.Chasm.Tests.TestObjects
{
    public class RandomChasmSerializer : IChasmSerializer
    {
        public Commit DeserializeCommit(ReadOnlySpan<byte> span) => CommitTestObject.Random;

        public CommitId DeserializeCommitId(ReadOnlySpan<byte> span) => CommitIdTestObject.Random;

        public TreeNodeMap DeserializeTree(ReadOnlySpan<byte> span) => TreeNodeMapTestObject.Random;

        public IMemoryOwner<byte> Serialize(TreeNodeMap model, out int length)
        {
            length = 1;
            return MemoryPool<byte>.Shared.Rent(1);
        }

        public IMemoryOwner<byte> Serialize(CommitId model, out int length)
        {
            length = 1;
            return MemoryPool<byte>.Shared.Rent(1);
        }

        public IMemoryOwner<byte> Serialize(Commit model, out int length)
        {
            length = 1;
            return MemoryPool<byte>.Shared.Rent(1);
        }
    }
}
