// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Text;
using ShaderTools.CodeAnalysis.Formatting;
using ShaderTools.CodeAnalysis.Text;
using ShaderTools.Utilities.Collections;
using ShaderTools.Utilities.Threading;

namespace ShaderTools.CodeAnalysis.Editor.Shared.Extensions
{
    internal static partial class ITextSnapshotExtensions
    {
        /// <summary>
        /// format given snapshot and apply text changes to buffer
        /// </summary>
        public static void FormatAndApplyToBuffer(this ITextSnapshot snapshot, TextSpan span, CancellationToken cancellationToken)
        {
            var document = snapshot.GetOpenDocumentInCurrentContextWithChanges();
            if (document == null)
            {
                return;
            }

            var tree = document.GetSyntaxTreeSynchronously(cancellationToken);
            var documentOptions = document.GetOptionsAsync(cancellationToken).WaitAndGetResult(cancellationToken);
            var changes = Formatter.GetFormattedTextChanges(tree, tree.Root, SpecializedCollections.SingletonEnumerable(span), document.Workspace, documentOptions, cancellationToken);

            //using (Logger.LogBlock(FunctionId.Formatting_ApplyResultToBuffer, cancellationToken))
            {
                document.Workspace.ApplyTextChanges(document.Id, changes, cancellationToken);
            }
        }
    }
}
