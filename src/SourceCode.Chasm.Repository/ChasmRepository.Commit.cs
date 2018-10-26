using System;
using System.Threading;
using System.Threading.Tasks;
using SourceCode.Chasm.Serializer;
using SourceCode.Clay;
using SourceCode.Clay.Buffers;

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
            using (var pool = new ArenaMemoryPool<byte>())
            {
                Memory<byte> mem = Serializer.Serialize(commit, pool);

                Sha1 sha1 = Hasher.HashData(mem.Span);
                await WriteObjectAsync(sha1, mem, false, cancellationToken).ConfigureAwait(false);

                var commitId = new CommitId(sha1);
                return commitId;
            }
        }
    }
}
