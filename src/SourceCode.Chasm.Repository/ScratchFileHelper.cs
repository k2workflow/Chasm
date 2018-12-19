using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using SourceCode.Clay;
using crypt = System.Security.Cryptography;

namespace SourceCode.Chasm.Repository
{
    public static class ScratchFileHelper
    {
        public static async Task<(Sha1 Sha1, string Path)> WriteAsync(string directory, Memory<byte> item, CompressionLevel compressionLevel, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(directory)) throw new ArgumentNullException(nameof(directory));

            string tempPath = Path.Combine(directory, Path.GetTempFileName()); // Note that an empty file is created

            using (var fs = new FileStream(tempPath, FileMode.Open, FileAccess.Write, FileShare.None))
            using (var gz = new GZipStream(fs, compressionLevel, false))
            using (var ct = crypt.SHA1.Create())
            {
                using (var cs = new crypt.CryptoStream(gz, ct, crypt.CryptoStreamMode.Write))
                {
                    await cs.WriteAsync(item, cancellationToken)
                        .ConfigureAwait(false);
                }

                var sha1 = new Sha1(ct.Hash);
                return (sha1, tempPath);
            }
        }

        public static async Task<(Sha1 Sha1, string Path)> WriteAsync(string directory, Stream stream, CompressionLevel compressionLevel, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(directory)) throw new ArgumentNullException(nameof(directory));
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            string tempPath = Path.Combine(directory, Path.GetTempFileName()); // Note that an empty file is created

            using (var fs = new FileStream(tempPath, FileMode.Open, FileAccess.Write, FileShare.None))
            using (var gz = new GZipStream(fs, compressionLevel, false))
            using (var ct = crypt.SHA1.Create())
            {
                using (var cs = new crypt.CryptoStream(gz, ct, crypt.CryptoStreamMode.Write))
                {
                    await stream.CopyToAsync(cs, cancellationToken)
                        .ConfigureAwait(false);
                }

                var sha1 = new Sha1(ct.Hash);
                return (sha1, tempPath);
            }
        }
    }
}
