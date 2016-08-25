/*
 * Copyright (c) 2014 Daniel Lo Nigro (Daniel15)
 * 
 * This source code is licensed under the BSD-style license found in the 
 * LICENSE file in the root directory of this source tree. 
 */

using System;
using System.Diagnostics;
using System.Reflection;
using JavaScriptEngineSwitcher.Core;

namespace JSPool
{
	/// <summary>
	/// Handles acquiring JavaScript engines from a shared pool, using JavaScriptEngineSwitcher. 
	/// This class is thread-safe.
	/// </summary>
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class JsPool : JsPool<IJsEngine>
	{
		private const string MSIE_TYPE = "MsieJsEngine";
		private const string V8_TYPE = "V8JsEngine";

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

		#region V8 Garbage Collection implementation
		private static FieldInfo _innerEngineField;
		private static MethodInfo _collectGarbageMethod;
		/// <summary>
		/// Runs garbage collection for the specified engine
		/// </summary>
		/// <param name="engine"></param>
		protected override void CollectGarbage(IJsEngine engine)
		{
			if (engine.GetType().Name != V8_TYPE)
			{
				return;
			}

			// Since JavaScriptEngineSwitcher does not expose the inner JavaScript engine, we need
			// to use reflection to get to it.
			if (_innerEngineField == null)
			{
				_innerEngineField = engine.GetType().GetField("_jsEngine", BindingFlags.NonPublic | BindingFlags.Instance);
			}
			var innerJsEngine = _innerEngineField.GetValue(engine);

			// Use reflection to get the garbage collection method so we don't have a hard 
			// dependency on ClearScript. Not ideal but this will do for now.
			if (_collectGarbageMethod == null)
			{
				_collectGarbageMethod = innerJsEngine.GetType().GetMethod("CollectGarbage");
			}
			_collectGarbageMethod.Invoke(innerJsEngine, new object[] { true });
		}
		#endregion
	}
}
