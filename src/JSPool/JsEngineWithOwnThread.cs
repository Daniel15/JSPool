/*
 * Copyright (c) 2014 Daniel Lo Nigro (Daniel15)
 * 
 * This source code is licensed under the BSD-style license found in the 
 * LICENSE file in the root directory of this source tree. 
 */

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Threading;
using JavaScriptEngineSwitcher.Core;

namespace JSPool
{
	/// <summary>
	/// An adapter for <see cref="IJsEngine"/> implementations that always need to be used from
	/// the same thread they are created on. This class creates a thread for the engine and
	/// marshalls all method calls to that thread.
	/// </summary>
	public class JsEngineWithOwnThread : IJsEngineWithOwnThread
	{
		/// <summary>
		/// Represents a method being executed on the JsEngine thread.
		/// </summary>
		protected class ThreadWorkItem
		{
			/// <summary>
			/// Gets or sets the method to execute.
			/// </summary>
			public Func<IJsEngine, object> Method { get; set; }
			/// <summary>
			/// Gets or sets the exception that occurred while executing the method. If no 
			/// exception has occurred, this will be null.
			/// </summary>
			public Exception Exception { get; set; }
			/// <summary>
			/// Gets or sets the result of the method invocation.
			/// </summary>
			public object Result { get; set; }
			/// <summary>
			/// Event to signal when method execution has completed.
			/// </summary>
			public ManualResetEvent WaitForCompletion { get; set; }
		}

		/// <summary>
		/// Stack size to use for the threads.
		/// </summary>
		protected const int THREAD_STACK_SIZE = 2 * 1024 * 1024;

		/// <summary>
		/// Method used to create <see cref="IJsEngine"/> instances.
		/// </summary>
		protected readonly Func<IJsEngine> _innerEngineFactory;
		/// <summary>
		/// JsEngine used by this thread.
		/// </summary>
		protected IJsEngine _innerEngine;
		/// <summary>
		/// The thread the engine runs on.
		/// </summary>
		protected readonly Thread _thread;
		/// <summary>
		/// Token used to signal that the thread should be shut down.
		/// </summary>
		protected readonly CancellationToken _cancellationToken;
		/// <summary>
		/// Queue of method calls to run on the thread.
		/// </summary>
		protected readonly BlockingCollection<ThreadWorkItem> _queue = 
			new BlockingCollection<ThreadWorkItem>();

		/// <summary>
		/// Initializes a new instance of the <see cref="JsEngineWithOwnThread"/> class.
		/// </summary>
		/// <param name="innerEngineFactory">The engine factory.</param>
		/// <param name="cancellationToken">Token used to signal that the thread should be shut down.</param>
		public JsEngineWithOwnThread(Func<IJsEngine> innerEngineFactory, CancellationToken cancellationToken)
		{
			_innerEngineFactory = innerEngineFactory;
			_cancellationToken = cancellationToken;
			_thread = new Thread(RunThread, THREAD_STACK_SIZE)
			{
				Name = "JSPool Worker", 
				IsBackground = true,
			};
			_thread.Start();
		}

		/// <summary>
		/// Writes a log message for debugging purposes. Only logs when compiled with 
		/// TRACE flag.
		/// </summary>
		/// <param name="format">Format string</param>
		/// <param name="args">Arguments for string</param>
		[Conditional("TRACE")]
		protected void WriteLog(string format, params object[] args)
		{
			Trace.WriteLine(
				string.Format("[JSPool {0}] ", _thread.ManagedThreadId) +
				string.Format(format, args)
			);
		}

		/// <summary>
		/// Runs in the thread for this engine. Loops forever, processing items from the queue.
		/// </summary>
		private void RunThread()
		{
			WriteLog("Starting thread");
			_innerEngine = _innerEngineFactory();
			while (!_cancellationToken.IsCancellationRequested)
			{
				ThreadWorkItem item;
				try
				{
					item = _queue.Take(_cancellationToken);
				}
				catch (OperationCanceledException)
				{
					WriteLog("Received cancellation request");
					return;
				}

				WriteLog("Received call");
				try
				{
					item.Result = item.Method(_innerEngine);
				}
				catch (Exception ex)
				{
					item.Exception = ex;
				}
				item.WaitForCompletion.Set();
				WriteLog("Call completed");
			}
			WriteLog("Stopping thread");
		}

		/// <summary>
		/// Runs the specified method on the engine thread, and returns its result as an 
		/// <c>object</c>. Blocks until the method has been executed.
		/// </summary>
		/// <param name="method">Method to execute</param>
		/// <returns>Result of the method</returns>
		protected virtual object RunOnThread(Func<IJsEngine, object> method)
		{
			var item = new ThreadWorkItem
			{
				Method = method,
				WaitForCompletion = new ManualResetEvent(initialState: false),
			};
			_queue.Add(item, _cancellationToken);
			item.WaitForCompletion.WaitOne();
			if (item.Exception != null)
			{
				throw item.Exception;
			}
			return item.Result;	
		}

		/// <summary>
		/// Runs the specified method on the engine thread, and returns its result as an
		/// instance of <typeparamref name="T" />. Blocks until the method has been executed.
		/// </summary>
		/// <typeparam name="T">Type of data returned by the method</typeparam>
		/// <param name="method">Method to execute</param>
		/// <returns>Result of the method</returns>
		protected virtual T RunOnThread<T>(Func<IJsEngine, T> method)
		{
			return (T)RunOnThread(engine => (object)method(engine));
		}

		/// <summary>
		/// Runs the specified method on the engine thread. Blocks until the method has been 
		/// executed.
		/// </summary>
		/// <param name="method">Method to execute</param>
		protected virtual void RunOnThread(Action<IJsEngine> method)
		{
			RunOnThread(engine => { method(engine); return null;});
		}

		/// <summary>
		/// Gets a value indicating the execution status of this engine's thread.
		/// </summary>
		public bool IsThreadAlive
		{
			get { return _thread.IsAlive; }
		}

		#region Implementation of IJsEngine
		/// <summary>
		/// Gets a name of JavaScript engine
		/// </summary>
		public string Name { get { return _innerEngine.Name; } }

		/// <summary>
		/// Gets a version of original JavaScript engine
		/// </summary>
		public string Version { get { return _innerEngine.Version; } }

		/// <summary>
		/// Disposes the inner JavaScript engine
		/// </summary>
		public void Dispose()
		{
			RunOnThread(engine => engine.Dispose());
		}

		/// <summary>
		/// Evaluates an expression
		/// </summary>
		/// <param name="expression">JS-expression</param>
		/// <returns>Result of the expression</returns>
		public virtual object Evaluate(string expression)
		{
			return RunOnThread(engine => engine.Evaluate(expression));
		}

		/// <summary>
		/// Evaluates an expression
		/// </summary>
		/// <typeparam name="T">Type of result</typeparam>
		/// <param name="expression">JS-expression</param>
		/// <returns>Result of the expression</returns>
		public virtual T Evaluate<T>(string expression)
		{
			return RunOnThread(engine => engine.Evaluate<T>(expression));
		}

		/// <summary>
		/// Executes a code
		/// </summary>
		/// <param name="code">Code</param>
		public virtual void Execute(string code)
		{
			RunOnThread(engine => engine.Execute(code));
		}

		/// <summary>
		/// Executes a code from JS-file
		/// </summary>
		/// <param name="path">Path to the JS-file</param>
		/// <param name="encoding">Text encoding</param>
		public virtual void ExecuteFile(string path, Encoding encoding = null)
		{
			RunOnThread(engine => engine.ExecuteFile(path, encoding));
		}

		/// <summary>
		/// Executes a code from embedded JS-resource
		/// </summary>
		/// <param name="resourceName">JS-resource name</param>
		/// <param name="type">Type from assembly that containing an embedded resource</param>
		public virtual void ExecuteResource(string resourceName, Type type)
		{
			RunOnThread(engine => engine.ExecuteResource(resourceName, type));
		}

		/// <summary>
		/// Executes a code from embedded JS-resource
		/// </summary>
		/// <param name="resourceName">JS-resource name</param>
		/// <param name="assembly">Assembly that containing an embedded resource</param>
		public virtual void ExecuteResource(string resourceName, Assembly assembly)
		{
			RunOnThread(engine => engine.ExecuteResource(resourceName, assembly));
		}

		/// <summary>
		/// Calls a function
		/// </summary>
		/// <param name="functionName">Function name</param>
		/// <param name="args">Function arguments</param>
		/// <returns>Result of the function execution</returns>
		public virtual object CallFunction(string functionName, params object[] args)
		{
			return RunOnThread(engine => engine.CallFunction(functionName, args));
		}

		/// <summary>
		/// Calls a function
		/// </summary>
		/// <typeparam name="T">Type of function result</typeparam>
		/// <param name="functionName">Function name</param>
		/// <param name="args">Function arguments</param>
		/// <returns>Result of the function execution</returns>
		public virtual T CallFunction<T>(string functionName, params object[] args)
		{
			return RunOnThread(engine => engine.CallFunction<T>(functionName, args));
		}

		/// <summary>
		/// Сhecks for the existence of a variable
		/// </summary>
		/// <param name="variableName">Variable name</param>
		/// <returns>Result of check (true - exists; false - not exists</returns>
		public virtual bool HasVariable(string variableName)
		{
			return RunOnThread(engine => engine.HasVariable(variableName));
		}

		/// <summary>
		/// Gets a value of variable
		/// </summary>
		/// <param name="variableName">Variable name</param>
		/// <returns>Value of variable</returns>
		public virtual object GetVariableValue(string variableName)
		{
			return RunOnThread(engine => engine.GetVariableValue(variableName));
		}

		/// <summary>
		/// Gets a value of variable
		/// </summary>
		/// <typeparam name="T">Type of variable</typeparam>
		/// <param name="variableName">Variable name</param>
		/// <returns>Value of variable</returns>
		public virtual T GetVariableValue<T>(string variableName)
		{
			return RunOnThread(engine => engine.GetVariableValue<T>(variableName));
		}

		/// <summary>
		/// Sets a value of variable
		/// </summary>
		/// <param name="variableName">Variable name</param>
		/// <param name="value">Value of variable</param>
		public virtual void SetVariableValue(string variableName, object value)
		{
			RunOnThread(engine => engine.SetVariableValue(variableName, value));
		}

		/// <summary>
		/// Removes a variable
		/// </summary>
		/// <param name="variableName">Variable name</param>
		public virtual void RemoveVariable(string variableName)
		{
			RunOnThread(engine => engine.RemoveVariable(variableName));
		}
		#endregion
	}
}
