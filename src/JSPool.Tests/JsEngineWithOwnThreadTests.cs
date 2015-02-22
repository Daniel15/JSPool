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
	class JsEngineWithOwnThreadTests
	{
		[Test]
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
			Assert.AreEqual(true, engine.IsThreadAlive);
			// Engine was created on a different thread
			Assert.AreNotEqual(Thread.CurrentThread.ManagedThreadId, threadEngineWasCreatedOn);

			engine.Execute("alert(hello)");
			engine.Execute("alert(world)");
			// Execute was called twice
			innerEngine.Verify(x => x.Execute(It.IsAny<string>()), Times.Exactly(2));
			// Both calls ran on same thread
			Assert.AreEqual(threadsExecuteWasCalledFrom[0], threadsExecuteWasCalledFrom[1]);
			// Both calls ran on the thread the engine was created on
			Assert.AreEqual(threadEngineWasCreatedOn, threadsExecuteWasCalledFrom[0]);	
		}

		[Test]
		public void HandlesCallFunctionThatReturnsGeneric()
		{
			var innerEngine = new Mock<IJsEngine>();
			innerEngine.Setup(x => x.CallFunction<int>("add", 40, 2)).Returns(42);
			var factory = new Mock<IEngineFactoryForMock>();
			factory.Setup(x => x.EngineFactory()).Returns(innerEngine.Object);

			var engine = new JsEngineWithOwnThread(factory.Object.EngineFactory, new CancellationToken());
			var result = engine.CallFunction<int>("add", 40, 2);

			Assert.AreEqual(42, result);
		}

		[Test]
		public void HandlesCallFunctionThatReturnsObject()
		{
			var innerEngine = new Mock<IJsEngine>();
			innerEngine.Setup(x => x.CallFunction("hello")).Returns("Hello World");
			var factory = new Mock<IEngineFactoryForMock>();
			factory.Setup(x => x.EngineFactory()).Returns(innerEngine.Object);

			var engine = new JsEngineWithOwnThread(factory.Object.EngineFactory, new CancellationToken());
			var result = engine.CallFunction("hello");

			Assert.AreEqual("Hello World", result);
		}

		[Test]
		public void HandlesDisposal()
		{
			var innerEngine = new Mock<IJsEngine>();
			var factory = new Mock<IEngineFactoryForMock>();
			factory.Setup(x => x.EngineFactory()).Returns(innerEngine.Object);

			var engine = new JsEngineWithOwnThread(factory.Object.EngineFactory, new CancellationToken());
			engine.Dispose();

			innerEngine.Verify(x => x.Dispose());
			Assert.IsFalse(engine.IsThreadAlive);
		}
	}
}
