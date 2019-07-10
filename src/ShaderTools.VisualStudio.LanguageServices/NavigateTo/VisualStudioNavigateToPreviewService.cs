// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Language.NavigateTo.Interfaces;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using ShaderTools.CodeAnalysis;
using ShaderTools.CodeAnalysis.Editor.Implementation.NavigateTo;

namespace ShaderTools.VisualStudio.LanguageServices.NavigateTo
{
    [ExportWorkspaceService(typeof(INavigateToPreviewService))]
    internal sealed class VisualStudioNavigateToPreviewService : INavigateToPreviewService
    {
        public int GetProvisionalViewingStatus(Document document)
        {
            if (document.FilePath == null)
            {
                return (int) __VSPROVISIONALVIEWINGSTATUS.PVS_Disabled;
            }

            return (int) VsShellUtilities.GetProvisionalViewingStatus(document.FilePath);
        }

        public void PreviewItem(INavigateToItemDisplay itemDisplay)
        {
            // Because NavigateTo synchronously opens the file, and because
            // the NavigateTo UI automatically creates a NewDocumentStateScope,
            // preview can be accomplished by simply calling NavigateTo.

            // Navigation may fail to open the document, which can result in an exception
            // in expected cases if preview is not supported.  CallWithCOMConvention handles
            // non-critical exceptions
            ErrorHandler.CallWithCOMConvention(() => itemDisplay.NavigateTo());
        }
    }
}
