/*
 * Copyright (c) 2017 Daniel Lo Nigro (Daniel15)
 * 
 * This source code is licensed under the BSD-style license found in the 
 * LICENSE file in the root directory of this source tree. 
 */

using System;

namespace JSPool
{
    /// <summary>
    /// Wraps an object stored in a pool, overriding the behaviour of <see cref="Dispose"/> so that
    /// it returns the engine to the pool rather than actually disposing the engine.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class PooledObject<T> : IDisposable
    {
        /// <summary>
        /// Engine being wrapped by this pool
        /// </summary>
        public virtual T InnerEngine { get; internal set; }

        /// <summary>
        /// Callback for returning the engine to the pool
        /// </summary>
        internal Action ReturnEngineToPool { private get; set; }

        /// <summary>
        /// Increase engine usage count by one.
        /// </summary>
        internal void IncreaseUsageCount()
        {
            UsageCount++;
        }

        /// <summary>
        /// Gets the number of times this engine has been used.
        /// </summary>
        public int UsageCount { get; private set; } = 0;

        /// <summary>
        /// Returns this engine to the pool.
        /// </summary>
        public virtual void Dispose()
		{
			ReturnEngineToPool();
		}
	}
}
