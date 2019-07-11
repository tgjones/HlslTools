// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace ShaderTools.CodeAnalysis.Editor
{
    /// <summary>
    /// Command handler names
    /// </summary>
    internal static class PredefinedCommandHandlerNames
    {
        /// <summary>
        /// Command handler name for Completion. Some additional Completion commands are handled by
        /// the <see cref="IntelliSense"/> command handler.
        /// </summary>
        public const string Completion = "ShaderTools Completion Command Handler";

        /// <summary>
        /// Command handler name for Documentation Comments.
        /// </summary>
        public const string DocumentationComments = "ShaderTools Documentation Comments Command Handler";

        /// <summary>
        /// Command handler name for Rename.
        /// </summary>
        public const string Rename = "ShaderTools Rename Command Handler";

        /// <summary>
        /// Command handler name for Signature Help. Some additional Signature Help commands are
        /// handled by the <see cref="IntelliSense"/> command handler.
        /// </summary>
        public const string SignatureHelp = "ShaderTools Signature Help Command Handler";
    }
}
