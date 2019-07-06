// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Text
{
    // The parts of a workspace that deal with open documents
    internal static class TextExtensions
    {
        /// <summary>
        /// Gets the document from the corresponding workspace's current solution that is associated with the source text's container 
        /// in its current project context, updated to contain the same text as the source if necessary.
        /// </summary>
        public static Document GetOpenDocumentInCurrentContextWithChanges(this SourceText text)
        {
            if (Workspace.TryGetWorkspace(text.Container, out var workspace))
            {
                var id = workspace.GetDocumentIdInCurrentContext(text.Container);
                if (id == null || !workspace.CurrentDocuments.ContainsDocument(id))
                {
                    return null;
                }

                var sol = workspace.CurrentDocuments.WithDocumentText(id, text);
                return sol.GetDocument(id);
            }

            return null;
        }

        /// <summary>
        /// Gets the document from the corresponding workspace's current solution that is associated with the text container 
        /// in its current project context.
        /// </summary>
        public static Document GetOpenDocumentInCurrentContext(this SourceTextContainer container)
        {
            if (Workspace.TryGetWorkspace(container, out var workspace))
            {
                var id = workspace.GetDocumentIdInCurrentContext(container);
                return workspace.CurrentDocuments.GetDocument(id);
            }

            return null;
        }
    }
}
