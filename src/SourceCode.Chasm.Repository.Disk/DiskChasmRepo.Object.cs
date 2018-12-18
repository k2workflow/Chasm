using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using SourceCode.Clay;

namespace SourceCode.Chasm.Repository.Disk
{
    partial class DiskChasmRepo // .Object
    {
        public override async Task<ReadOnlyMemory<byte>?> ReadObjectAsync(Sha1 objectId, CancellationToken cancellationToken)
        {
            string filename = DeriveFileName(objectId);
            string path = Path.Combine(_objectsContainer, filename);

            byte[] bytes = await ReadFileAsync(path, cancellationToken)
                .ConfigureAwait(false);

            if (bytes == null) return default;

            using (var input = new MemoryStream(bytes))
            using (var gzip = new GZipStream(input, CompressionMode.Decompress, false))
            using (var output = new MemoryStream())
            {
                input.Position = 0; // Else gzip returns []
                gzip.CopyTo(output);

                if (output.Length > 0)
                {
                    return output.ToArray(); // TODO: Perf
                }
            }

            return default;
        }

        public override async Task<Stream> ReadStreamAsync(Sha1 objectId, CancellationToken cancellationToken)
        {
            string filename = DeriveFileName(objectId);
            string path = Path.Combine(_objectsContainer, filename);

            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
                return default;

            if (!File.Exists(path))
                return default;

            FileStream fileStream = await WaitForFileAsync(path, FileMode.Open, FileAccess.Read, FileShare.Read, cancellationToken)
                .ConfigureAwait(false);
            var gzip = new GZipStream(fileStream, CompressionMode.Decompress, false);

            return gzip;
        }

        public override async Task<Sha1> WriteObjectAsync(Memory<byte> item, bool forceOverwrite, CancellationToken cancellationToken)
        {
            string tempPath = GetTempPath(); // Note that an empty file is created
            try
            {
                Sha1 sha1;
                using (var fs = new FileStream(tempPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
                using (var gz = new GZipStream(fs, CompressionLevel, true))
#pragma warning disable CA5350 // Do Not Use Weak Cryptographic Algorithms
                using (var ct = SHA1.Create())
#pragma warning restore CA5350 // Do Not Use Weak Cryptographic Algorithms
                using (var cs = new CryptoStream(gz, ct, CryptoStreamMode.Write))
                {
                    await cs.WriteAsync(item, cancellationToken)
                        .ConfigureAwait(false);

                    cs.FlushFinalBlock();
                    sha1 = new Sha1(ct.Hash);
                }

                string filename = DeriveFileName(sha1);
                string path = Path.Combine(_objectsContainer, filename);

                string dir = Path.GetDirectoryName(path);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                else if (File.Exists(path))
                {
                    // If file already exists then we can be sure it already contains the same content
                    if (!forceOverwrite)
                        return sha1;

                    File.Delete(path);
                }

                File.Move(tempPath, path);

                return sha1;
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }

        public override async Task<Sha1> WriteObjectAsync(Stream data, bool forceOverwrite, CancellationToken cancellationToken)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            string tempPath = GetTempPath(); // Note that an empty file is created
            try
            {
                Sha1 sha1;
                using (var fs = new FileStream(tempPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
                using (var gz = new GZipStream(fs, CompressionLevel, true))
#pragma warning disable CA5350 // Do Not Use Weak Cryptographic Algorithms
                using (var ct = SHA1.Create())
#pragma warning restore CA5350 // Do Not Use Weak Cryptographic Algorithms
                using (var cs = new CryptoStream(gz, ct, CryptoStreamMode.Write))
                {
                    await data.CopyToAsync(cs)
                        .ConfigureAwait(false);

                    cs.FlushFinalBlock();
                    sha1 = new Sha1(ct.Hash);
                }

                string filename = DeriveFileName(sha1);
                string path = Path.Combine(_objectsContainer, filename);

                string dir = Path.GetDirectoryName(path);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                else if (File.Exists(path))
                {
                    // If file already exists then we can be sure it already contains the same content
                    if (!forceOverwrite)
                        return sha1;

                    File.Delete(path);
                }

                File.Move(tempPath, path);

                return sha1;
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }
    }
}
