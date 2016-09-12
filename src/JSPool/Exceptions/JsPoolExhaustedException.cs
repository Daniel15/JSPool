/*
 * Copyright (c) 2014 Daniel Lo Nigro (Daniel15)
 * 
 * This source code is licensed under the BSD-style license found in the 
 * LICENSE file in the root directory of this source tree. 
 */

using System;
#if !NETSTANDARD1_3
using System.Runtime.Serialization;
#endif

namespace JSPool.Exceptions
{
	/// <summary>
	/// Thrown when no engines are available in the pool.
	/// </summary>
#if !NETSTANDARD1_3
	[Serializable]
#endif
	public class JsPoolExhaustedException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="JsPoolExhaustedException"/> class.
		/// </summary>
		public JsPoolExhaustedException() : base() { }
		/// <summary>
		/// Initializes a new instance of the <see cref="JsPoolExhaustedException"/> class.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		public JsPoolExhaustedException(string message) : base(message) { }
		/// <summary>
		/// Initializes a new instance of the <see cref="JsPoolExhaustedException"/> class.
		/// </summary>
		/// <param name="message">The error message that explains the reason for the exception.</param>
		/// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
		public JsPoolExhaustedException(string message, Exception innerException)
			: base(message, innerException) { }

#if !NETSTANDARD1_3
		/// <summary>
		/// Used by deserialization
		/// </summary>
		protected JsPoolExhaustedException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{ }
#endif
	}
}
