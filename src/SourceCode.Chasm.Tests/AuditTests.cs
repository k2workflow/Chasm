using SourceCode.Chasm.Tests.Helpers;
using SourceCode.Chasm.Tests.TestObjects;
using System;
using Xunit;

namespace SourceCode.Chasm.Tests
{
    public static class AuditTests
    {
        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(Audit_Deconstruct))]
        public static void Audit_Deconstruct()
        {
            // Arrange
            var expectedName = RandomHelper.String;
            DateTimeOffset expectedDateTimeOffset = RandomHelper.DateTimeOffset;
            var audit = new Audit(expectedName, expectedDateTimeOffset);

            // Action
            audit.Deconstruct(out var actualName, out DateTimeOffset actualDateTimeOffset);

            // Assert
            Assert.Equal(expectedName, actualName);
            Assert.Equal(expectedDateTimeOffset, actualDateTimeOffset);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(Audit_Equals_Object))]
        public static void Audit_Equals_Object()
        {
            // Arrange
            Audit audit = AuditTestObject.Random;

            // Action
            Assert.True(audit.Equals((object)audit));
            Assert.False(audit.Equals(new object()));
        }
    }
}
