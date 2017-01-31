// Original code from https://github.com/Microsoft/nodejstools/blob/master/Nodejs/Product/Nodejs/NodejsEditorFactory.cs
// Licensed under the Apache 2.0 license.

using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using ShaderTools.Editor.VisualStudio.Core.Navigation;
using ShaderTools.Editor.VisualStudio.Core.Util;
using ShaderTools.Editor.VisualStudio.Core.Util.Extensions;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace ShaderTools.Editor.VisualStudio.Core
{
    internal abstract class EditorFactoryBase : IVsEditorFactory
    {
        private readonly LanguagePackageBase _package;
        private IOleServiceProvider _oleServiceProvider;
        private ServiceProvider _serviceProvider;

        protected EditorFactoryBase(LanguagePackageBase package)
        {
            _package = package;
        }

        public int CreateEditorInstance(uint createEditorFlags,
            string documentMoniker,
            string physicalView,
            IVsHierarchy hierarchy,
            uint itemid,
            IntPtr docDataExisting,
            out IntPtr docView,
            out IntPtr docData,
            out string editorCaption,
            out Guid commandUIGuid,
            out int createDocumentWindowFlags)
        {
            // Initialize output parameters
            docView = IntPtr.Zero;
            docData = IntPtr.Zero;
            commandUIGuid = Guid.Empty;
            createDocumentWindowFlags = 0;
            editorCaption = null;

            // Validate inputs
            if ((createEditorFlags & (uint) (VSConstants.CEF.OpenFile | VSConstants.CEF.Silent)) == 0)
            {
                return VSConstants.E_INVALIDARG;
            }

            // Get a text buffer
            IVsTextLines textLines = GetTextBuffer(docDataExisting, documentMoniker);

            // Assign docData IntPtr to either existing docData or the new text buffer
            if (docDataExisting != IntPtr.Zero)
            {
                docData = docDataExisting;
                Marshal.AddRef(docData);
            }
            else
            {
                docData = Marshal.GetIUnknownForObject(textLines);
            }

            try
            {
                object docViewObject = CreateDocumentView(documentMoniker, physicalView, hierarchy, itemid, textLines, docDataExisting == IntPtr.Zero, out editorCaption, out commandUIGuid);
                docView = Marshal.GetIUnknownForObject(docViewObject);
            }
            finally
            {
                if (docView == IntPtr.Zero)
                {
                    if (docDataExisting != docData && docData != IntPtr.Zero)
                    {
                        // Cleanup the instance of the docData that we have addref'ed
                        Marshal.Release(docData);
                        docData = IntPtr.Zero;
                    }
                }
            }
            return VSConstants.S_OK;
        }

        private IVsTextLines GetTextBuffer(System.IntPtr docDataExisting, string filename)
        {
            IVsTextLines textLines;
            if (docDataExisting == IntPtr.Zero)
            {
                // Create a new IVsTextLines buffer.
                Type textLinesType = typeof(IVsTextLines);
                Guid riid = textLinesType.GUID;
                Guid clsid = typeof(VsTextBufferClass).GUID;
                textLines = _package.CreateInstance(ref clsid, ref riid, textLinesType) as IVsTextLines;

                // set the buffer's site
                ((IObjectWithSite) textLines).SetSite(_serviceProvider.GetService(typeof(IOleServiceProvider)));
            }
            else
            {
                // Use the existing text buffer
                Object dataObject = Marshal.GetObjectForIUnknown(docDataExisting);
                textLines = dataObject as IVsTextLines;
                if (textLines == null)
                {
                    // Try get the text buffer from textbuffer provider
                    IVsTextBufferProvider textBufferProvider = dataObject as IVsTextBufferProvider;
                    if (textBufferProvider != null)
                    {
                        textBufferProvider.GetTextBuffer(out textLines);
                    }
                }
                if (textLines == null)
                {
                    // Unknown docData type then, so we have to force VS to close the other editor.
                    throw Marshal.GetExceptionForHR(VSConstants.VS_E_INCOMPATIBLEDOCDATA);
                }

            }
            return textLines;
        }

        private object CreateDocumentView(string documentMoniker, string physicalView, IVsHierarchy hierarchy, uint itemid, IVsTextLines textLines, bool createdDocData, out string editorCaption, out Guid cmdUI)
        {
            //Init out params
            editorCaption = string.Empty;
            cmdUI = Guid.Empty;

            if (string.IsNullOrEmpty(physicalView))
            {
                // create code window as default physical view
                return CreateCodeView(documentMoniker, textLines, createdDocData, ref editorCaption, ref cmdUI);
            }

            // We couldn't create the view
            // Return special error code so VS can try another editor factory.
            throw Marshal.GetExceptionForHR(VSConstants.VS_E_UNSUPPORTEDFORMAT);
        }

        private IVsCodeWindow CreateCodeView(string documentMoniker, IVsTextLines textLines, bool createdDocData, ref string editorCaption, ref Guid cmdUI)
        {
            Type codeWindowType = typeof(IVsCodeWindow);
            var compModel = _package.AsVsServiceProvider().GetComponentModel();
            var adapterService = compModel.GetService<IVsEditorAdaptersFactoryService>();

            var window = adapterService.CreateVsCodeWindowAdapter(_oleServiceProvider);
            ErrorHandler.ThrowOnFailure(window.SetBuffer(textLines));
            ErrorHandler.ThrowOnFailure(window.SetBaseEditorCaption(null));
            ErrorHandler.ThrowOnFailure(window.GetEditorCaption(READONLYSTATUS.ROSTATUS_Unknown, out editorCaption));

            cmdUI = VSConstants.GUID_TextEditorFactory;

            var textMgr = _package.AsVsServiceProvider().GetTextManager();
            var bufferEventListener = new TextBufferEventListener(this, compModel, textLines, textMgr, window);
            if (!createdDocData)
            {
                // we have a pre-created buffer, go ahead and initialize now as the buffer already
                // exists and is initialized.
                bufferEventListener.OnLoadCompleted(0);
            }

            return window;
        }

        /// <summary>
        /// Listens for the text buffer to finish loading and then sets up our projection
        /// buffer.
        /// </summary>
        private sealed class TextBufferEventListener : IVsTextBufferDataEvents
        {
            private readonly EditorFactoryBase _editorFactory;
            private readonly IVsTextLines _textLines;
            private readonly IComEventSink _textLinesEventsSink;
            private readonly IComponentModel _compModel;
            private readonly IVsTextManager _textMgr;
            private readonly IVsCodeWindow _window;

            public TextBufferEventListener(EditorFactoryBase editorFactory, IComponentModel compModel, IVsTextLines textLines, IVsTextManager textMgr, IVsCodeWindow window)
            {
                _editorFactory = editorFactory;
                _textLines = textLines;
                _compModel = compModel;
                _textMgr = textMgr;
                _window = window;

                _textLinesEventsSink = ComEventSink.Advise<IVsTextBufferDataEvents>(textLines, this);
            }

            #region IVsTextBufferDataEvents

            public void OnFileChanged(uint grfChange, uint dwFileAttrs)
            {
            }

            public int OnLoadCompleted(int fReload)
            {
                _textLinesEventsSink.Unadvise();

                Guid langSvcGuid = _editorFactory.GetLanguageInfoType().GUID;
                _textLines.SetLanguageServiceID(ref langSvcGuid);

                return VSConstants.S_OK;
            }

            #endregion
        }

        protected abstract Type GetLanguageInfoType();

        public int SetSite(IOleServiceProvider psp)
        {
            _oleServiceProvider = psp;
            _serviceProvider = new ServiceProvider(psp);
            return VSConstants.S_OK;
        }

        public int Close()
        {
            return VSConstants.S_OK;
        }

        public int MapLogicalView(ref Guid rguidLogicalView, out string pbstrPhysicalView)
        {
            // initialize out parameter
            pbstrPhysicalView = null;

            var isSupportedView =
                VSConstants.LOGVIEWID_Primary == rguidLogicalView ||
                VSConstants.LOGVIEWID_Code == rguidLogicalView ||
                VSConstants.LOGVIEWID_TextView == rguidLogicalView;

            if (isSupportedView)
                return VSConstants.S_OK;

            // E_NOTIMPL must be returned for any unrecognized rguidLogicalView values
            return VSConstants.E_NOTIMPL;
        }
    }
}