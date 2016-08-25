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
	/// Contains the configuration information for JSPool
	/// </summary>
	public class ClearScriptPoolConfig : JsPoolConfig<V8ScriptEngine>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ClearScriptPoolConfig"/> class.
		/// </summary>
		public ClearScriptPoolConfig() : base()
		{
			EngineFactory = () => new V8ScriptEngine();
		}
	}
}
