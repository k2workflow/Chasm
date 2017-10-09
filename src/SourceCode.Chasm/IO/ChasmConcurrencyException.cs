#region License

// Copyright (c) K2 Workflow (SourceCode Technology Holdings Inc.). All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#endregion

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace SourceCode.Chasm.IO
{
    [Serializable]
    public sealed class ChasmConcurrencyException : System.Data.DataException
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ChasmConcurrencyException"/> class.
        /// This is the default constructor.
        /// </summary>
        public ChasmConcurrencyException()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChasmConcurrencyException"/> class with the specified string.
        /// </summary>
        /// <param name="s">The string to display when the exception is thrown.</param>
        public ChasmConcurrencyException(string s)
            : base(s)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChasmConcurrencyException"/> class with the specified string and inner exception.
        /// </summary>
        /// <param name="s">The string to display when the exception is thrown.</param>
        /// <param name="innerException">A reference to an inner exception.</param>
        public ChasmConcurrencyException(string s, Exception innerException)
            : base(s, innerException)
        { }

        #endregion

        #region Serializable

        // https://stackoverflow.com/questions/94488/what-is-the-correct-way-to-make-a-custom-net-exception-serializable

        // Private for sealed, protected for open (accessed via reflection by runtime)
        [ExcludeFromCodeCoverage]
        private ChasmConcurrencyException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
            => base.GetObjectData(info, context);

        #endregion
    }
}
