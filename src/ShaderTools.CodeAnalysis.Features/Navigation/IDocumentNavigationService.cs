// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Host;
using ShaderTools.CodeAnalysis.Options;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Navigation
{
    internal interface IDocumentNavigationService : IWorkspaceService
    {
        /// <summary>
        /// Determines whether it is possible to navigate to the given position in the specified document.
        /// </summary>
        bool CanNavigateToSpan(Workspace workspace, DocumentId documentId, SourceFileSpan textSpan);
        
        /// <summary>
        /// Navigates to the given position in the specified document, opening it if necessary.
        /// </summary>
        bool TryNavigateToSpan(Workspace workspace, DocumentId documentId, SourceFileSpan textSpan, OptionSet options = null);

        bool TryNavigateToPosition(Workspace workspace, DocumentId documentId, int position, int virtualSpace = 0, OptionSet options = null);
    }
}
