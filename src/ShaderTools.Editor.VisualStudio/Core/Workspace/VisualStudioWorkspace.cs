using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using ShaderTools.CodeAnalysis;
using ShaderTools.CodeAnalysis.Host.Mef;
using ShaderTools.EditorServices.Host;

namespace ShaderTools.Editor.VisualStudio.Core.Workspace
{
    [Export(typeof(VisualStudioWorkspace))]
    internal sealed class VisualStudioWorkspace : CodeAnalysis.Workspace
    {
        private readonly Dictionary<ITextBuffer, List<IWpfTextView>> _bufferToViewMap;
        private readonly BackgroundParser _backgroundParser;

        [ImportingConstructor]
        public VisualStudioWorkspace(ExportProvider exportProvider)
            : base(MefV1HostServices.Create(exportProvider))
        {
            _bufferToViewMap = new Dictionary<ITextBuffer, List<IWpfTextView>>();

            _backgroundParser = new BackgroundParser(this);
            _backgroundParser.Start();
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

        public void RegisterBuffers(IWpfTextView textView, IEnumerable<ITextBuffer> subjectBuffers)
        {
            //if (!TextDocumentFactoryService.TryGetTextDocument(textView.TextBuffer, out var document))
            //    return;

            foreach (var buffer in subjectBuffers)
            {
                if (!_bufferToViewMap.TryGetValue(buffer, out var textViews))
                {
                    _bufferToViewMap[buffer] = textViews = new List<IWpfTextView>();
                    // TODO: Call OnDocumentOpened()
                }

                textViews.Add(textView);
            }
        }

        public void UnregisterBuffers(IWpfTextView textView, IEnumerable<ITextBuffer> subjectBuffers)
        {
            foreach (var buffer in subjectBuffers)
            {
                if (!_bufferToViewMap.TryGetValue(buffer, out var textViews))
                    throw new InvalidOperationException();

                if (!textViews.Remove(textView))
                    throw new InvalidOperationException();

                if (textViews.Count == 0)
                {
                    _bufferToViewMap.Remove(buffer);

                    // TODO: Call OnDocumentClosed()
                }
            }
        }
    }
}
