using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SourceCode.Clay;
using crypt = System.Security.Cryptography;

namespace SourceCode.Chasm.Repository
{
    partial class ChasmRepository // .File
    {
        public static async Task<Sha1> WriteFileAsync(Func<Stream, Task> writeAction, Func<Sha1, string, Task> fileAction, bool delete, CancellationToken cancellationToken)
        {
            if (writeAction == null) throw new ArgumentNullException(nameof(writeAction));

            // Note that an empty file is physically created
            var tempPath = Path.GetTempFileName();

            try
            {
                Sha1 sha1;
                using (var fs = new FileStream(tempPath, FileMode.Open, FileAccess.Write, FileShare.Read))
                using (var ct = crypt.SHA1.Create())
                {
                    using (var cs = new crypt.CryptoStream(fs, ct, crypt.CryptoStreamMode.Write, false))
                    {
                        await writeAction(cs)
                            .ConfigureAwait(false);
                    }

                    sha1 = new Sha1(ct.Hash);
                }

                if (fileAction != null)
                {
                    await fileAction(sha1, tempPath)
                        .ConfigureAwait(false);
                }

                return sha1;
            }
            finally
            {
                if (delete
                    && File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }
        }

        public static Task<Sha1> WriteFileAsync(Stream stream, Func<Sha1, string, Task> fileAction, bool delete, CancellationToken cancellationToken)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            Task FileAction(Stream inner)
                => stream.CopyToAsync(inner, cancellationToken);

            return WriteFileAsync(FileAction, fileAction, delete, cancellationToken);
        }

        public static Task<Sha1> WriteFileAsync(Memory<byte> buffer, Func<Sha1, string, Task> fileAction, bool delete, CancellationToken cancellationToken)
        {
            Task FileAction(Stream inner)
                => inner.WriteAsync(buffer, cancellationToken).AsTask();

            return WriteFileAsync(FileAction, fileAction, delete, cancellationToken);
        }
    }
}
