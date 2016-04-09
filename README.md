JSPool
======

JSPool facilitates easy integration of JavaScript scripting into a .NET 
application in a scalable and performant manner. It does so by creating a pool
of engines that can be reused multiple times. This makes it easy to use 
JavaScript libraries that have long initialisation times without having to 
create a new engine and load the script again every time.

It is powered by the [JavaScriptEngineSwitcher](https://github.com/Taritsyn/JavaScriptEngineSwitcher)
library.

Features
========
 - Supports any JavaScript engine supported by JavaScriptEngineSwitcher 
   (including V8, MSIE, Jint, and Jurassic).
 - Pools are thread-safe.
 - Automatically calls an initialisation callback when engines are created. This
   can be used to load JavaScript libraries.
 - Pools have a configurable number of maximum engines.
 - There is no global state; pools are instantiated so you can have multiple 
   pools in your application, each with their own configuration.

Usage
=====

Installation can be done either through NuGet (`Install-Package JSPool`) or by 
cloning the Git repository and running `dev-build.bat`. In addition to JSPool, 
you will need to have at least one engine supported by JavaScriptEngineSwitcher
installed (eg. [V8](https://www.nuget.org/packages/JavaScriptEngineSwitcher.V8) 
or [MSIE](http://www.nuget.org/packages/JavaScriptEngineSwitcher.Msie)). V8 is
recommended.

```csharp
var pool = new JsPool(new JsPoolConfig
{
  Initializer = initEngine =>
  {
    // This initializer will be called whenever a new engine is created. In a 
    // real app you'd commonly use ExecuteFile and ExecuteResource to load
    // libraries into the engine.
    initEngine.Execute(@"
      function sayHello(name) {
        return 'Hello ' + name + '!';
      }
    ");
  }
});

// Get an engine from the pool.
var engine = pool.GetEngine();
var message = engine.CallFunction<string>("sayHello", "Daniel");
Console.WriteLine(message); // "Hello Daniel!"

// Always release an engine when you're done with it. This adds the engine back
// into the pool so it can be used again.
pool.ReturnEngineToPool(engine);

// Disposing the pool will also dispose all its engines. Always dispose the pool
// when it is no longer required.
pool.Dispose();
```

Configuration
=============

The following configuration settings are available for JSPool:

 - **StartEngines**: Number of engines to initially start when a pool is 
   created. Defaults to 10.
 - **MaxEngines**: Maximum number of engines that will be created in the pool. 
   If an engine is requested but all the current engines are busy, a new engine 
   will be created unless the maximum has been reached. Defaults to 25.
 - **Initializer**: Action called when a new engine is created. This should 
   configure the environment and load any required JavaScript libraries. The 
   engine will not be available for use until this method has completed.
 - **MaxUsagesPerEngine**: The maximum number of times an engine can be reused
   before it is disposed. 0 is unlimited. Defaults to 100.
 - **GarbageCollectionInterval**: The number of times an engine can be reused
   before its garbage collector is ran. Only affects engines that support 
   garbage collection (V8). Defaults to 20.
 - **GetEngineTimeout**: If all engines in the pool are currently busy and 
   *MaxEngines* has been reached, the call to `GetEngine` will block for this 
   period of time waiting for an engine to become free. If an engine can not be 
   acquired in this timeframe, throws a `JsPoolExhaustedException`. Set this to
   -1 to wait indefinitely. Defaults to 5 seconds.
 - **WatchPath**: Path to watch for file changes. If any files in this path
   change, all engines in the pool will be recycled.
 - **WatchFiles**: Used in combination with WatchPath. If specified, only these
   particular files within the path will be watched. If not specified, all files
   within the path will be watched.
 - **EngineFactory**: Method used to create new JavaScript engines. Defaults to 
   the default factory method in JavaScriptEngineSwitcher
   (`JsEngineSwitcher.Current.CreateDefaultJsEngineInstance`)


Changelog
=========
0.3.2 - 9th April 2016
----------------------
 - [#13](https://github.com/Daniel15/JSPool/issues/13) - Fix concurrency
   issue

0.3.1 - 15th November 2015
--------------------------
 - [#8](https://github.com/Daniel15/JSPool/issues/8) - Do not throw if 
   `WatchPath` is used without `WatchFiles`
 - Fire `Recycled` event when pool is recycled

0.3 - 12th April 2015
---------------------
 - Added `Recycle` method to dispose all current engines and create new ones. 
   This is essentially the same as disposing the whole pool and creating a new
   pool
 - Added the ability to recycle the pool when any watched files are modified

0.2 - 21st February 2015
------------------------
 - [#2](https://github.com/Daniel15/JSPool/issues/2) - Collect garbage when
   engine returned to pool
 - [#3](https://github.com/Daniel15/JSPool/issues/3) - Recycle workers after a
   certain number of usages
 - [#4](https://github.com/Daniel15/JSPool/issues/4) and
   [#5](https://github.com/Daniel15/JSPool/issues/5) - Upgrade 
   JavaScriptEngineSwitcher to latest version to allow settings such as max heap
   size in Web.config or App.config

0.1 - 28th November 2014
------------------------
 - Initial release
