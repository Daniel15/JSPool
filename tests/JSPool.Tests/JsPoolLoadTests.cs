/*
 * Copyright (c) 2014 Daniel Lo Nigro (Daniel15)
 * 
 * This source code is licensed under the BSD-style license found in the 
 * LICENSE file in the root directory of this source tree. 
 */

using System.Collections.Generic;
using System.Threading;
using JavaScriptEngineSwitcher.Core;
using Moq;
using NUnit.Framework;

namespace JSPool.Tests
{
	[TestFixture]
	public class JsPoolLoadTests
	{
		[Test]
		public void ConcurrentGetAndReleaseEnginesIsSafe()
		{
			const int ConcurrentThreadCount = 100;
			var config = new JsPoolConfig
			{
				StartEngines = 0,
				MaxEngines = ConcurrentThreadCount,
				EngineFactory = () => new Mock<IJsEngine>().Object
			};

			var pool = new JsPool(config);
			ThreadStart getReleaseEngine = () =>
			{
				for (var i = 0; i < 10000; ++i)
				{
					IJsEngine engine = pool.GetEngine();
					pool.ReturnEngineToPool(engine);
				}
			};


			IList<Thread> threads = new List<Thread>();
			for (var i = 0; i < ConcurrentThreadCount; ++i)
			{
				threads.Add(new Thread(getReleaseEngine));
			}

			foreach (var thread in threads)
			{
				thread.Start();
			}

			threads[0].Join(10000);
			for (var i = 1; i < threads.Count; ++i)
			{
				Thread thread = threads[i];
				thread.Join(100);
			}

			Assert.AreEqual(0, pool.EngineCount);
		}
	}
}
