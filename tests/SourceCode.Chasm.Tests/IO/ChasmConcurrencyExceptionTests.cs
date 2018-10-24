using System;
using System.Runtime.Serialization;
using Moq;
using SourceCode.Chasm.Tests.Helpers;
using Xunit;

namespace SourceCode.Chasm.Repository.Tests
{
    public static class ChasmConcurrencyExceptionTests
    {
        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(ChasmConcurrencyException_Constructor_Empty))]
        public static void ChasmConcurrencyException_Constructor_Empty()
        {
            // Arrange & Action
            var actual = new ChasmConcurrencyException();

            // Assert
            Assert.NotNull(actual);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(ChasmConcurrencyException_Constructor_String))]
        public static void ChasmConcurrencyException_Constructor_String()
        {
            // Arrange
            string expectedMessage = RandomHelper.String;

            // Action
            var actual = new ChasmConcurrencyException(expectedMessage);

            // Assert
            Assert.NotNull(actual);
            Assert.Equal(expectedMessage, actual.Message);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(ChasmConcurrencyException_Constructor_String_Exception))]
        public static void ChasmConcurrencyException_Constructor_String_Exception()
        {
            // Arrange
            var expectedInnerException = new Exception(RandomHelper.String);
            string expectedMessage = RandomHelper.String;

            // Action
            var actual = new ChasmConcurrencyException(expectedMessage, expectedInnerException);

            // Assert
            Assert.NotNull(actual);
            Assert.Equal(expectedMessage, actual.Message);
            Assert.Equal(expectedInnerException, actual.InnerException);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(ChasmConcurrencyException_GetObjectData))]
        public static void ChasmConcurrencyException_GetObjectData()
        {
            // Arrange
            var mockFormatterConverter = new Mock<IFormatterConverter>();
            var mockSerializationInfo = new SerializationInfo(typeof(ChasmConcurrencyException), mockFormatterConverter.Object);
            var mockStreamingContext = new StreamingContext();

            // Action
            var actual = new ChasmConcurrencyException();
            actual.GetObjectData(mockSerializationInfo, mockStreamingContext);
        }
    }
}
