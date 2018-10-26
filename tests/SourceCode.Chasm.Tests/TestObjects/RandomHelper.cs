using System;

namespace SourceCode.Chasm.Tests.TestObjects
{
    public static class RandomHelper
    {
        public static byte[] ByteArray
        {
            get
            {
                byte[] buffer = new byte[Random.Next()];
                Random.NextBytes(buffer);
                return buffer;
            }
        }

        public static Random Random => new Random(Guid.NewGuid().GetHashCode());

        public static string String => Guid.NewGuid().ToString("n");

        public static DateTime DateTime => DateTime.Now.Subtract(new TimeSpan(Random.Next(3650), Random.Next(12), Random.Next(59), Random.Next(59)));

        public static DateTimeOffset DateTimeOffset => new DateTimeOffset(DateTime);
    }
}
