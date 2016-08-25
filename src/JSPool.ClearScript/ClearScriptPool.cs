/*
 * Copyright (c) 2016 Daniel Lo Nigro (Daniel15)
 * 
 * This source code is licensed under the BSD-style license found in the 
 * LICENSE file in the root directory of this source tree. 
 */

using Microsoft.ClearScript.V8;

namespace JSPool.ClearScript
{
	/// <summary>
	/// Handles acquiring JavaScript engines from a shared pool, using ClearScript V8. 
	/// This class is thread-safe.
	/// </summary>
	public class ClearScriptPool : JsPool<V8ScriptEngine>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ClearScriptPool"/> class.
		/// </summary>
		/// <param name="config">The configuration.</param>
		public ClearScriptPool(ClearScriptPoolConfig config = null) 
			: base(config ?? new ClearScriptPoolConfig())
		{
		}

		/// <summary>
		/// Runs garbage collection for the specified engine
		/// </summary>
		/// <param name="engine"></param>
		protected override void CollectGarbage(V8ScriptEngine engine)
		{
			engine.CollectGarbage(exhaustive: false);
		}
	}
}
