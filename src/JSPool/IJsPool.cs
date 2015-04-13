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
	/// Handles acquiring JavaScript engines from a shared pool. This class is thread safe.
	/// </summary>
	public interface IJsPool : IDisposable
	{
		/// <summary>
		/// Gets an engine from the pool. This engine should be returned to the pool via
		/// <see cref="JsPool.ReturnEngineToPool"/> when you are finished with it.
		/// If an engine is free, this method returns immediately with the engine.
		/// If no engines are available but we have not reached <see cref="JsPoolConfig.MaxEngines"/>
		/// yet, creates a new engine. If MaxEngines has been reached, blocks until an engine is
		/// avaiable again.
		/// </summary>
		/// <param name="timeout">
		/// Maximum time to wait for a free engine. If not specified, defaults to the timeout 
		/// specified in the configuration.
		/// </param>
		/// <returns>A JavaScript engine</returns>
		/// <exception cref="JsPoolExhaustedException">
		/// Thrown if no engines are available in the pool within the provided timeout period.
		/// </exception>
		IJsEngine GetEngine(TimeSpan? timeout = null);

		/// <summary>
		/// Returns an engine to the pool so it can be reused
		/// </summary>
		/// <param name="engine">Engine to return</param>
		void ReturnEngineToPool(IJsEngine engine);

		/// <summary>
		/// Gets the total number of engines in this engine pool, including engines that are
		/// currently busy.
		/// </summary>
		int EngineCount { get; }

		/// <summary>
		/// Gets the number of currently available engines in this engine pool.
		/// </summary>
		int AvailableEngineCount { get; }

		/// <summary>
		/// Disposes the specified engine
		/// </summary>
		/// <param name="engine">Engine to dispose</param>
		/// <param name="repopulateEngines">
		/// If <c>true</c>, a new engine will be created to replace the disposed engine
		/// </param>
		void DisposeEngine(IJsEngine engine, bool repopulateEngines = true);

		/// <summary>
		/// Disposes all engines in this pool, and creates new engines in their place.
		/// </summary>
		void Recycle();
	}
}