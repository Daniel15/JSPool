/*
 * Copyright (c) 2017 Daniel Lo Nigro (Daniel15)
 * 
 * This source code is licensed under the BSD-style license found in the 
 * LICENSE file in the root directory of this source tree. 
 */

using System;
using System.Reflection;
using System.Text;
using JavaScriptEngineSwitcher.Core;

namespace JSPool
{
	/// <summary>
	/// An <see cref="IJsEngine"/> that has come from a pool of engines. When this engine is disposed,
	/// it will be returned to the pool.
	/// </summary>
	public class PooledJsEngine : PooledObject<IJsEngine>, IJsEngine
	{
		#region IJsEngine implementation
		/// <summary>
		/// Gets the name of this JS engine
		/// </summary>
		public string Name => InnerEngine.Name;

		/// <summary>
		/// Gets the version of this JS engine
		/// </summary>
		public string Version => InnerEngine.Version;

		/// <summary>
		/// Determines if this engine supports script pre-compilation
		/// </summary>
		public bool SupportsScriptPrecompilation => InnerEngine.SupportsScriptPrecompilation;

		/// <summary>
		/// Determines if this engine supports script interruption
		/// </summary>
		public bool SupportsScriptInterruption => InnerEngine.SupportsScriptInterruption;

		/// <summary>
		/// Determines if this engine supports garbage collection
		/// </summary>
		public bool SupportsGarbageCollection => InnerEngine.SupportsGarbageCollection;

		/// <summary>
		/// Creates a pre-compiled script from JS code
		/// </summary>
		/// <param name="code">JS code</param>
		/// <returns>A pre-compiled script that can be executed by different instances of JS engine</returns>
		public virtual IPrecompiledScript Precompile(string code)
		{
			return InnerEngine.Precompile(code);
		}

		/// <summary>
		/// Creates a pre-compiled script from JS code
		/// </summary>
		/// <param name="code">JS code</param>
		/// <param name="documentName">Document name</param>
		/// <returns>A pre-compiled script that can be executed by different instances of JS engine</returns>
		public virtual IPrecompiledScript Precompile(string code, string documentName)
		{
			return InnerEngine.Precompile(code, documentName);
		}

		/// <summary>
		/// Creates a pre-compiled script from JS file
		/// </summary>
		/// <param name="path">Path to the JS file</param>
		/// <param name="encoding">Text encoding</param>
		/// <returns>A pre-compiled script that can be executed by different instances of JS engine</returns>
		public virtual IPrecompiledScript PrecompileFile(string path, Encoding encoding = null)
		{
			return InnerEngine.PrecompileFile(path, encoding);
		}

		/// <summary>
		/// Creates a pre-compiled script from embedded JS resource
		/// </summary>
		/// <param name="resourceName">The case-sensitive resource name without the namespace of the specified type</param>
		/// <param name="type">The type, that determines the assembly and whose namespace is used to scope
		/// the resource name</param>
		/// <returns>A pre-compiled script that can be executed by different instances of JS engine</returns>
		public virtual IPrecompiledScript PrecompileResource(string resourceName, Type type)
		{
			return InnerEngine.PrecompileResource(resourceName, type);
		}

		/// <summary>
		/// Creates a pre-compiled script from embedded JS resource
		/// </summary>
		/// <param name="resourceName">The case-sensitive resource name</param>
		/// <param name="assembly">The assembly, which contains the embedded resource</param>
		/// <returns>A pre-compiled script that can be executed by different instances of JS engine</returns>
		public virtual IPrecompiledScript PrecompileResource(string resourceName, Assembly assembly)
		{
			return InnerEngine.PrecompileResource(resourceName, assembly);
		}

		/// <summary>
		/// Evaluates an expression
		/// </summary>
		/// <param name="expression">JS expression</param>
		/// <returns>Result of the expression</returns>
		public virtual object Evaluate(string expression)
		{
			return InnerEngine.Evaluate(expression);
		}

		/// <summary>
		/// Evaluates an expression
		/// </summary>
		/// <param name="expression">JS expression</param>
		/// <param name="documentName">Document name</param>
		/// <returns>Result of the expression</returns>
		public virtual object Evaluate(string expression, string documentName)
		{
			return InnerEngine.Evaluate(expression, documentName);
		}

		/// <summary>
		/// Evaluates an expression
		/// </summary>
		/// <typeparam name="T">Type of result</typeparam>
		/// <param name="expression">JS expression</param>
		/// <returns>Result of the expression</returns>
		public virtual T Evaluate<T>(string expression)
		{
			return InnerEngine.Evaluate<T>(expression);
		}

		/// <summary>
		/// Evaluates an expression
		/// </summary>
		/// <typeparam name="T">Type of result</typeparam>
		/// <param name="expression">JS expression</param>
		/// <param name="documentName">Document name</param>
		/// <returns>Result of the expression</returns>
		public virtual T Evaluate<T>(string expression, string documentName)
		{
			return InnerEngine.Evaluate<T>(expression, documentName);
		}

		/// <summary>
		/// Executes a code
		/// </summary>
		/// <param name="code">JS code</param>
		public virtual void Execute(string code)
		{
			InnerEngine.Execute(code);
		}

		/// <summary>
		/// Executes a code
		/// </summary>
		/// <param name="code">JS code</param>
		/// <param name="documentName">Document name</param>
		public virtual void Execute(string code, string documentName)
		{
			InnerEngine.Execute(code, documentName);
		}

		/// <summary>
		/// Executes a pre-compiled script
		/// </summary>
		/// <param name="precompiledScript">A pre-compiled script that can be executed by different
		/// instances of JS engine</param>
		public virtual void Execute(IPrecompiledScript precompiledScript)
		{
			InnerEngine.Execute(precompiledScript);
		}

		/// <summary>
		/// Executes a code from JS file
		/// </summary>
		/// <param name="path">Path to the JS file</param>
		/// <param name="encoding">Text encoding</param>
		public virtual void ExecuteFile(string path, Encoding encoding = null)
		{
			InnerEngine.ExecuteFile(path, encoding);
		}

		/// <summary>
		/// Executes a code from embedded JS resource
		/// </summary>
		/// <param name="resourceName">The case-sensitive resource name without the namespace of the specified type</param>
		/// <param name="type">The type, that determines the assembly and whose namespace is used to scope
		/// the resource name</param>
		public virtual void ExecuteResource(string resourceName, Type type)
		{
			InnerEngine.ExecuteResource(resourceName, type);
		}

		/// <summary>
		/// Executes a code from embedded JS resource
		/// </summary>
		/// <param name="resourceName">The case-sensitive resource name</param>
		/// <param name="assembly">The assembly, which contains the embedded resource</param>
		public virtual void ExecuteResource(string resourceName, Assembly assembly)
		{
			InnerEngine.ExecuteResource(resourceName, assembly);
		}

		/// <summary>
		/// Calls a function
		/// </summary>
		/// <param name="functionName">Function name</param>
		/// <param name="args">Function arguments</param>
		/// <returns>Result of the function execution</returns>
		public virtual object CallFunction(string functionName, params object[] args)
		{
			return InnerEngine.CallFunction(functionName, args);
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
			return InnerEngine.CallFunction<T>(functionName, args);
		}

		/// <summary>
		/// Сhecks for the existence of a variable
		/// </summary>
		/// <param name="variableName">Variable name</param>
		/// <returns>Result of check (true - exists; false - not exists</returns>
		public virtual bool HasVariable(string variableName)
		{
			return InnerEngine.HasVariable(variableName);
		}

		/// <summary>
		/// Gets a value of variable
		/// </summary>
		/// <param name="variableName">Variable name</param>
		/// <returns>Value of variable</returns>
		public virtual object GetVariableValue(string variableName)
		{
			return InnerEngine.GetVariableValue(variableName);
		}

		/// <summary>
		/// Gets a value of variable
		/// </summary>
		/// <typeparam name="T">Type of variable</typeparam>
		/// <param name="variableName">Variable name</param>
		/// <returns>Value of variable</returns>
		public virtual T GetVariableValue<T>(string variableName)
		{
			return InnerEngine.GetVariableValue<T>(variableName);
		}

		/// <summary>
		/// Sets a value of variable
		/// </summary>
		/// <param name="variableName">Variable name</param>
		/// <param name="value">Value of variable</param>
		public virtual void SetVariableValue(string variableName, object value)
		{
			InnerEngine.SetVariableValue(variableName, value);
		}

		/// <summary>
		/// Removes a variable
		/// </summary>
		/// <param name="variableName">Variable name</param>
		public virtual void RemoveVariable(string variableName)
		{
			InnerEngine.RemoveVariable(variableName);
		}

		/// <summary>
		/// Embeds a .NET object in the JS engine
		/// </summary>
		/// <param name="itemName">Name of the item</param>
		/// <param name="value">Value of the item</param>
		/// <remarks>Allows to embed instances of simple classes (or structures) and delegates.</remarks>
		public virtual void EmbedHostObject(string itemName, object value)
		{
			InnerEngine.EmbedHostObject(itemName, value);
		}

		/// <summary>
		/// Embeds a .NET type in the JS engine
		/// </summary>
		/// <param name="itemName">Name of the type</param>
		/// <param name="type">The type</param>
		/// <remarks>
		/// .NET types are exposed to script code in the form of objects whose properties and
		/// methods are bound to the type's static members.
		/// </remarks>
		public virtual void EmbedHostType(string itemName, Type type)
		{
			InnerEngine.EmbedHostType(itemName, type);
		}

		/// <summary>
		/// Interrupts script execution and causes the JS engine to throw an exception
		/// </summary>
		public virtual void Interrupt()
		{
			InnerEngine.Interrupt();
		}

		/// <summary>
		/// Collects the garbage
		/// </summary>
		public virtual void CollectGarbage()
		{
			InnerEngine.CollectGarbage();
		}
		#endregion
	}
}
