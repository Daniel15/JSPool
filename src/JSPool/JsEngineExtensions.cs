/*
 * Copyright (c) 2014 Daniel Lo Nigro (Daniel15)
 * 
 * This source code is licensed under the BSD-style license found in the 
 * LICENSE file in the root directory of this source tree. 
 */

using System;
using System.Reflection;
using JavaScriptEngineSwitcher.Core;

namespace JSPool
{
	/// <summary>
	/// Extension methods for <see cref="IJsEngine"/>.
	/// </summary>
	public static class JsEngineExtensions
	{
		private const string MSIE_TYPE = "MsieJsEngine";
		private const string V8_TYPE = "V8JsEngine";

		/// <summary>
		/// Determines if the specified engine can only be used on the thread it is created on.
		/// </summary>
		/// <param name="engine">Engine</param>
		/// <returns><c>true</c> if the engine should be confined to a single thread</returns>
		public static bool NeedsOwnThread(this IJsEngine engine)
		{
			// Checking MsieJsEngine as a string so we don't need to pull in an otherwise
			// unneeded dependency on MsieJsEngine.
			return engine.GetType().Name == MSIE_TYPE;
		}

		/// <summary>
		/// Determines if the specified engine supports garbage collection.
		/// </summary>
		/// <param name="engine">Engine</param>
		/// <returns><c>true</c> if the engine supports garbage collection</returns>
		public static bool SupportsGarbageCollection(this IJsEngine engine)
		{
			return engine.GetType().Name == V8_TYPE;
		}

		/// <summary>
		/// Collecs garbage in the specified engine.
		/// </summary>
		/// <param name="engine">Engine to collect garbage in</param>
		public static void CollectGarbage(this IJsEngine engine)
		{
			if (!engine.SupportsGarbageCollection())
			{
				throw new InvalidOperationException("Engine doesn't support garbage collection.");
			}

			V8CollectGarbage(engine);
		}

		#region V8 Garbage Collection implementation
		private static FieldInfo _innerEngineField;
		private static MethodInfo _collectGarbageMethod;

		/// <summary>
		/// Collects garbage in the specified V8 engine.
		/// </summary>
		/// <param name="engine"></param>
		private static void V8CollectGarbage(IJsEngine engine)
		{
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
