using System;
using SourceCode.Chasm.Serializer;

namespace SourceCode.Chasm.Tests.TestObjects
{
    public class RandomChasmSerializer : IChasmSerializer
    {
        private readonly OwnerTrackingPool<byte> _pool = new OwnerTrackingPool<byte>();

        public Commit DeserializeCommit(ReadOnlySpan<byte> span)
            => CommitTestObject.Random;

        public CommitId DeserializeCommitId(ReadOnlySpan<byte> span)
            => CommitIdTestObject.Random;

        public TreeNodeMap DeserializeTree(ReadOnlySpan<byte> span)
            => TreeNodeMapTestObject.Random;

        public Memory<byte> Serialize(TreeNodeMap model)
            => _pool.Rent(1).Memory;

        public Memory<byte> Serialize(CommitId model)
            => _pool.Rent(1).Memory;

        public Memory<byte> Serialize(Commit model)
            => _pool.Rent(1).Memory;
    }
}
