#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using System;
using System.IO.Compression;
using System.Reflection;
using Moq;
using SourceCode.Chasm.IO;
using Xunit;

namespace SourceCode.Chasm.Tests.IO
{
    public static class ChasmRepositoryTests
    {
        #region Methods

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(ChasmRepository_BuildConcurrencyException))]
        public static void ChasmRepository_BuildConcurrencyException()
        {
            // Arrange
            var expectedBranch = Guid.NewGuid().ToString();
            var expectedName = Guid.NewGuid().ToString();
            var expectedInnerException = new Exception(Guid.NewGuid().ToString());

            // Action
            var actual = MockChasmRepository.MockBuildConcurrencyException(expectedBranch, expectedName, expectedInnerException);

            // Assert
            Assert.Equal(expectedInnerException, actual.InnerException);
            Assert.Contains(nameof(CommitRef), actual.Message);
            Assert.Contains(expectedBranch, actual.Message);
            Assert.Contains(expectedName, actual.Message);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(ChasmRepository_Constructor_ChasmSerializer_CompressionLevel_MaxDop))]
        public static void ChasmRepository_Constructor_ChasmSerializer_CompressionLevel_MaxDop()
        {
            // Arrange
            var mockChasmSerializer = new Mock<IChasmSerializer>();
            var expectedCompressionLevel = CompressionLevel.NoCompression;
            var expectedMaxDop = 5;
            var mockChasmRepository = new Mock<ChasmRepository>(mockChasmSerializer.Object, expectedCompressionLevel, expectedMaxDop);

            // Action
            var actual = mockChasmRepository.Object;

            // Assert
            Assert.NotNull(actual);
            Assert.Equal(mockChasmSerializer.Object, actual.Serializer);
            Assert.Equal(expectedCompressionLevel, actual.CompressionLevel);
            Assert.Equal(expectedMaxDop, actual.MaxDop);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(ChasmRepository_Constructor_OutOfRange_CompressionLevel))]
        public static void ChasmRepository_Constructor_OutOfRange_CompressionLevel()
        {
            // Arrange
            var mockChasmSerializer = new Mock<IChasmSerializer>();
            var expectedCompressionLevel = (CompressionLevel)int.MaxValue;
            var expectedMaxDop = default(int);
            var mockChasmRepository = new Mock<ChasmRepository>(mockChasmSerializer.Object, expectedCompressionLevel, expectedMaxDop);

            // Action
            var actual = Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                try
                {
                    var obj = mockChasmRepository.Object;
                }
                catch (TargetInvocationException targetInvocationException)
                {
                    throw targetInvocationException.InnerException;
                }
            });

            // Assert
            Assert.Contains("compressionLevel", actual.Message);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(ChasmRepository_Constructor_OutOfRange_MaxDop_Negative))]
        public static void ChasmRepository_Constructor_OutOfRange_MaxDop_Negative()
        {
            // Arrange
            var mockChasmSerializer = new Mock<IChasmSerializer>();
            var expectedCompressionLevel = CompressionLevel.NoCompression;
            var expectedMaxDop = int.MinValue;
            var mockChasmRepository = new Mock<ChasmRepository>(mockChasmSerializer.Object, expectedCompressionLevel, expectedMaxDop);

            // Action
            var actual = Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                try
                {
                    var obj = mockChasmRepository.Object;
                }
                catch (TargetInvocationException targetInvocationException)
                {
                    throw targetInvocationException.InnerException;
                }
            });

            // Assert
            Assert.Contains("maxDop", actual.Message);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(ChasmRepository_Constructor_OutOfRange_MaxDop_Zero))]
        public static void ChasmRepository_Constructor_OutOfRange_MaxDop_Zero()
        {
            // Arrange
            var mockChasmSerializer = new Mock<IChasmSerializer>();
            var expectedCompressionLevel = CompressionLevel.NoCompression;
            var expectedMaxDop = default(int);
            var mockChasmRepository = new Mock<ChasmRepository>(mockChasmSerializer.Object, expectedCompressionLevel, expectedMaxDop);

            // Action
            var actual = Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                try
                {
                    var obj = mockChasmRepository.Object;
                }
                catch (TargetInvocationException targetInvocationException)
                {
                    throw targetInvocationException.InnerException;
                }
            });

            // Assert
            Assert.Contains("maxDop", actual.Message);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(ChasmRepository_Constructor_ChasmSerializer_CompressionLevel_MaxDop))]
        public static void ChasmRepository_Constructor_SerialzerNull()
        {
            // Arrange
            var chasmSerializer = default(IChasmSerializer);
            var expectedCompressionLevel = CompressionLevel.NoCompression;
            var expectedMaxDop = 5;
            var mockChasmRepository = new Mock<ChasmRepository>(chasmSerializer, expectedCompressionLevel, expectedMaxDop);

            // Action
            var actual = Assert.Throws<ArgumentNullException>(() =>
            {
                try
                {
                    var obj = mockChasmRepository.Object;
                }
                catch (TargetInvocationException targetInvocationException)
                {
                    throw targetInvocationException.InnerException;
                }
            });

            // Assert
            Assert.Contains("serializer", actual.Message);
        }

        #endregion
    }
}
