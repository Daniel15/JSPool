/*
 * Copyright (c) 2014 Daniel Lo Nigro (Daniel15)
 * 
 * This source code is licensed under the BSD-style license found in the 
 * LICENSE file in the root directory of this source tree. 
 */

using System;
using System.IO;
using JavaScriptEngineSwitcher.Core;
using JSPool.Exceptions;
using Moq;
using Xunit;

// Some tests are for deprecated methods
#pragma warning disable 618

namespace JSPool.Tests
{
	public class JsPoolTests
	{
		[Fact]
		public void ConstructorCreatesEngines()
		{
			var factory = new Mock<IEngineFactoryForMock>();
			factory.Setup(x => x.EngineFactory()).Returns(() => new Mock<IJsEngine>().Object);
			var config = new JsPoolConfig
			{
				StartEngines = 5,
				EngineFactory = factory.Object.EngineFactory
			};

			var pool = new JsPool(config);
			Assert.Equal(5, pool.AvailableEngineCount);
			factory.Verify(x => x.EngineFactory(), Times.Exactly(5));
		}

		[Fact]
		public void GetEngineReturnsAllAvailableEngines()
		{
			var engines = new[]
			{
				new Mock<IJsEngine>().Object,
				new Mock<IJsEngine>().Object,
				new Mock<IJsEngine>().Object,
			};
			var factory = new Mock<IEngineFactoryForMock>();
			factory.SetupSequence(x => x.EngineFactory())
				.Returns(engines[0])
				.Returns(engines[1])
				.Returns(engines[2]);
			var config = new JsPoolConfig
			{
				StartEngines = 3,
				EngineFactory = factory.Object.EngineFactory
			};

			var pool = new JsPool(config);
			var resultEngines = new[]
			{
				pool.GetEngine().InnerEngine,
				pool.GetEngine().InnerEngine,
				pool.GetEngine().InnerEngine,
			};

			Assert.Equal(engines, resultEngines);
		}

		[Fact]
		public void GetEngineCreatesNewEngineIfNotAtMaximum()
		{
			var factory = new Mock<IEngineFactoryForMock>();
			factory.Setup(x => x.EngineFactory()).Returns(() => new Mock<IJsEngine>().Object);
			var config = new JsPoolConfig
			{
				StartEngines = 1,
				MaxEngines = 2,
				EngineFactory = factory.Object.EngineFactory
			};

			var pool = new JsPool(config);
			factory.Verify(x => x.EngineFactory(), Times.Exactly(1));
			pool.GetEngine(); // First engine created on init
			factory.Verify(x => x.EngineFactory(), Times.Exactly(1));
			Assert.Equal(1, pool.EngineCount);
			Assert.Equal(0, pool.AvailableEngineCount);

			pool.GetEngine(); // Second engine created JIT
			factory.Verify(x => x.EngineFactory(), Times.Exactly(2));
			Assert.Equal(2, pool.EngineCount);
			Assert.Equal(0, pool.AvailableEngineCount);
		}

		[Fact]
		public void GetEngineFailsIfAtMaximum()
		{
			var factory = new Mock<IEngineFactoryForMock>();
			factory.Setup(x => x.EngineFactory()).Returns(() => new Mock<IJsEngine>().Object);
			var config = new JsPoolConfig
			{
				StartEngines = 1,
				MaxEngines = 1,
				EngineFactory = factory.Object.EngineFactory
			};

			var pool = new JsPool(config);
			factory.Verify(x => x.EngineFactory(), Times.Exactly(1));
			pool.GetEngine(); // First engine created on init

			Assert.Throws<JsPoolExhaustedException>(() =>
				pool.GetEngine(TimeSpan.Zero)
			);
		}

		[Fact]
		public void LegacyReturnEngineToPoolAddsToAvailableEngines()
		{
			var factory = new Mock<IEngineFactoryForMock>();
			factory.Setup(x => x.EngineFactory()).Returns(() => new Mock<IJsEngine>().Object);
			var config = new JsPoolConfig
			{
				StartEngines = 2,
				EngineFactory = factory.Object.EngineFactory
			};

			var pool = new JsPool(config);
			Assert.Equal(2, pool.AvailableEngineCount);
			var engine = pool.GetEngine();
			Assert.Equal(1, pool.AvailableEngineCount);
			pool.ReturnEngineToPool(engine);
			Assert.Equal(2, pool.AvailableEngineCount);
		}

		[Fact]
		public void ReturnEngineAddsToAvailableEngines()
		{
			var factory = new Mock<IEngineFactoryForMock>();
			factory.Setup(x => x.EngineFactory()).Returns(() => new Mock<IJsEngine>().Object);
			var config = new JsPoolConfig
			{
				StartEngines = 2,
				EngineFactory = factory.Object.EngineFactory
			};

			var pool = new JsPool(config);
			Assert.Equal(2, pool.AvailableEngineCount);
			using (var engine = pool.GetEngine())
			{
				Assert.Equal(1, pool.AvailableEngineCount);
			}
			Assert.Equal(2, pool.AvailableEngineCount);
		}

		[Fact]
		public void LegacyReturnEngineDisposesIfAtMaxUsages()
		{
			var mockEngine1 = new Mock<IJsEngine>();
			var mockEngine2 = new Mock<IJsEngine>();
			var factory = new Mock<IEngineFactoryForMock>();
			factory.SetupSequence(x => x.EngineFactory())
				.Returns(mockEngine1.Object)
				.Returns(mockEngine2.Object);
			var config = new JsPoolConfig
			{
				StartEngines = 1,
				MaxUsagesPerEngine = 3,
				EngineFactory = factory.Object.EngineFactory
			};

			var pool = new JsPool(config);

			// First two usages should not recycle it
			var engine = pool.GetEngine();
			Assert.Equal(mockEngine1.Object, engine.InnerEngine);
			pool.ReturnEngineToPool(engine);
			mockEngine1.Verify(x => x.Dispose(), Times.Never);

			engine = pool.GetEngine();
			Assert.Equal(mockEngine1.Object, engine.InnerEngine);
			pool.ReturnEngineToPool(engine);
			mockEngine1.Verify(x => x.Dispose(), Times.Never);

			// Third usage should recycle it, since the max usages is 3
			engine = pool.GetEngine();
			pool.ReturnEngineToPool(engine);
			mockEngine1.Verify(x => x.Dispose());

			// Next usage should get a new engine
			engine = pool.GetEngine();
			Assert.Equal(mockEngine2.Object, engine.InnerEngine);
		}

		[Fact]
		public void ReturnEngineDisposesIfAtMaxUsages()
		{
			var mockEngine1 = new Mock<IJsEngine>();
			var mockEngine2 = new Mock<IJsEngine>();
			var factory = new Mock<IEngineFactoryForMock>();
			factory.SetupSequence(x => x.EngineFactory())
				.Returns(mockEngine1.Object)
				.Returns(mockEngine2.Object);
			var config = new JsPoolConfig
			{
				StartEngines = 1,
				MaxUsagesPerEngine = 3,
				EngineFactory = factory.Object.EngineFactory
			};

			var pool = new JsPool(config);

			// First two usages should not recycle it
			using (var engine = pool.GetEngine())
			{
				Assert.Equal(mockEngine1.Object, engine.InnerEngine);
			}
			mockEngine1.Verify(x => x.Dispose(), Times.Never);

			using (var engine = pool.GetEngine())
			{
				Assert.Equal(mockEngine1.Object, engine.InnerEngine);
			}
			mockEngine1.Verify(x => x.Dispose(), Times.Never);

			// Third usage should recycle it, since the max usages is 3
			using (var engine = pool.GetEngine())
			{
				Assert.Equal(mockEngine1.Object, engine.InnerEngine);
			}
			mockEngine1.Verify(x => x.Dispose());

			// Next usage should get a new engine
			using (var engine = pool.GetEngine())
			{
				Assert.Equal(mockEngine2.Object, engine.InnerEngine);
			}
		}

		[Fact]
		public void DisposeDisposesAllEngines()
		{
			var engines = new[]
			{
				new Mock<IJsEngine>(),
				new Mock<IJsEngine>(),
				new Mock<IJsEngine>(),
			};
			var factory = new Mock<IEngineFactoryForMock>();
			factory.SetupSequence(x => x.EngineFactory())
				.Returns(engines[0].Object)
				.Returns(engines[1].Object)
				.Returns(engines[2].Object);
			var config = new JsPoolConfig
			{
				StartEngines = 3,
				EngineFactory = factory.Object.EngineFactory
			};

			var pool = new JsPool(config);
			pool.Dispose();

			foreach (var engine in engines)
			{
				engine.Verify(x => x.Dispose());
			}
		}

		[Fact]
		public void ShouldIgnoreReturnToPoolIfUnknownEngine()
		{
			var factory = new Mock<IEngineFactoryForMock>();
			factory.Setup(x => x.EngineFactory()).Returns(() => new Mock<IJsEngine>().Object);
			var config = new JsPoolConfig
			{
				StartEngines = 1,
				EngineFactory = factory.Object.EngineFactory
			};
			var rogueEngine = new Mock<PooledJsEngine>();
			rogueEngine.Setup(x => x.InnerEngine.Dispose());

			var pool = new JsPool(config);
			pool.ReturnEngineToPool(rogueEngine.Object);

			Assert.Equal(1, pool.AvailableEngineCount);
			Assert.Equal(1, pool.EngineCount);
			rogueEngine.Verify(x => x.InnerEngine.Dispose());
		}

		[Fact]
		public void RecycleCreatesNewEngines()
		{
			var factory = new Mock<IEngineFactoryForMock>();
			factory.Setup(x => x.EngineFactory()).Returns(() => new Mock<IJsEngine>().Object);
			var config = new JsPoolConfig
			{
				StartEngines = 2,
				EngineFactory = factory.Object.EngineFactory
			};

			var pool = new JsPool(config);
			Assert.Equal(2, pool.AvailableEngineCount);
			factory.Verify(x => x.EngineFactory(), Times.Exactly(2));

			// Now recycle the pool
			pool.Recycle();
			Assert.Equal(2, pool.AvailableEngineCount);
			// Two more engines should have been created
			factory.Verify(x => x.EngineFactory(), Times.Exactly(4));
		}


		[Fact]
		public void RecycleFiresRecycledEvent()
		{
			var callCount = 0;
			var factory = new Mock<IEngineFactoryForMock>();
			factory.Setup(x => x.EngineFactory()).Returns(() => new Mock<IJsEngine>().Object);
			var config = new JsPoolConfig
			{
				StartEngines = 2,
				EngineFactory = factory.Object.EngineFactory
			};

			var pool = new JsPool(config);
			pool.Recycled += (sender, args) => callCount++;
			Assert.Equal(0, callCount);

			pool.Recycle();
			Assert.Equal(1, callCount);

			pool.Recycle();
			Assert.Equal(2, callCount);
		}

		[Fact]
		public void WatchPathWithoutWatchFilesDoesNotThrow()
		{
			var factory = new Mock<IEngineFactoryForMock>();
			factory.Setup(x => x.EngineFactory()).Returns(() => new Mock<IJsEngine>().Object);
			var config = new JsPoolConfig
			{
				StartEngines = 2,
				EngineFactory = factory.Object.EngineFactory,
				WatchPath = Directory.GetCurrentDirectory(),
			};

			// Should not throw
			// ReSharper disable once UnusedVariable
			var pool = new JsPool(config);
		}
	}

	public interface IEngineFactoryForMock
	{
		IJsEngine EngineFactory();
	}
}
