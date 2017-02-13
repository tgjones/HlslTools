//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using ShaderTools.Core.Options;
using ShaderTools.Core.Text;
using ShaderTools.EditorServices.Utility;

namespace ShaderTools.EditorServices.Workspace
{
    public abstract class Workspace
    {
        private ImmutableDictionary<string, Document> _workspaceFiles = ImmutableDictionary<string, Document>.Empty;
        private ImmutableDictionary<string, ConfigFile> _configFiles = ImmutableDictionary<string, ConfigFile>.Empty;

        /// <summary>
        /// Gets or sets the root path of the workspace.
        /// </summary>
        public string WorkspacePath { get; set; }

        /// <summary>
        /// Gets an open file in the workspace.  If the file isn't open but
        /// exists on the filesystem, load and return it.
        /// </summary>
        /// <param name="filePath">The file path at which the script resides.</param>
        /// <exception cref="FileNotFoundException">
        /// <paramref name="filePath"/> is not found.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="filePath"/> contains a null or empty string.
        /// </exception>
        public Document GetFile(string filePath)
        {
            Validate.IsNotNullOrEmptyString(nameof(filePath), filePath);

            // Resolve the full file path 
            string resolvedFilePath = this.ResolveFilePath(filePath);
            string keyName = resolvedFilePath.ToLower();

            // Make sure the file isn't already loaded into the workspace
            Document scriptFile = null;
            if (!_workspaceFiles.TryGetValue(keyName, out scriptFile))
            {
                // This method allows FileNotFoundException to bubble up 
                // if the file isn't found.
                using (FileStream fileStream = new FileStream(resolvedFilePath, FileMode.Open, FileAccess.Read))
                using (StreamReader streamReader = new StreamReader(fileStream, Encoding.UTF8))
                {
                    var text = SourceText.From(streamReader.ReadToEnd(), resolvedFilePath);
                    scriptFile = CreateDocument(text, filePath);

                    _workspaceFiles = _workspaceFiles.Add(keyName, scriptFile);
                }

                Logger.Write(LogLevel.Verbose, "Opened file on disk: " + resolvedFilePath);
            }

            return scriptFile;
        }

        public bool TryGetDocument(string filePath, out Document document)
        {
            var keyName = ResolveFilePath(filePath).ToLower();
            return _workspaceFiles.TryGetValue(keyName, out document);
        }

        protected abstract Document CreateDocument(SourceText sourceText, string clientFilePath);

        /// <summary>
        /// Gets a new <see cref="Document"/> instance which is identified by the given file
        /// path and initially contains the given buffer contents.
        /// </summary>
        /// <param name="filePath">The file path for which a buffer will be retrieved.</param>
        /// <param name="initialBuffer">The initial buffer contents if there is not an existing ScriptFile for this path.</param>
        /// <returns>A ScriptFile instance for the specified path.</returns>
        public Document GetFileBuffer(string filePath, string initialBuffer)
        {
            Validate.IsNotNullOrEmptyString(nameof(filePath), filePath);

            // Resolve the full file path 
            string resolvedFilePath = this.ResolveFilePath(filePath);
            string keyName = resolvedFilePath.ToLower();

            // Make sure the file isn't already loaded into the workspace
            Document scriptFile = null;
            if (!_workspaceFiles.TryGetValue(keyName, out scriptFile) && initialBuffer != null)
            {
                scriptFile = CreateDocument(SourceText.From(initialBuffer, resolvedFilePath), filePath);

                _workspaceFiles = _workspaceFiles.Add(keyName, scriptFile);

                Logger.Write(LogLevel.Verbose, "Opened file as in-memory buffer: " + resolvedFilePath);
            }

            return scriptFile;
        }

        /// <summary>
        /// Gets an array of all opened <see cref="Document"/>s in the workspace.
        /// </summary>
        /// <returns>An array of all opened <see cref="Document"/>s in the workspace.</returns>
        public Document[] GetOpenedFiles()
        {
            return _workspaceFiles.Values.ToArray();
        }

        /// <summary>
        /// Closes a currently open <see cref="Document"/>.
        /// </summary>
        /// <param name="document">The <see cref="Document"/> to close.</param>
        public void CloseFile(Document document)
        {
            Validate.IsNotNull(nameof(document), document);

            _workspaceFiles = _workspaceFiles.Remove(document.Id);
        }

        private string ResolveFilePath(string filePath)
        {
            if (!IsPathInMemory(filePath))
            {
                if (filePath.StartsWith(@"file://"))
                {
                    // Client sent the path in URI format, extract the local path
                    Uri fileUri = new Uri(Uri.UnescapeDataString(filePath));
                    filePath = fileUri.LocalPath;
                }

                // Get the absolute file path
                filePath = Path.GetFullPath(filePath);
            }

            Logger.Write(LogLevel.Verbose, "Resolved path: " + filePath);

            return filePath;
        }

        internal static bool IsPathInMemory(string filePath)
        {
            // When viewing PowerShell files in the Git diff viewer, VS Code
            // sends the contents of the file at HEAD with a URI that starts
            // with 'inmemory'.  Untitled files which have been marked of
            // type PowerShell have a path starting with 'untitled'.
            return
                filePath.StartsWith("inmemory") ||
                filePath.StartsWith("untitled") ||
                filePath.StartsWith("private") ||
                filePath.StartsWith("git");

            // TODO #342: Remove 'private' and 'git' and then add logic to
            // throw when any unsupported file URI scheme is encountered.
        }

        internal ConfigFile LoadConfigFile(string directory)
        {
            return ImmutableInterlocked.GetOrAdd(
                ref _configFiles, 
                directory.ToLower(), 
                x => ConfigFileLoader.LoadAndMergeConfigFile(x));
        }
    }
}
