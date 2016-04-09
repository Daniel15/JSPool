/*
 * Copyright (c) 2014 Daniel Lo Nigro (Daniel15)
 * 
 * This source code is licensed under the BSD-style license found in the 
 * LICENSE file in the root directory of this source tree. 
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using JavaScriptEngineSwitcher.Core;
using JSPool.Exceptions;

namespace JSPool
{
	/// <summary>
	/// Handles acquiring JavaScript engines from a shared pool. This class is thread-safe.
	/// </summary>
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class JsPool : IJsPool
	{
		/// <summary>
		/// Configuration for this engine pool.
		/// </summary>
		protected readonly JsPoolConfig _config;
		/// <summary>
		/// Engines that are currently available for use.
		/// </summary>
		protected readonly BlockingCollection<IJsEngine> _availableEngines = new BlockingCollection<IJsEngine>();
		/// <summary>
		/// Metadata for the engines.
		/// </summary>
		protected readonly IDictionary<IJsEngine, EngineMetadata> _metadata = new ConcurrentDictionary<IJsEngine, EngineMetadata>();
		/// <summary>
		/// Totan number of engines that have been created.
		/// </summary>
		protected int _engineCount;
		/// <summary>
		/// Factory method used to create engines.
		/// </summary>
		protected readonly Func<IJsEngine> _engineFactory;
		/// <summary>
		/// Handles watching for changes to files, to recycle the engines if any related files change.
		/// </summary>
		protected IFileWatcher _fileWatcher;

		/// <summary>
		/// Used to cancel threads when disposing the class.
		/// </summary>
		protected readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
		/// <summary>
		/// Lock object used when creating a new engine
		/// </summary>
		private readonly object _engineCreationLock = new object();

		/// <summary>
		/// Occurs when any watched files have changed (including renames and deletions).
		/// </summary>
		public event EventHandler Recycled;

		/// <summary>
		/// Creates a new JavaScript engine pool
		/// </summary>
		/// <param name="config">
		/// The configuration to use. If not provided, a default configuration will be used.
		/// </param>
		public JsPool(JsPoolConfig config = null)
		{
			_config = config ?? new JsPoolConfig();
			_engineFactory = CreateEngineFactory();
			PopulateEngines();
			InitializeWatcher();
		}

		/// <summary>
		/// Gets a factory method used to create engines.
		/// </summary>
		protected virtual Func<IJsEngine> CreateEngineFactory()
		{
			using (var tempEngine = _config.EngineFactory())
			{
				if (!tempEngine.NeedsOwnThread())
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
		/// Initializes a <see cref="FileWatcher"/> if enabled in the configuration.
		/// </summary>
		protected virtual void InitializeWatcher()
		{
			if (!string.IsNullOrEmpty(_config.WatchPath))
			{
				_fileWatcher = new FileWatcher
				{
					Path = _config.WatchPath,
					Files = _config.WatchFiles,
				};
				_fileWatcher.Changed += (sender, args) => Recycle();
				_fileWatcher.Start();
			}
		}

		/// <summary>
		/// Ensures that at least <see cref="JsPoolConfig.StartEngines"/> engines have been created.
		/// </summary>
		protected virtual void PopulateEngines()
		{
			while (EngineCount < _config.StartEngines)
			{
				var engine = CreateEngine();
				_availableEngines.Add(engine);
			}
		}

		/// <summary>
		/// Creates a new JavaScript engine and adds it to the list of all available engines.
		/// </summary>
		protected virtual IJsEngine CreateEngine()
		{
			var engine = _engineFactory();
			_config.Initializer(engine);
			_metadata[engine] = new EngineMetadata();
			Interlocked.Increment(ref _engineCount);
			return engine;
		}

		/// <summary>
		/// Gets an engine from the pool. This engine should be returned to the pool via
		/// <see cref="ReturnEngineToPool"/> when you are finished with it.
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
		public virtual IJsEngine GetEngine(TimeSpan? timeout = null)
		{
			IJsEngine engine;

			// First see if a pooled engine is immediately available
			if (_availableEngines.TryTake(out engine))
			{
				return TakeEngine(engine);
			}

			// If we're not at the limit, a new engine can be added immediately
			if (EngineCount < _config.MaxEngines)
			{
				lock (_engineCreationLock)
				{
					if (EngineCount < _config.MaxEngines)
					{
						return TakeEngine(CreateEngine());
					}
				}
			}

			// At the limit, so block until one is available
			if (!_availableEngines.TryTake(out engine, timeout ?? _config.GetEngineTimeout))
			{
				throw new JsPoolExhaustedException(string.Format(
					"Could not acquire JavaScript engine within {0}",
					_config.GetEngineTimeout
				));
			}
			return TakeEngine(engine);
		}

		/// <summary>
		/// Marks the specified engine as "in use"
		/// </summary>
		/// <param name="engine"></param>
		private IJsEngine TakeEngine(IJsEngine engine)
		{
			_metadata[engine].InUse = true;
			_metadata[engine].UsageCount++;
			return engine;
		}

		/// <summary>
		/// Returns an engine to the pool so it can be reused
		/// </summary>
		/// <param name="engine">Engine to return</param>
		public virtual void ReturnEngineToPool(IJsEngine engine)
		{
			if (!_metadata.ContainsKey(engine))
			{
				// This engine was from another pool. This could happen if a pool is recycled
				// and replaced with a different one (like what ReactJS.NET does when any 
				// loaded files change). Let's just pretend we never saw it.
				engine.Dispose();
				return;
			}

			_metadata[engine].InUse = false;
			var usageCount = _metadata[engine].UsageCount;
            if (_config.MaxUsagesPerEngine > 0 && usageCount >= _config.MaxUsagesPerEngine)
			{
				// Engine has been reused the maximum number of times, recycle it.
				DisposeEngine(engine);
				return;
			}

			if (
				_config.GarbageCollectionInterval > 0 && 
				usageCount % _config.GarbageCollectionInterval == 0 &&
				engine.SupportsGarbageCollection()
			)
			{
				engine.CollectGarbage();
			}

			_availableEngines.Add(engine);
		}

		/// <summary>
		/// Disposes the specified engine.
		/// </summary>
		/// <param name="engine">Engine to dispose</param>
		/// <param name="repopulateEngines">
		/// If <c>true</c>, a new engine will be created to replace the disposed engine
		/// </param>
		public virtual void DisposeEngine(IJsEngine engine, bool repopulateEngines = true)
		{
			engine.Dispose();
			_metadata.Remove(engine);
			Interlocked.Decrement(ref _engineCount);

			if (repopulateEngines)
			{
				// Ensure we still have at least the minimum number of engines.
				PopulateEngines();
			}
		}

		/// <summary>
		/// Disposes all engines in this pool. Note that this will only dispose the engines that 
		/// are *currently* available. Engines that are in use will be disposed when the user
		/// attempts to return them.
		/// </summary>
		protected virtual void DisposeAllEngines()
		{
			IJsEngine engine;
			while (_availableEngines.TryTake(out engine))
			{
				DisposeEngine(engine, repopulateEngines: false);
			}
			// Also clear out all metadata so engines that are currently in use while this disposal is 
			// happening get disposed on return.
			_metadata.Clear();
			_engineCount = 0;
		}

		/// <summary>
		/// Disposes all engines in this pool, and creates new engines in their place.
		/// </summary>
		public virtual void Recycle()
		{
			if (Recycled != null)
			{
				Recycled(this, null);
			}

			DisposeAllEngines();
			PopulateEngines();
		}

		/// <summary>
		/// Disposes all the JavaScript engines in this pool.
		/// </summary>
		public virtual void Dispose()
		{
			DisposeAllEngines();
			_cancellationTokenSource.Cancel();
			if (_fileWatcher != null)
			{
				_fileWatcher.Dispose();
			}
		}

		#region Statistics and debugging
		/// <summary>
		/// Gets the total number of engines in this engine pool, including engines that are
		/// currently busy.
		/// </summary>
		public virtual int EngineCount
		{
			get { return _engineCount; }
		}

		/// <summary>
		/// Gets the number of currently available engines in this engine pool.
		/// </summary>
		public virtual int AvailableEngineCount
		{
			get { return _availableEngines.Count; }
		}

		// ReSharper disable once UnusedMember.Local
		/// <summary>
		/// Gets a string for displaying this engine pool in the Visual Studio debugger.
		/// </summary>
		private string DebuggerDisplay
		{
			get
			{
				return string.Format(
					"Engines = {0}, Available = {1}, Max = {2}",
					EngineCount,
					AvailableEngineCount,
					_config.MaxEngines
				);
			}
		}
		#endregion
	}
}
