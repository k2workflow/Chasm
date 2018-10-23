using Moq;
using SourceCode.Chasm.Repository;
using SourceCode.Chasm.Serializer;
using SourceCode.Chasm.Tests.Helpers;
using System;
using System.IO.Compression;
using System.Reflection;
using Xunit;

namespace SourceCode.Chasm.Repository.Tests
{
    public static class ChasmRepositoryTests
    {
        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(ChasmRepository_BuildConcurrencyException))]
        public static void ChasmRepository_BuildConcurrencyException()
        {
            // Arrange
            string expectedBranch = RandomHelper.String;
            string expectedName = RandomHelper.String;
            var expectedInnerException = new Exception(RandomHelper.String);

            // Action
            ChasmConcurrencyException actual = MockChasmRepository.MockBuildConcurrencyException(expectedBranch, expectedName, expectedInnerException);

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
            CompressionLevel expectedCompressionLevel = CompressionLevel.NoCompression;
            int expectedMaxDop = 5;
            var mockChasmRepository = new Mock<ChasmRepository>(mockChasmSerializer.Object, expectedCompressionLevel, expectedMaxDop);

            // Action
            ChasmRepository actual = mockChasmRepository.Object;

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
            int expectedMaxDop = default;
            var mockChasmRepository = new Mock<ChasmRepository>(mockChasmSerializer.Object, expectedCompressionLevel, expectedMaxDop);

            // Action
            ArgumentOutOfRangeException actual = Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                try
                {
                    ChasmRepository obj = mockChasmRepository.Object;
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
            CompressionLevel expectedCompressionLevel = CompressionLevel.NoCompression;
            int expectedMaxDop = int.MinValue;
            var mockChasmRepository = new Mock<ChasmRepository>(mockChasmSerializer.Object, expectedCompressionLevel, expectedMaxDop);

            // Action
            ArgumentOutOfRangeException actual = Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                try
                {
                    ChasmRepository obj = mockChasmRepository.Object;
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
            CompressionLevel expectedCompressionLevel = CompressionLevel.NoCompression;
            int expectedMaxDop = default;
            var mockChasmRepository = new Mock<ChasmRepository>(mockChasmSerializer.Object, expectedCompressionLevel, expectedMaxDop);

            // Action
            ArgumentOutOfRangeException actual = Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                try
                {
                    ChasmRepository obj = mockChasmRepository.Object;
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
            CompressionLevel expectedCompressionLevel = CompressionLevel.NoCompression;
            int expectedMaxDop = 5;
            var mockChasmRepository = new Mock<ChasmRepository>(chasmSerializer, expectedCompressionLevel, expectedMaxDop);

            // Action
            ArgumentNullException actual = Assert.Throws<ArgumentNullException>(() =>
            {
                try
                {
                    ChasmRepository obj = mockChasmRepository.Object;
                }
                catch (TargetInvocationException targetInvocationException)
                {
                    throw targetInvocationException.InnerException;
                }
            });

            // Assert
            Assert.Contains("serializer", actual.Message);
        }
    }
}
