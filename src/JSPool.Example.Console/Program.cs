/*
 * Copyright (c) 2014 Daniel Lo Nigro (Daniel15)
 * 
 * This source code is licensed under the BSD-style license found in the 
 * LICENSE file in the root directory of this source tree. 
 */

using System;
using JavaScriptEngineSwitcher.Core;
using JavaScriptEngineSwitcher.V8;

namespace JSPool.Example.ConsoleApp
{
	public static class Program
	{
		static void Main(string[] args)
		{
			// Configure JavaScriptEngineSwitcher
			JsEngineSwitcher.Instance.EngineFactories.AddV8();
			JsEngineSwitcher.Instance.DefaultEngineName = V8JsEngine.EngineName;

			var pool = new JsPool(new JsPoolConfig
			{
				Initializer = initEngine =>
				{
					// In a real app you'd probably use ExecuteFile and ExecuteResource to load
					// libraries into the engine.
					initEngine.Execute(@"
						function sayHello(name) {
							return 'Hello ' + name + '!';
						}
					");
				}
			});

			// Get an engine from the pool
			var engine = pool.GetEngine();
			var message = engine.CallFunction<string>("sayHello", "Daniel");
			Console.WriteLine(message); // "Hello Daniel!"
			Console.ReadKey();

			// Always release an engine when you're done with it.
			pool.ReturnEngineToPool(engine);

			// Disposing the pool will also dispose all its engines. Always dispose it when
			// it is no longer required.
			pool.Dispose();
		}
	}
}
