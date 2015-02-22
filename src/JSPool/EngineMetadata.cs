/*
 * Copyright (c) 2014 Daniel Lo Nigro (Daniel15)
 * 
 * This source code is licensed under the BSD-style license found in the 
 * LICENSE file in the root directory of this source tree. 
 */

namespace JSPool
{
	/// <summary>
	/// Contains metadata relating to a JavaScript engine.
	/// </summary>
	public class EngineMetadata
	{
		/// <summary>
		/// Gets or sets whether this JavaScript engine is currently in use.
		/// </summary>
		public bool InUse { get; set; }

		/// <summary>
		/// Gets or sets the number of times this engine has been used.
		/// </summary>
		public int UsageCount { get; set; }
	}
}
