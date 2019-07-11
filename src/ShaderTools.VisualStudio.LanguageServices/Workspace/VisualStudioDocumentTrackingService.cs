// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using ShaderTools.CodeAnalysis;
using ShaderTools.CodeAnalysis.Editor.Implementation;

namespace ShaderTools.VisualStudio.LanguageServices
{
    [ExportWorkspaceService(typeof(IDocumentTrackingService)), Shared]
    [DebuggerDisplay("{GetDebuggerDisplay(),nq}")]
    internal class VisualStudioDocumentTrackingService : IDocumentTrackingService, IVsSelectionEvents, IDisposable
    {
        private readonly IVsEditorAdaptersFactoryService _editorAdaptersFactoryService;
        private IVsMonitorSelection _monitorSelection;
        private uint _cookie;
        private IVsWindowFrame _activeFrame;

        [ImportingConstructor]
        public VisualStudioDocumentTrackingService(SVsServiceProvider serviceProvider, IVsEditorAdaptersFactoryService editorAdaptersFactoryService)
        {
            _editorAdaptersFactoryService = editorAdaptersFactoryService;

            _monitorSelection = (IVsMonitorSelection) serviceProvider.GetService(typeof(SVsShellMonitorSelection));
            _monitorSelection.AdviseSelectionEvents(this, out _cookie);
        }

        public event EventHandler<ImmutableArray<DocumentId>> ActiveDocumentChanged;

        /// <summary>
        /// Get the <see cref="DocumentId"/> of the active document. May be called from any thread.
        /// May return null if there is no active document or the active document is not part of this
        /// workspace.
        /// </summary>
        /// <returns>The ID of the active document (if any)</returns>
        public ImmutableArray<DocumentId> GetActiveDocuments()
        {
            if (_activeFrame == null)
            {
                return ImmutableArray<DocumentId>.Empty;
            }

            var vsTextView = VsShellUtilities.GetTextView(_activeFrame);

            var textView = _editorAdaptersFactoryService.GetWpfTextView(vsTextView);

            return ((VisualStudioWorkspace) PrimaryWorkspace.Workspace).GetDocumentIdsForTextView(textView);
        }

        public int OnSelectionChanged(IVsHierarchy pHierOld, [ComAliasName("Microsoft.VisualStudio.Shell.Interop.VSITEMID")]uint itemidOld, IVsMultiItemSelect pMISOld, ISelectionContainer pSCOld, IVsHierarchy pHierNew, [ComAliasName("Microsoft.VisualStudio.Shell.Interop.VSITEMID")]uint itemidNew, IVsMultiItemSelect pMISNew, ISelectionContainer pSCNew)
        {
            return VSConstants.E_NOTIMPL;
        }

        public int OnElementValueChanged([ComAliasName("Microsoft.VisualStudio.Shell.Interop.VSSELELEMID")]uint elementid, object varValueOld, object varValueNew)
        {
            if (elementid == (uint) VSConstants.VSSELELEMID.SEID_DocumentFrame)
            {
                // Remember the newly activated frame so it can be read from another thread.
                _activeFrame = varValueNew as IVsWindowFrame;
                this.ActiveDocumentChanged?.Invoke(this, GetActiveDocuments());
            }

            return VSConstants.S_OK;
        }

        public int OnCmdUIContextChanged([ComAliasName("Microsoft.VisualStudio.Shell.Interop.VSCOOKIE")]uint dwCmdUICookie, [ComAliasName("Microsoft.VisualStudio.OLE.Interop.BOOL")]int fActive)
        {
            return VSConstants.E_NOTIMPL;
        }

        public void Dispose()
        {
            
        }
    }
}