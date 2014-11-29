/*
 * Copyright (c) 2014 Daniel Lo Nigro (Daniel15)
 * 
 * This source code is licensed under the BSD-style license found in the 
 * LICENSE file in the root directory of this source tree. 
 */

using JavaScriptEngineSwitcher.Core;

namespace JSPool
{
	/// <summary>
	/// An adapter for <see cref="IJsEngine"/> implementations that always need to be used from
	/// the same thread they are created on.
	/// </summary>
	public interface IJsEngineWithOwnThread : IJsEngine
	{
		/// <summary>
		/// Gets a value indicating the execution status of this engine's thread.
		/// </summary>
		bool IsThreadAlive { get; }
	}
}