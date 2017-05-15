/*
 * Copyright (c) 2016 Daniel Lo Nigro (Daniel15)
 * 
 * This source code is licensed under the BSD-style license found in the 
 * LICENSE file in the root directory of this source tree. 
 */

namespace JSPool
{
	/// <summary>
	/// Handles acquiring JavaScript engines from a shared pool. This class is thread safe.
	/// </summary>
	public interface IJsPool : IJsPool<PooledJsEngine>
	{
	}
}