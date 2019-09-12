using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using ShaderTools.CodeAnalysis;
using ShaderTools.CodeAnalysis.Editor.Implementation;
using ShaderTools.CodeAnalysis.Editor.Shared.Utilities;
using ShaderTools.CodeAnalysis.Host;
using ShaderTools.CodeAnalysis.Host.Mef;
using ShaderTools.CodeAnalysis.Options;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.VisualStudio.LanguageServices
{
    [Export(typeof(VisualStudioWorkspace))]
    internal sealed class VisualStudioWorkspace : Workspace
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ITextDocumentFactoryService _textDocumentFactoryService;
        private readonly BackgroundParser _backgroundParser;
        private readonly ConditionalWeakTable<ITextBuffer, ITextDocument> _textBufferToTextDocumentMap;
        private readonly ConditionalWeakTable<DocumentId, ITextBuffer> _documentIdToTextBufferMap;
        private readonly ConditionalWeakTable<ITextBuffer, DocumentId> _textBufferToDocumentIdMap;
        private readonly ConditionalWeakTable<ITextBuffer, List<ITextView>> _textBufferToViewsMap;
        private readonly ConditionalWeakTable<ITextView, List<ITextBuffer>> _textViewToBuffersMap;

        /// <summary>
        /// A <see cref="ForegroundThreadAffinitizedObject"/> to make assertions that stuff is on the right thread.
        /// This is Lazy because it might be created on a background thread when nothing is initialized yet.
        /// </summary>
        private readonly Lazy<ForegroundThreadAffinitizedObject> _foregroundObject
            = new Lazy<ForegroundThreadAffinitizedObject>(() => new ForegroundThreadAffinitizedObject());

        [ImportingConstructor]
        public VisualStudioWorkspace(
            SVsServiceProvider serviceProvider,
            ITextDocumentFactoryService textDocumentFactoryService)
            : base(MefV1HostServices.Create(GetExportProvider(serviceProvider)))
        {
            PrimaryWorkspace.Register(this);

            _serviceProvider = serviceProvider;
            _textDocumentFactoryService = textDocumentFactoryService;

            _backgroundParser = new BackgroundParser(this);
            _backgroundParser.Start();

            _textBufferToTextDocumentMap = new ConditionalWeakTable<ITextBuffer, ITextDocument>();
            _documentIdToTextBufferMap = new ConditionalWeakTable<DocumentId, ITextBuffer>();
            _textBufferToDocumentIdMap = new ConditionalWeakTable<ITextBuffer, DocumentId>();
            _textBufferToViewsMap = new ConditionalWeakTable<ITextBuffer, List<ITextView>>();
            _textViewToBuffersMap = new ConditionalWeakTable<ITextView, List<ITextBuffer>>();

            Services.GetService<IDocumentTrackingService>();

            if (Services.GetService<IOptionService>() is OptionServiceFactory.OptionService optionService)
            {
                optionService.RegisterDocumentOptionsProvider(new Implementation.Options.EditorconfigDocumentOptionsProvider());
            }
        }

        public override bool CanOpenDocuments => true;

        public override void OpenDocument(DocumentId documentId, bool activate = true)
        {
            if (documentId == null)
            {
                throw new ArgumentNullException(nameof(documentId));
            }

            if (!_foregroundObject.Value.IsForeground())
            {
                throw new InvalidOperationException("This workspace only supports opening documents on the UI thread.");
            }

            var document = CurrentDocuments.GetDocument(documentId);
            if (document == null)
            {
                return;
            }

            uint itemID;
            IVsUIHierarchy hierarchy;
            IVsWindowFrame docFrame;
            IVsTextView textView;

            try
            {
                VsShellUtilities.OpenDocument(
                    _serviceProvider, document.FilePath, VSConstants.LOGVIEWID_Code,
                    out hierarchy, out itemID, out docFrame, out textView);
            }
            catch
            {
                // File might not exist, etc.
                return;
            }

            if (activate)
            {
                docFrame.Show();
            }
            else
            {
                docFrame.ShowNoActivate();
            }
        }

        private static ExportProvider GetExportProvider(SVsServiceProvider serviceProvider)
        {
            var componentModel = (IComponentModel) serviceProvider.GetService(typeof(SComponentModel));
            return componentModel.DefaultExportProvider;
        }

        protected override void OnDocumentTextChanged(Document document)
        {
            if (_backgroundParser != null)
            {
                _backgroundParser.Parse(document);
            }
        }

        protected override void OnDocumentClosing(DocumentId documentId)
        {
            if (_backgroundParser != null)
            {
                _backgroundParser.CancelParse(documentId);
            }
        }

        internal void OnSubjectBufferConnected(ITextView textView, ITextBuffer textBuffer)
        {
            // Add this ITextView to the list of known views for this buffer.
            _textBufferToViewsMap.GetOrCreateValue(textBuffer).Add(textView);
            _textViewToBuffersMap.GetOrCreateValue(textView).Add(textBuffer);

            // Do we already know about this text buffer?
            if (_textBufferToDocumentIdMap.TryGetValue(textBuffer, out var _))
            {
                return;
            }

            // If this is the disk buffer, get the associated ITextDocument.
            if (_textDocumentFactoryService.TryGetTextDocument(textBuffer, out var textDocument))
            {
                _textBufferToTextDocumentMap.Add(textBuffer, textDocument);

                // TODO: hookup textDocument.FileActionOccurred for renames.
            }

            var debugName = textDocument?.FilePath ?? "EmbeddedBuffer"; // TODO: Use more useful name based on projection buffer hierarchy.
            var documentId = DocumentId.CreateNewId(debugName);

            _documentIdToTextBufferMap.Add(documentId, textBuffer);
            _textBufferToDocumentIdMap.Add(textBuffer, documentId);

            var textContainer = textBuffer.AsTextContainer();

            var document = CreateDocument(
                documentId, 
                textBuffer.ContentType.TypeName, 
                new SourceFile(textContainer.CurrentText, textDocument?.FilePath));

            OnDocumentOpened(document);
        }

        internal void OnSubjectBufferDisconnected(ITextView textView, ITextBuffer textBuffer)
        {
            var textBuffers = _textViewToBuffersMap.GetOrCreateValue(textView);
            textBuffers.Remove(textBuffer);

            if (textBuffers.Count == 0)
            {
                _textViewToBuffersMap.Remove(textView);
            }

            // Remove text view from the list of known text views for this text buffer.
            var textViews = _textBufferToViewsMap.GetOrCreateValue(textBuffer);
            textViews.Remove(textView);

            // Only close document if this is the last view referencing the text buffer.
            if (textViews.Count > 0)
            {
                return;
            }

            if (!_textBufferToDocumentIdMap.TryGetValue(textBuffer, out var _))
            {
                throw new InvalidOperationException();
            }

            var document = textBuffer.AsTextContainer().GetOpenDocumentInCurrentContext();

            OnDocumentClosed(document.Id);

            _documentIdToTextBufferMap.Remove(document.Id);
            _textBufferToDocumentIdMap.Remove(textBuffer);
            _textBufferToTextDocumentMap.Remove(textBuffer);
            _textBufferToViewsMap.Remove(textBuffer);
        }

        protected override void ApplyDocumentTextChanged(DocumentId id, SourceText text)
        {
            var currentDocumentBuffer = CurrentDocuments.GetDocument(id)?.SourceText.Container.GetTextBuffer();
            if (currentDocumentBuffer == null)
            {
                return;
            }

            using (var edit = currentDocumentBuffer.CreateEdit(EditOptions.DefaultMinimalChange, reiteratedVersionNumber: null, editTag: null))
            {
                var oldSnapshot = currentDocumentBuffer.CurrentSnapshot;
                var oldText = oldSnapshot.AsText();
                var changes = text.GetTextChanges(oldText);
                //if (Workspace.TryGetWorkspace(oldText.Container, out var workspace))
                //{
                //    var undoService = workspace.Services.GetService<ISourceTextUndoService>();
                //    undoService.BeginUndoTransaction(oldSnapshot);
                //}

                foreach (var change in changes)
                {
                    edit.Replace(change.Span.Start, change.Span.Length, change.NewText);
                }

                edit.Apply();
            }
        }

        internal ImmutableArray<DocumentId> GetDocumentIdsForTextView(ITextView textView)
        {
            if (!_textViewToBuffersMap.TryGetValue(textView, out var textBuffers))
            {
                return ImmutableArray<DocumentId>.Empty;
            }

            return textBuffers
                .Select(x => x.AsTextContainer()?.GetOpenDocumentInCurrentContext()?.Id)
                .Where(x => x != null)
                .ToImmutableArray();
        }

        internal ITextBuffer GetTextBufferForDocument(DocumentId documentId)
        {
            if (!_documentIdToTextBufferMap.TryGetValue(documentId, out var textBuffer))
            {
                return null;
            }

            return textBuffer;
        }
    }
}
