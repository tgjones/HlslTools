// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Host;

namespace ShaderTools.CodeAnalysis.Editor.Implementation
{
    internal interface IDocumentTrackingService : IWorkspaceService
    {
        /// <summary>
        /// Get the <see cref="DocumentId"/> of the active document. May be null if there is no active document
        /// or the active document is not in the workspace.
        /// </summary>
        ImmutableArray<DocumentId> GetActiveDocuments();

        event EventHandler<ImmutableArray<DocumentId>> ActiveDocumentChanged;
    }
}
