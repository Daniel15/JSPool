/*
 * Copyright (c) 2014-2016 Daniel Lo Nigro (Daniel15)
 * 
 * This source code is licensed under the BSD-style license found in the 
 * LICENSE file in the root directory of this source tree. 
 */

using JavaScriptEngineSwitcher.Core;

namespace JSPool
{
	/// <summary>
	/// Contains the configuration information for JSPool
	/// </summary>
	public class JsPoolConfig : JsPoolConfig<IJsEngine>
	{
		/// <summary>
		/// Creates a new JavaScript pool configuration. Default values will be set automatically.
		/// </summary>
		public JsPoolConfig()
		{
			EngineFactory = JsEngineSwitcher.Instance.CreateDefaultEngine;
		}
	}
}
