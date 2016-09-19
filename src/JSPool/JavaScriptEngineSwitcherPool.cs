/*
 * Copyright (c) 2014 Daniel Lo Nigro (Daniel15)
 * 
 * This source code is licensed under the BSD-style license found in the 
 * LICENSE file in the root directory of this source tree. 
 */

using System;
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
		private const string MSIE_TYPE = "MsieJsEngine";

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
		/// Gets a factory method used to create engines.
		/// </summary>
		protected override Func<IJsEngine> CreateEngineFactory()
		{
			using (var tempEngine = _config.EngineFactory())
			{
				if (!NeedsOwnThread(tempEngine))
				{
					// Engine is fine with being accessed across multiple threads, we can just
					// return its factory directly.
					return _config.EngineFactory;
				}
				// Engine needs special treatment. This is the case with the MSIE engine, which
				// can only be accessed from the thread it was created on. In this case we need
				// to create the engine in a separate thread and marshall the requests across.
				return () => new JsEngineWithOwnThread(_config.EngineFactory, _cancellationTokenSource.Token);
			}
		}

		/// <summary>
		/// Determines if the specified engine can only be used on the thread it is created on.
		/// </summary>
		/// <param name="engine">Engine</param>
		/// <returns><c>true</c> if the engine should be confined to a single thread</returns>
		private static bool NeedsOwnThread(IJsEngine engine)
		{
			// Checking MsieJsEngine as a string so we don't need to pull in an otherwise
			// unneeded dependency on MsieJsEngine.
			return engine.GetType().Name == MSIE_TYPE;
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
