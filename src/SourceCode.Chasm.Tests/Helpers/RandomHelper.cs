#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using System;

namespace SourceCode.Chasm.Tests.Helpers
{
    public static class RandomHelper
    {
        #region Properties

        public static byte[] ByteArray
        {
            get
            {
                var buffer = new byte[Random.Next()];
                Random.NextBytes(buffer);
                return buffer;
            }
        }

        public static Random Random => new Random(Guid.NewGuid().GetHashCode());

        public static string String => Guid.NewGuid().ToString("n");

        public static DateTime DateTime => DateTime.Now.Subtract(new TimeSpan(Random.Next(3650), Random.Next(12), Random.Next(59), Random.Next(59)));

        public static DateTimeOffset DateTimeOffset => new DateTimeOffset(DateTime);

        #endregion
    }
}
