using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using ShaderTools.CodeAnalysis.Editor;

namespace ShaderTools.VisualStudio.LanguageServices
{
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType(ContentTypeNames.ShaderToolsContentType)]
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    internal sealed class WorkspaceBufferListener : IWpfTextViewCreationListener
    {
        private readonly VisualStudioWorkspace _workspace;

        [ImportingConstructor]
        public WorkspaceBufferListener(VisualStudioWorkspace workspace)
        {
            _workspace = workspace;
        }

        public void TextViewCreated(IWpfTextView textView)
        {
            _workspace.OnTextViewCreated(textView);

            textView.Closed += OnTextViewClosed;
        }

        private void OnTextViewClosed(object sender, EventArgs e)
        {
            var textView = (IWpfTextView) sender;
            textView.Closed -= OnTextViewClosed;

            _workspace.OnTextViewClosed(textView);
        }
    }
}
