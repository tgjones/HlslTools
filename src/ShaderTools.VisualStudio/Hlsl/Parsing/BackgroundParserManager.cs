using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using ShaderTools.VisualStudio.Hlsl.Util.Extensions;

namespace ShaderTools.VisualStudio.Hlsl.Parsing
{
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType(HlslConstants.ContentTypeName)]
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    internal sealed class BackgroundParserManager : IWpfTextViewCreationListener
    {
        public void TextViewCreated(IWpfTextView textView)
        {
            // Ensure BackgroundParser is created.
            textView.TextBuffer.GetBackgroundParser();
            textView.Closed += OnViewClosed;
        }

        private void OnViewClosed(object sender, EventArgs e)
        {
            var view = (IWpfTextView)sender;
            view.Closed -= OnViewClosed;

            var backgroundParser = view.TextBuffer.GetBackgroundParser();
            backgroundParser?.Dispose();
        }
    }
}