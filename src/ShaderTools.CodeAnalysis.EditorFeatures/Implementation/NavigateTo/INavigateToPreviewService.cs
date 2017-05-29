// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.VisualStudio.Language.NavigateTo.Interfaces;
using ShaderTools.CodeAnalysis.Host;

namespace ShaderTools.CodeAnalysis.Editor.Implementation.NavigateTo
{
    internal interface INavigateToPreviewService : IWorkspaceService
    {
        int GetProvisionalViewingStatus(Document document);
        void PreviewItem(INavigateToItemDisplay itemDisplay);
    }
}
