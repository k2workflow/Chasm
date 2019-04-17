#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using System;

namespace SourceCode.Chasm.Repository.Internal
{
    /// <summary>
    /// Represents extensions for <see cref="Exception"/>.
    /// </summary>
    internal static class ExceptionExtensions
    {
        /// <summary>
        /// Utility method that can be used in a try catch handler to capture and ignore an exception.
        /// Note that using exceptions for control flow is bad-practice. This should be use frugally.
        /// </summary>
        /// <remarks>
        /// Used to indicate that an exception has been intentionally suppressed.
        /// </remarks>
        /// <param name="exception">The exception to suppress.</param>
#pragma warning disable RECS0154 // Parameter is never used
        public static Exception Suppress(this Exception exception)
#pragma warning restore RECS0154 // Parameter is never used
        {
            // Do nothing by design
            return exception;
        }
    }
}
