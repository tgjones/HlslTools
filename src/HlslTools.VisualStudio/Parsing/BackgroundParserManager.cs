using System;
using System.ComponentModel.Composition;
using HlslTools.VisualStudio.Text;
using HlslTools.VisualStudio.Util.Extensions;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace HlslTools.VisualStudio.Parsing
{
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType(HlslConstants.ContentTypeName)]
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    internal sealed class BackgroundParserManager : IWpfTextViewCreationListener
    {
        [Import]
        public VisualStudioSourceTextFactory SourceTextFactory { get; set; }

        public void TextViewCreated(IWpfTextView textView)
        {
            // Ensure BackgroundParser is created.
            textView.TextBuffer.GetBackgroundParser(SourceTextFactory);
            textView.Closed += OnViewClosed;
        }

        private void OnViewClosed(object sender, EventArgs e)
        {
            var view = (IWpfTextView)sender;
            view.Closed -= OnViewClosed;

            var backgroundParser = view.TextBuffer.GetBackgroundParser(SourceTextFactory);
            backgroundParser?.Dispose();
        }
    }
}