#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace SourceCode.Chasm
{
    /// <summary>
    /// Represents a <see cref="Sha1"/> value.
    /// </summary>
    /// <seealso cref="SHA1" />
    /// <seealso cref="System.IEquatable{T}" />
    /// <seealso cref="System.IComparable{T}" />
    [DebuggerDisplay("{ToString(\"D\"),nq,ac}")]
    public struct Sha1 : IEquatable<Sha1>, IComparable<Sha1>
    {
        #region Constants

        // Use a thread-local instance of the underlying crypto algorithm.
        private static readonly ThreadLocal<SHA1> _sha1 = new ThreadLocal<SHA1>(SHA1.Create);

        /// <summary>
        /// The fixed byte length of a <see cref="Sha1"/> value.
        /// </summary>
        public const byte ByteLen = 20;

        /// <summary>
        /// The number of hex characters required to represent a <see cref="Sha1"/> value.
        /// </summary>
        public const byte CharLen = ByteLen * 2;

        /// <summary>
        /// A singleton representing an empty <see cref="Sha1"/> value.
        /// </summary>
        /// <value>
        /// The empty.
        /// </value>
        public static Sha1 Empty { get; }

        // 40

        #endregion

        #region Properties

        // We choose to use value types for primary storage so that we can live on the stack
        // Using byte[] or String means a dereference to the heap (& fixed byte would require unsafe)

        public ulong Blit0 { get; }

        public ulong Blit1 { get; }

        public uint Blit2 { get; }

        /// <summary>
        /// Returns a <see cref="Memory{T}"/> from the <see cref="Sha1"/>.
        /// </summary>
        public Memory<byte> Memory
        {
            get
            {
                var buffer = new byte[ByteLen];

                unsafe
                {
                    fixed (byte* ptr = buffer)
                    {
                        // Code is valid per BitConverter.ToInt32|64 (see #1 elsewhere in this class)
                        *(ulong*)(&ptr[0 + 0]) = Blit0;
                        *(ulong*)(&ptr[0 + 8]) = Blit1;
                        *(uint*)(&ptr[0 + 16]) = Blit2;
                    }
                }

                var mem = new Memory<byte>(buffer);
                return mem;
            }
        }

        #endregion

        #region De/Constructors

        /// <summary>
        /// Deserializes a <see cref="Sha1"/> value from the provided blits.
        /// </summary>
        /// <param name="blit0">The blit0.</param>
        /// <param name="blit1">The blit1.</param>
        /// <param name="blit2">The blit2.</param>
        public Sha1(ulong blit0, ulong blit1, uint blit2)
        {
            Blit0 = blit0;
            Blit1 = blit1;
            Blit2 = blit2;
        }

        /// <summary>
        /// Deserializes a <see cref="Sha1"/> value from the provided <see cref="Byte[]"/> starting at the specified position.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <exception cref="ArgumentNullException">buffer</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// buffer - buffer
        /// or
        /// offset - buffer
        /// </exception>
        [SecuritySafeCritical]
        public Sha1(byte[] buffer, int offset)
        {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));

            if (buffer.Length < ByteLen)
                throw new ArgumentOutOfRangeException(nameof(buffer), $"{nameof(buffer)} must have length at least {ByteLen}");

            if (offset < 0 || offset + ByteLen > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(offset), $"{nameof(buffer)} must have length at least {ByteLen}");

            unsafe
            {
                fixed (byte* ptr = buffer)
                {
                    // Code is valid per BitConverter.ToInt32|64 (see #1 elsewhere in this class)
                    Blit0 = *(ulong*)(&ptr[offset + 0]);
                    Blit1 = *(ulong*)(&ptr[offset + 8]);
                    Blit2 = *(uint*)(&ptr[offset + 16]);
                }
            }
        }

        /// <summary>
        /// Deserializes a <see cref="Sha1"/> value from the provided <see cref="ReadOnlyMemory{T}"/>.
        /// </summary>
        /// <param name="span">The buffer.</param>
        /// <exception cref="ArgumentOutOfRangeException">buffer - buffer</exception>
        [SecuritySafeCritical]
        public Sha1(ReadOnlySpan<byte> span)
        {
            if (span.IsEmpty || span.Length < ByteLen)
                throw new ArgumentOutOfRangeException(nameof(span), $"{nameof(span)} must have length at least {ByteLen}");

            unsafe
            {
                fixed (byte* ptr = &span.DangerousGetPinnableReference())
                {
                    // Code is valid per BitConverter.ToInt32|64 (see #1 elsewhere in this class)
                    Blit0 = *(ulong*)(&ptr[0]);
                    Blit1 = *(ulong*)(&ptr[8]);
                    Blit2 = *(uint*)(&ptr[16]);
                }
            }
        }

        /// <summary>
        /// Deconstructs the specified <see cref="Sha1"/>.
        /// </summary>
        /// <param name="blit0">The blit0.</param>
        /// <param name="blit1">The blit1.</param>
        /// <param name="blit2">The blit2.</param>
        public void Deconstruct(out ulong blit0, out ulong blit1, out uint blit2)
        {
            blit0 = Blit0;
            blit1 = Blit1;
            blit2 = Blit2;
        }

        #endregion

        #region Factory

        /// <summary>
        /// Hashes the specified value using utf8 encoding.
        /// </summary>
        /// <param name="value">The string to hash.</param>
        /// <returns></returns>
        public static Sha1 Hash(string value)
        {
            if (value == null) return Empty;
            // Note that length=0 should not short-circuit

            // Rent buffer
            var maxLen = Encoding.UTF8.GetMaxByteCount(value.Length); // Utf8 is 1-4 bpc
            var rented = ArrayPool<byte>.Shared.Rent(maxLen);
            var count = Encoding.UTF8.GetBytes(value, 0, value.Length, rented, 0);

            var hash = _sha1.Value.ComputeHash(rented, 0, count);
            var sha1 = new Sha1(hash);

            // Return buffer
            ArrayPool<byte>.Shared.Return(rented);

            return sha1;
        }

        /// <summary>
        /// Hashes the specified bytes.
        /// </summary>
        /// <param name="bytes">The bytes to hash.</param>
        /// <returns></returns>
        public static Sha1 Hash(byte[] bytes)
        {
            if (bytes == null) return Empty;
            // Note that length=0 should not short-circuit

            var hash = _sha1.Value.ComputeHash(bytes);

            var sha1 = new Sha1(hash);
            return sha1;
        }

        /// <summary>
        /// Hashes the specified bytes, starting at the specified offset and count.
        /// </summary>
        /// <param name="bytes">The bytes to hash.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="count">The count.</param>
        /// <returns></returns>
        public static Sha1 Hash(byte[] bytes, int offset, int count)
        {
            if (bytes == null) return Empty;
            // Note that length=0 should not short-circuit

            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            if (offset < 0 || offset + count > bytes.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));

            var hash = _sha1.Value.ComputeHash(bytes, offset, count);

            var sha1 = new Sha1(hash);
            return sha1;
        }

        /// <summary>
        /// Hashes the specified bytes.
        /// </summary>
        /// <param name="bytes">The bytes to hash.</param>
        /// <returns></returns>
        public static Sha1 Hash(ArraySegment<byte> bytes)
        {
            if (bytes.Array == null) return Empty;
            // Note that length=0 should not short-circuit

            var hash = _sha1.Value.ComputeHash(bytes.Array, bytes.Offset, bytes.Count);

            var sha1 = new Sha1(hash);
            return sha1;
        }

        /// <summary>
        /// Hashes the specified stream.
        /// </summary>
        /// <param name="stream">The stream to hash.</param>
        /// <returns></returns>
        public static Sha1 Hash(Stream stream)
        {
            if (stream == null) return Empty;
            // Note that length=0 should not short-circuit

            var hash = _sha1.Value.ComputeHash(stream);

            var sha1 = new Sha1(hash);
            return sha1;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Copies the <see cref="Sha1"/> value to the provided buffer.
        /// </summary>
        /// <param name="buffer">The buffer to copy to.</param>
        /// <param name="offset">The offset.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">buffer</exception>
        /// <exception cref="ArgumentOutOfRangeException">offset - buffer</exception>
        [SecuritySafeCritical]
        public int CopyTo(byte[] buffer, int offset)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            if (offset < 0 || checked(offset + ByteLen) > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(offset), $"{nameof(buffer)} must have length at least {ByteLen}");

            unsafe
            {
                fixed (byte* ptr = buffer)
                {
                    // Code is valid per BitConverter.ToInt32|64 (see #1 elsewhere in this class)
                    *(ulong*)(&ptr[offset + 0]) = Blit0;
                    *(ulong*)(&ptr[offset + 8]) = Blit1;
                    *(uint*)(&ptr[offset + 16]) = Blit2;
                }
            }

            return ByteLen;
        }

        #endregion

        #region ToString

        private const char FormatN = (char)0;

        [SecuritySafeCritical]
        private char[] ToChars(char separator)
        {
            Debug.Assert(separator == FormatN || separator == '-' || separator == ' ');

            var sep = 0;
            char[] chars;

            // Text is treated as 5 groups of 8 chars (4 bytes); 4 separators optional
            if (separator == FormatN)
            {
                chars = new char[CharLen];
            }
            else
            {
                sep = 8;
                chars = new char[CharLen + 4];
            }

            unsafe
            {
                var bytes = stackalloc byte[ByteLen]; // TODO: https://github.com/dotnet/corefx/pull/24212
                {
                    // Code is valid per BitConverter.ToInt32|64 (see #1 elsewhere in this class)
                    *(ulong*)(&bytes[0]) = Blit0;
                    *(ulong*)(&bytes[8]) = Blit1;
                    *(uint*)(&bytes[16]) = Blit2;
                }

                var pos = 0;
                for (var i = 0; i < ByteLen; i++) // 20
                {
                    // Each byte is two hexits (convention is lowercase)

                    var b = bytes[i] >> 4; // == b / 16
                    chars[pos++] = (char)(b < 10 ? b + '0' : b - 10 + 'a');

                    b = bytes[i] - (b << 4); // == b % 16
                    chars[pos++] = (char)(b < 10 ? b + '0' : b - 10 + 'a');

                    // Append a separator if required
                    if (pos == sep) // pos >= 2, sep = 0|N
                    {
                        chars[pos++] = separator;

                        sep = pos + 8;
                        if (sep >= chars.Length)
                            sep = 0;
                    }
                }
            }

            return chars;
        }

        /// <summary>
        /// Returns a string representation of the <see cref="Sha1"/> instance using the 'N' format.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var chars = ToChars(FormatN);
            return new string(chars);
        }

        /// <summary>
        /// Returns a string representation of the <see cref="Sha1"/> instance.
        /// N: a9993e364706816aba3e25717850c26c9cd0d89d,
        /// D: a9993e36-4706816a-ba3e2571-7850c26c-9cd0d89d,
        /// S: a9993e36 4706816a ba3e2571 7850c26c 9cd0d89d
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        public string ToString(string format)
        {
            if (string.IsNullOrWhiteSpace(format))
                throw new FormatException($"Empty format specification");

            if (format.Length != 1)
                throw new FormatException($"Invalid format specification length {format.Length}");

            switch (format[0])
            {
                // a9993e364706816aba3e25717850c26c9cd0d89d
                case 'n':
                case 'N':
                    {
                        var chars = ToChars(FormatN);
                        return new string(chars);
                    }

                // a9993e36-4706816a-ba3e2571-7850c26c-9cd0d89d
                case 'd':
                case 'D':
                    {
                        var chars = ToChars('-');
                        return new string(chars);
                    }

                // a9993e36 4706816a ba3e2571 7850c26c 9cd0d89d
                case 's':
                case 'S':
                    {
                        var chars = ToChars(' ');
                        return new string(chars);
                    }
            }

            throw new FormatException($"Invalid format specification '{format}'");
        }

        /// <summary>
        /// Converts the <see cref="Sha1"/> instance to a string using the 'N' format,
        /// and returns the value split into two tokens.
        /// </summary>
        /// <param name="prefixLength">The length of the first token.</param>
        /// <returns></returns>
        public KeyValuePair<string, string> Split(int prefixLength)
        {
            var chars = ToChars(FormatN);

            if (prefixLength <= 0)
                return new KeyValuePair<string, string>(string.Empty, new string(chars));

            if (prefixLength >= CharLen)
                return new KeyValuePair<string, string>(new string(chars), string.Empty);

            var key = new string(chars, 0, prefixLength);
            var val = new string(chars, prefixLength, chars.Length - prefixLength);

            var kvp = new KeyValuePair<string, string>(key, val);
            return kvp;
        }

        #endregion

        #region Parse

        // Sentinel value for n/a (128)
        private const byte __ = 0b1000_0000;

        // '0'=48, '9'=57
        // 'A'=65, 'F'=70
        // 'a'=97, 'f'=102
        private static readonly byte[] Hexits = new byte['f' - '0' + 1] // 102 - 48 + 1 = 55
        {
            00, 01, 02, 03, 04, 05, 06, 07, 08, 09, // [00-09]       = 48..57 = '0'..'9'
            __, __, __, __, __, __, __, 10, 11, 12, // [10-16,17-19] = 65..67 = 'A'..'C'
            13, 14, 15, __, __, __, __, __, __, __, // [20-22,23-29] = 68..70 = 'D'..'F'
            __, __, __, __, __, __, __, __, __, __, // [30-39]
            __, __, __, __, __, __, __, __, __, 10, // [40-48,49]    = 97..97 = 'a'
            11, 12, 13, 14, 15                      // [50-54]       = 98..102= 'b'..'f'
        };

        [SecuritySafeCritical]
        private static bool TryParseImpl(string hex, int startIndex, out Sha1 value)
        {
            Debug.Assert(hex != null);
            Debug.Assert(startIndex >= 0);
            Debug.Assert(hex.Length >= CharLen + startIndex);

            value = Empty;

            unsafe
            {
                var bytes = stackalloc byte[ByteLen]; // TODO: https://github.com/dotnet/corefx/pull/24212

                // Text is treated as 5 groups of 8 chars (4 bytes); 4 separators optional
                // "34aa973c-d4c4daa4-f61eeb2b-dbad2731-6534016f"
                var pos = startIndex;
                for (var i = 0; i < 5; i++)
                {
                    for (var j = 0; j < 4; j++)
                    {
                        // Two hexits per byte: aaaa bbbb
                        if (!TryParseHexit(hex[pos++], out byte h1)
                            || !TryParseHexit(hex[pos++], out byte h2))
                            return false;

                        bytes[i * 4 + j] = (byte)((h1 << 4) | h2);
                    }

                    if (pos < CharLen && (hex[pos] == '-' || hex[pos] == ' '))
                        pos++;
                }

                // If the string is not fully consumed, it had an invalid length
                if (pos != hex.Length)
                    return false;

                // Code is valid per BitConverter.ToInt32|64 (see #1 elsewhere in this class)
                var blit0 = *(ulong*)(&bytes[0]);
                var blit1 = *(ulong*)(&bytes[8]);
                var blit2 = *(uint*)(&bytes[16]);

                value = new Sha1(blit0, blit1, blit2);
                return true;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryParseHexit(char c, out byte b)
        {
            b = 0;

            if (c < '0' || c > 'f')
                return false;

            var bex = Hexits[c - '0'];
            if (bex == __) // Sentinel value for n/a (128)
                return false;

            b = bex;
            return true;
        }

        /// <summary>
        /// Parses the specified hexadecimal.
        /// </summary>
        /// <param name="hex">The hexadecimal.</param>
        /// <returns></returns>
        /// <exception cref="FormatException">Sha1</exception>
        public static Sha1 Parse(string hex)
        {
            if (!TryParse(hex, out Sha1 sha1))
                throw new FormatException($"String was not recognized as a valid {nameof(Sha1)}");

            return sha1;
        }

        /// <summary>
        /// Tries to parse the specified hexadecimal.
        /// </summary>
        /// <param name="hex">The hexadecimal.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static bool TryParse(string hex, out Sha1 value)
        {
            value = Empty;

            // Length must be at least 40
            if (hex == null || hex.Length < CharLen)
                return false;

            var startIndex = 0;

            // Check if the hex specifier '0x' is present
            if (hex[0] == '0' && (hex[1] == 'x' || hex[1] == 'X'))
            {
                // Length must be at least 42
                if (hex.Length < 2 + CharLen)
                    return false;

                // Skip '0x'
                startIndex = 2;
            }

            if (!TryParseImpl(hex, startIndex, out Sha1 sha1))
                return false;

            value = sha1;
            return true;
        }

        #endregion

        #region IEquatable

        public bool Equals(Sha1 other) => Sha1Comparer.Default.Equals(this, other);

        public override bool Equals(object obj)
            => obj is Sha1 sha1
            && Sha1Comparer.Default.Equals(this, sha1);

        public override int GetHashCode() => Sha1Comparer.Default.GetHashCode(this);

        #endregion

        #region IComparable

        public int CompareTo(Sha1 other) => Sha1Comparer.Default.Compare(this, other);

        #endregion

        #region Operators

        public static bool operator ==(Sha1 x, Sha1 y) => Sha1Comparer.Default.Equals(x, y);

        public static bool operator !=(Sha1 x, Sha1 y) => !(x == y);

        public static bool operator >=(Sha1 x, Sha1 y) => Sha1Comparer.Default.Compare(x, y) >= 0;

        public static bool operator >(Sha1 x, Sha1 y) => Sha1Comparer.Default.Compare(x, y) > 0;

        public static bool operator <=(Sha1 x, Sha1 y) => Sha1Comparer.Default.Compare(x, y) <= 0;

        public static bool operator <(Sha1 x, Sha1 y) => Sha1Comparer.Default.Compare(x, y) < 0;

        #endregion
    }
}
