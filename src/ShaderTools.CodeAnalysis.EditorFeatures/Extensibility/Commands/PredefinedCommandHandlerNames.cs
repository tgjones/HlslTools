// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace ShaderTools.CodeAnalysis.Editor
{
    /// <summary>
    /// Command handler names
    /// </summary>
    internal static class PredefinedCommandHandlerNames
    {
        /// <summary>
        /// Command handler name for Automatic Pair Completion
        /// </summary>
        /// <remarks></remarks>
        public const string AutomaticCompletion = "ShaderTools Automatic Pair Completion Command Handler";

        /// <summary>
        /// Command handler name for Automatic Line Ender
        /// </summary>
        /// <remarks></remarks>
        public const string AutomaticLineEnder = "ShaderTools Automatic Line Ender Command Handler";

        /// <summary>
        /// Command handler name for Change Signature.
        /// </summary>
        public const string ChangeSignature = "ShaderTools Change Signature";

        /// <summary>
        /// Command handler name for Class View.
        /// </summary>
        public const string ClassView = "ShaderTools Class View";

        /// <summary>
        /// Command handler name for Comment Selection.
        /// </summary>
        /// <remarks></remarks>
        public const string CommentSelection = "ShaderTools Comment Selection Command Handler";

        /// <summary>
        /// Command handler name for Commit.
        /// </summary>
        /// <remarks></remarks>
        public const string Commit = "ShaderTools Commit Command Handler";

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
        /// Command handler name for Encapsulate Field.
        /// </summary>
        public const string EncapsulateField = nameof(EncapsulateField);

        /// <summary>
        /// Command handler name for End Construct.
        /// </summary>
        public const string EndConstruct = "ShaderTools End Construct Command Handler";

        /// <summary>
        /// Command handler name for Event Hookup.
        /// </summary>
        public const string EventHookup = "ShaderTools Event Hookup Command Handler";

        /// <summary>
        /// Command handler name for Extract Interface
        /// </summary>
        public const string ExtractInterface = "ShaderTools Extract Interface Command Handler";

        /// <summary>
        /// Command handler name for Extract Method
        /// </summary>
        public const string ExtractMethod = "ShaderTools Extract Method Command Handler";

        /// <summary>
        /// Command handler name for Find References.
        /// </summary>
        public const string FindReferences = "ShaderTools Find References Command Handler";

        /// <summary>
        /// Command handler name for Format Document.
        /// </summary>
        public const string FormatDocument = "ShaderTools Format Document Command Handler";

        /// <summary>
        /// Command handler name for Go to Definition.
        /// </summary>
        public const string GoToDefinition = "ShaderTools Go To Definition Command Handler";

        /// <summary>
        /// Command handler name for Go to Implementation.
        /// </summary>
        public const string GoToImplementation = "ShaderTools Go To Implementation Command Handler";

        /// <summary>
        /// Command handler name for Go to Adjacent Member.
        /// </summary>
        public const string GoToAdjacentMember = "ShaderTools Go To Adjacent Member Command Handler";

        /// <summary>
        /// Command handler name for Indent.
        /// </summary>
        public const string Indent = "ShaderTools Indent Command Handler";

        /// <summary>
        /// Command handler name for IntelliSense. This command handler handles some commands for
        /// <see cref="Completion"/>, <see cref="QuickInfo"/>, and <see cref="SignatureHelp"/>.
        /// </summary>
        public const string IntelliSense = nameof(IntelliSense);

        /// <summary>
        /// Command handler name for Navigate to Highlighted Reference.
        /// </summary>
        public const string NavigateToHighlightedReference = "ShaderTools Navigate to Highlighted Reference Command Handler";

        /// <summary>
        /// Command handler name for Organize Document.
        /// </summary>
        public const string OrganizeDocument = "ShaderTools Organize Document Command Handler";

        /// <summary>
        /// Command handler name for Quick Info. Some additional Quick Info commands are handled by
        /// the <see cref="IntelliSense"/> command handler.
        /// </summary>
        public const string QuickInfo = "ShaderTools Quick Info Command Handler";

        /// <summary>
        /// Command handler name for Rename.
        /// </summary>
        public const string Rename = "ShaderTools Rename Command Handler";

        /// <summary>
        /// Command handler name for Rename Tracking cancellation.
        /// </summary>
        public const string RenameTrackingCancellation = "ShaderTools Rename Tracking Cancellation Command Handler";

        /// <summary>
        /// Command handler name for Signature Help. Some additional Signature Help commands are
        /// handled by the <see cref="IntelliSense"/> command handler.
        /// </summary>
        public const string SignatureHelp = "ShaderTools Signature Help Command Handler";

        /// <summary>
        /// Command handler name for Paste Content in Interactive Format. 
        /// </summary>
        public const string InteractivePaste = "ShaderTools Interactive Paste Command Handler";
    }
}
