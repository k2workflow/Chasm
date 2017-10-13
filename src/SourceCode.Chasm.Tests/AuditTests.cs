#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using System;
using SourceCode.Chasm.Tests.Helpers;
using SourceCode.Chasm.Tests.TestObjects;
using Xunit;

namespace SourceCode.Chasm.Tests
{
    public static class AuditTests
    {
        #region Methods

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(Audit_Deconstruct))]
        public static void Audit_Deconstruct()
        {
            // Arrange
            var expectedName = RandomHelper.String;
            var expectedDateTimeOffset = RandomHelper.DateTimeOffset;
            var audit = new Audit(expectedName, expectedDateTimeOffset);

            // Action
            audit.Deconstruct(out var actualName, out var actualDateTimeOffset);

            // Assert
            Assert.Equal(expectedName, actualName);
            Assert.Equal(expectedDateTimeOffset, actualDateTimeOffset);
        }

        [Trait("Type", "Unit")]
        [Fact(DisplayName = nameof(Audit_Equals_Object))]
        public static void Audit_Equals_Object()
        {
            // Arrange
            var audit = AuditTestObject.Random;

            // Action
            Assert.True(audit.Equals((object)audit));
            Assert.False(audit.Equals(new object()));
        }

        #endregion
    }
}
