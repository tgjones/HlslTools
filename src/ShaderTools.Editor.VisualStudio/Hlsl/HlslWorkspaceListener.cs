using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace ShaderTools.Editor.VisualStudio.Hlsl
{
    //[Export(typeof(IWpfTextViewCreationListener))]
    [ContentType(HlslConstants.ContentTypeName)]
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    internal sealed class HlslWorkspaceListener : IWpfTextViewCreationListener
    {
        //[Import]
        //public HlslWorkspaceManager WorkspaceManager { get; set; }

        [Import]
        public ITextDocumentFactoryService TextDocumentFactoryService { get; set; }

        public void TextViewCreated(IWpfTextView textView)
        {
            //if (!TextDocumentFactoryService.TryGetTextDocument(textView.TextBuffer, out var document))
            //    return;

            //WorkspaceManager.Workspace.GetFileBuffer(document.FilePath, textView.TextBuffer.CurrentSnapshot.GetText());

            //document.FileActionOccurred += OnFileActionOccurred;

            //textView.Closed += OnTextViewClosed;

            //textView.TextBuffer.Properties.GetOrCreateSingletonProperty(() => WorkspaceManager);
        }

        private void OnFileActionOccurred(object sender, TextDocumentFileActionEventArgs e)
        {
            //if (e.FileActionType != FileActionTypes.DocumentRenamed)
            //    return;

            //var document = (ITextDocument) sender;

            //var oldFilePath = document.FilePath;
            //var newFilePath = e.FilePath;

            //WorkspaceManager.Workspace.RenameDocument(oldFilePath, newFilePath);
        }

        private void OnTextViewClosed(object sender, EventArgs e)
        {
            //var textView = (IWpfTextView) sender;
            //textView.Closed -= OnTextViewClosed;

            //if (!TextDocumentFactoryService.TryGetTextDocument(textView.TextBuffer, out var document))
            //    return;

            //WorkspaceManager.Workspace.CloseDocument(document.FilePath);
        }
    }
}
