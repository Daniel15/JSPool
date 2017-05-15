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
using Xunit;

namespace JSPool.Tests
{
	public class JsEngineWithOwnThreadTests
	{
		[Fact]
		public void ExecutesCodeOnCorrectThread()
		{
			int? threadEngineWasCreatedOn = null;
			var threadsExecuteWasCalledFrom = new List<int>();

			var innerEngine = new Mock<IJsEngine>();
			innerEngine.Setup(x => x.Execute(It.IsAny<string>()))
				.Callback(() =>
					threadsExecuteWasCalledFrom.Add(Thread.CurrentThread.ManagedThreadId)
				);
			var factory = new Mock<IEngineFactoryForMock>();
			factory.Setup(x => x.EngineFactory())
				.Returns(innerEngine.Object)
				.Callback(() => threadEngineWasCreatedOn = Thread.CurrentThread.ManagedThreadId);

			var engine = new JsEngineWithOwnThread(factory.Object.EngineFactory, new CancellationToken());
			Assert.True(engine.IsThreadAlive);
			// Engine was created on a different thread
			Assert.NotEqual(Thread.CurrentThread.ManagedThreadId, threadEngineWasCreatedOn);

			engine.Execute("alert(hello)");
			engine.Execute("alert(world)");
			// Execute was called twice
			innerEngine.Verify(x => x.Execute(It.IsAny<string>()), Times.Exactly(2));
			// Both calls ran on same thread
			Assert.Equal(threadsExecuteWasCalledFrom[0], threadsExecuteWasCalledFrom[1]);
			// Both calls ran on the thread the engine was created on
			Assert.Equal(threadEngineWasCreatedOn, threadsExecuteWasCalledFrom[0]);	
		}

		[Fact]
		public void HandlesCallFunctionThatReturnsGeneric()
		{
			var innerEngine = new Mock<IJsEngine>();
			innerEngine.Setup(x => x.CallFunction<int>("add", 40, 2)).Returns(42);
			var factory = new Mock<IEngineFactoryForMock>();
			factory.Setup(x => x.EngineFactory()).Returns(innerEngine.Object);

			var engine = new JsEngineWithOwnThread(factory.Object.EngineFactory, new CancellationToken());
			var result = engine.CallFunction<int>("add", 40, 2);

			Assert.Equal(42, result);
		}

		[Fact]
		public void HandlesCallFunctionThatReturnsObject()
		{
			var innerEngine = new Mock<IJsEngine>();
			innerEngine.Setup(x => x.CallFunction("hello")).Returns("Hello World");
			var factory = new Mock<IEngineFactoryForMock>();
			factory.Setup(x => x.EngineFactory()).Returns(innerEngine.Object);

			var engine = new JsEngineWithOwnThread(factory.Object.EngineFactory, new CancellationToken());
			var result = engine.CallFunction("hello");

			Assert.Equal("Hello World", result);
		}

		[Fact]
		public void HandlesDisposal()
		{
			var innerEngine = new Mock<IJsEngine>();
			var factory = new Mock<IEngineFactoryForMock>();
			factory.Setup(x => x.EngineFactory()).Returns(innerEngine.Object);

			var engine = new JsEngineWithOwnThread(factory.Object.EngineFactory, new CancellationToken());
			engine.Dispose();

			innerEngine.Verify(x => x.Dispose());
			Thread.Sleep(50);
			Assert.False(engine.IsThreadAlive);
		}
	}
}
