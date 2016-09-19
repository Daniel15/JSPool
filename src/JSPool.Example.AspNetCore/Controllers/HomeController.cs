/*
 * Copyright (c) 2016 Daniel Lo Nigro (Daniel15)
 * 
 * This source code is licensed under the BSD-style license found in the 
 * LICENSE file in the root directory of this source tree. 
 */

using System;
using JSPool;
using JSPool.Example.AspNetCore.Models;
using Microsoft.AspNetCore.Mvc;

namespace JSPool.Example.AspNetCore.Controllers
{
	public class HomeController : Controller
	{
		private readonly IJsPool _jsPool;

		public HomeController(IJsPool jsPool)
		{
			_jsPool = jsPool;
		}

		public ActionResult Index()
		{
			return View(new HomeViewModel
			{
				AvailableEngineCount = _jsPool.AvailableEngineCount,
				EngineCount = _jsPool.EngineCount,
			});
		}

		public ActionResult HelloWorld()
		{
			var engine = _jsPool.GetEngine();
			// This function is created in JsPoolInitializer.cs
			var result = engine.CallFunction<string>("helloWorld");
			// Always release an engine when you're done with it.
			_jsPool.ReturnEngineToPool(engine);

			return Content(result);
		}

		public ActionResult Loop()
		{
			var engine = _jsPool.GetEngine();
			engine.Execute(@"
				var timeToEnd = Date.now() + 10000;
				while (Date.now() < timeToEnd) { }
			");
			// Always release an engine when you're done with it.
			_jsPool.ReturnEngineToPool(engine);

			return Content("Done " + DateTime.Now);
		}
	}
}