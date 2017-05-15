/*
 * Copyright (c) 2014 Daniel Lo Nigro (Daniel15)
 * 
 * This source code is licensed under the BSD-style license found in the 
 * LICENSE file in the root directory of this source tree. 
 */

using System.Diagnostics;
using JavaScriptEngineSwitcher.Core;

namespace JSPool
{
	/// <summary>
	/// Handles acquiring JavaScript engines from a shared pool. This class is thread-safe.
	/// </summary>
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class JsPool : JsPool<IJsEngine>, IJsPool
	{
		/// <summary>
		/// Creates a new JavaScript engine pool
		/// </summary>
		/// <param name="config">
		/// The configuration to use. If not provided, a default configuration will be used.
		/// </param>
		public JsPool(JsPoolConfig config = null) 
			: base(config ?? new JsPoolConfig())
		{
		}

		/// <summary>
		/// Runs garbage collection for the specified engine
		/// </summary>
		/// <param name="engine"></param>
		protected override void CollectGarbage(IJsEngine engine)
		{
			if (engine.SupportsGarbageCollection)
			{
				engine.CollectGarbage();
			}
		}
	}
}
