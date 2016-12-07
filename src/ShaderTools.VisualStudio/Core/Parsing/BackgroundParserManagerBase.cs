using System;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace ShaderTools.VisualStudio.Core.Parsing
{
    internal abstract class BackgroundParserManagerBase : IWpfTextViewCreationListener
    {
        protected abstract BackgroundParserBase GetBackgroundParser(ITextBuffer textBuffer);

        public void TextViewCreated(IWpfTextView textView)
        {
            // Ensure BackgroundParser is created.
            GetBackgroundParser(textView.TextBuffer);
            textView.Closed += OnViewClosed;
        }

        private void OnViewClosed(object sender, EventArgs e)
        {
            var view = (IWpfTextView) sender;
            view.Closed -= OnViewClosed;

            var backgroundParser = GetBackgroundParser(view.TextBuffer);
            backgroundParser?.Dispose();
        }
    }
}
