/*
 * Copyright (c) 2015 Daniel Lo Nigro (Daniel15)
 * 
 * This source code is licensed under the BSD-style license found in the 
 * LICENSE file in the root directory of this source tree. 
 */

using System;
using System.Collections.Generic;

namespace JSPool
{
	/// <summary>
	/// Handles watching for changes to files.
	/// </summary>
	public interface IFileWatcher : IDisposable
	{
		/// <summary>
		/// Occurs when any watched files have changed (including renames and deletions).
		/// </summary>
		event EventHandler Changed;

		/// <summary>
		/// Gets or sets the path to watch.
		/// </summary>
		string Path { get; set; }

		/// <summary>
		/// Gets or sets the files to watch in the path. If no files are provided, every file in the 
		/// path is watched.
		/// </summary>
		IEnumerable<string> Files { get; set; }

		/// <summary>
		/// Starts watching for changes in the specified path.
		/// </summary>
		/// <returns>Whether creation of the watcher was successful</returns>
		bool Start();

		/// <summary>
		/// Stops watching for changes in the specified path.
		/// </summary>
		void Stop();
	}
}