using System;
using System.Threading;
using System.Threading.Tasks;

namespace SourceCode.Chasm.Tests
{
    /// <summary>
    /// Values used for testing.
    /// </summary>
    public static class TestValues
    {
        public static readonly Audit Audit = new Audit(Guid.NewGuid().ToString(), DateTimeOffset.MaxValue);
        public static readonly string Branch = Guid.NewGuid().ToString();
        public static readonly CancellationToken CancellationToken = new CancellationToken(false);
        public static readonly Commit Commit = new Commit(CommitId, TreeId, Audit, Audit, Guid.NewGuid().ToString());
        public static readonly CommitId CommitId = new CommitId(Sha1);
        public static readonly CommitRef CommitRef = new CommitRef(Guid.NewGuid().ToString(), CommitId);
        public static readonly string HashValue = Guid.NewGuid().ToString();
        public static readonly string Message = Guid.NewGuid().ToString();
        public static readonly ParallelOptions ParallelOptions = new ParallelOptions() { CancellationToken = CancellationToken };
        public static readonly ReadOnlyMemory<byte> ReadOnlyMemory = new ReadOnlyMemory<byte>(new byte[] { 1, 2, 3, 4 });
        public static readonly Sha1 Sha1 = Sha1.Hash(HashValue);
        public static readonly TreeId TreeId = new TreeId(Sha1);
        public static readonly TreeNode TreeNode = new TreeNode(Guid.NewGuid().ToString(), TreeId);
        public static readonly TreeNodeList TreeNodeList = new TreeNodeList(TreeNode);
    }
}