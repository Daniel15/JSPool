﻿/*
 * Copyright (c) 2014-2016 Daniel Lo Nigro (Daniel15)
 * 
 * This source code is licensed under the BSD-style license found in the 
 * LICENSE file in the root directory of this source tree. 
 */

using System;
using System.Collections.Generic;
using JSPool.Exceptions;

namespace JSPool
{
	/// <summary>
	/// Contains the configuration information for JSPool
	/// </summary>
	public class JsPoolConfig<T>
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
		public Action<T> Initializer { get; set; }

		/// <summary>
		/// Gets or sets the function method used to create engines. Defaults to the standard 
		/// JsEngineSwitcher factory method.
		/// </summary>
		public Func<T> EngineFactory { get; set; }

		/// <summary>
		/// Gets or sets the maximum number of times an engine can be reused before it is disposed.
		/// <c>0</c> is unlimited.
		/// </summary>
		public int MaxUsagesPerEngine { get; set; }

		/// <summary>
		/// Gets or sets the number of times an engine can be reused before its garbage collector
		/// is ran. Only used if the engine supports garbage collection (V8).
		/// </summary>
		public int GarbageCollectionInterval { get; set; }

		/// <summary>
		/// Gets or sets the path to watch for file changes. If any files in this path change,
		/// all engines in the pool will be recycled
		/// </summary>
		public string WatchPath { get; set; }

		/// <summary>
		/// Gets or sets the file paths to watch for file changes. Requires 
		/// <see cref="WatchPath" /> to be set too. If not set, all files in 
		/// <see cref="WatchPath" /> will be watched.
		/// </summary>
		public IEnumerable<string> WatchFiles { get; set; }

		/// <summary>
		/// Creates a new JavaScript pool configuration. Default values will be set automatically.
		/// </summary>
		public JsPoolConfig()
		{
			StartEngines = 10;
			MaxEngines = 25;
			MaxUsagesPerEngine = 100;
			GarbageCollectionInterval = 20;
			GetEngineTimeout = TimeSpan.FromSeconds(5);
			Initializer = engine => { };
		}
	}
}
