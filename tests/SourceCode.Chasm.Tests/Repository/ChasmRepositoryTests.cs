using System;
using System.Reflection;
using Moq;
using SourceCode.Chasm.Serializer;
using SourceCode.Chasm.Tests.TestObjects;
using Xunit;

namespace SourceCode.Chasm.Repository.Tests
{
    public static class ChasmRepositoryTests
    {
        [Trait("Type", "Unit")]
        [Fact]
        public static void ChasmRepository_BuildConcurrencyException()
        {
            // Arrange
            string expectedBranch = RandomHelper.String;
            string expectedName = RandomHelper.String;
            var expectedInnerException = new Exception(RandomHelper.String);

            // Action
            ChasmConcurrencyException actual = MockChasmRepository.MockBuildConcurrencyException(expectedBranch, expectedName, expectedInnerException, null);

            // Assert
            Assert.Equal(expectedInnerException, actual.InnerException);
            Assert.Contains(expectedBranch, actual.Message);
            Assert.Contains(expectedName, actual.Message);
        }

        [Trait("Type", "Unit")]
        [Fact]
        public static void ChasmRepository_Constructor_ChasmSerializer_MaxDop()
        {
            // Arrange
            var mockChasmSerializer = new Mock<IChasmSerializer>();

            var mockChasmRepository = new Mock<ChasmRepository>(mockChasmSerializer.Object);

            // Action
            ChasmRepository actual = mockChasmRepository.Object;

            // Assert
            Assert.NotNull(actual);
            Assert.Equal(mockChasmSerializer.Object, actual.Serializer);
        }

        [Trait("Type", "Unit")]
        [Fact]
        public static void ChasmRepository_Constructor_SerializerNull()
        {
            // Arrange
            var chasmSerializer = default(IChasmSerializer);
            var mockChasmRepository = new Mock<ChasmRepository>(chasmSerializer);

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
