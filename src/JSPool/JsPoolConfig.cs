/*
 * Copyright (c) 2014 Daniel Lo Nigro (Daniel15)
 * 
 * This source code is licensed under the BSD-style license found in the 
 * LICENSE file in the root directory of this source tree. 
 */

using System;
using JavaScriptEngineSwitcher.Core;
using JSPool.Exceptions;

namespace JSPool
{
	/// <summary>
	/// Contains the configuration information for JSPool
	/// </summary>
	public class JsPoolConfig
	{
		/// <summary>
		/// Gets or sets the number of engines to initially start when a pool is created. 
		/// Defaults to <c>10</c>.
		/// </summary>
		public int StartEngines { get; set; }

		/// <summary>
		/// Gets or sets the maximum number of engines that will be created in the pool. 
		/// Defaults to <c>25</c>.
		/// </summary>
		public int MaxEngines { get; set; }

		/// <summary>
		/// Gets or sets the default timeout to use when acquiring an engine from the pool.
		/// If an engine can not be acquired in this timeframe, a 
		/// <see cref="JsPoolExhaustedException"/> will be thrown.
		/// </summary>
		public TimeSpan GetEngineTimeout { get; set; }

		/// <summary>
		/// Gets or sets the code to run when a new engine is created. This should configure
		/// the environment and set up any required JavaScript libraries.
		/// </summary>
		public Action<IJsEngine> Initializer { get; set; }

		/// <summary>
		/// Gets or sets the function method used to create engines. Defaults to the standard 
		/// JsEngineSwitcher factory method.
		/// </summary>
		public Func<IJsEngine> EngineFactory { get; set; }

		/// <summary>
		/// Creates a new JavaScript pool configuration. Default values will be set automatically.
		/// </summary>
		public JsPoolConfig()
		{
			StartEngines = 10;
			MaxEngines = 25;
			GetEngineTimeout = TimeSpan.FromSeconds(5);
			EngineFactory = () => JsEngineSwitcher.Current.CreateDefaultJsEngineInstance();
			Initializer = engine => { };
		}
	}
}
