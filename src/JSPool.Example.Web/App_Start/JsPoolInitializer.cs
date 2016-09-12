/*
 * Copyright (c) 2014 Daniel Lo Nigro (Daniel15)
 * 
 * This source code is licensed under the BSD-style license found in the 
 * LICENSE file in the root directory of this source tree. 
 */

using System.Web;
using JavaScriptEngineSwitcher.Core;
using JavaScriptEngineSwitcher.V8;

namespace JSPool.Example.Web
{
	public class JsPoolInitializer
	{
		public static void Initialize()
		{
			// Configure JavaScriptEngineSwitcher
			JsEngineSwitcher.Instance.EngineFactories.AddV8();
			JsEngineSwitcher.Instance.DefaultEngineName = V8JsEngine.EngineName;

			// Ideally this would use an IoC container, but I'm just using HttpApplicationState
			// to keep the example simple.
			HttpContext.Current.Application["jspool"] = new JsPool(new JsPoolConfig
			{
				Initializer = engine =>
				{
					// In a real app you'd probably use ExecuteFile and ExecuteResource to load
					// libraries into the engine.
					engine.Execute(@"
						function helloWorld() {
							return 'Hello World!'
						}"
					);
				}
			});
		}
	}
}