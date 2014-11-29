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
	/// Extension methods for <see cref="IJsEngine"/>.
	/// </summary>
	public static class JsEngineExtensions
	{
		/// <summary>
		/// Determines if the specified engine can only be used on the thread it is created on.
		/// </summary>
		/// <param name="engine">Engine</param>
		/// <returns><c>true</c> if the engine should be confined to a single thread</returns>
		public static bool NeedsOwnThread(this IJsEngine engine)
		{
			// Checking MsieJsEngine as a string so we don't need to pull in an otherwise
			// unneeded dependency on MsieJsEngine.
			return engine.GetType().Name == "MsieJsEngine";
		}
	}
}
