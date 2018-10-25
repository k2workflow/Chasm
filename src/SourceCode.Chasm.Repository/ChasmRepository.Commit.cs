using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using SourceCode.Clay;

namespace SourceCode.Chasm.Repository
{
    partial class ChasmRepository // .Commit
    {
        public virtual async ValueTask<Commit?> ReadCommitAsync(CommitId commitId, CancellationToken cancellationToken)
        {
            ReadOnlyMemory<byte>? buffer = await ReadObjectAsync(commitId.Sha1, cancellationToken).ConfigureAwait(false);

            if (buffer == null || buffer.Value.Length == 0) return default;

            Commit model = Serializer.DeserializeCommit(buffer.Value.Span);
            return model;
        }

        public virtual async ValueTask<CommitId> WriteCommitAsync(Commit commit, CancellationToken cancellationToken)
        {
            using (IMemoryOwner<byte> owner = Serializer.Serialize(commit, out int len))
            {
                Memory<byte> mem = owner.Memory.Slice(0, len);
                var sha1 = Sha1.Hash(mem.Span);

                await WriteObjectAsync(sha1, mem, false, cancellationToken).ConfigureAwait(false);

                var commitId = new CommitId(sha1);
                return commitId;
            }
        }
    }
}
