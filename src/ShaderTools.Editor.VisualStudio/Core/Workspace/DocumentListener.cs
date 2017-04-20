using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace ShaderTools.Editor.VisualStudio.Core.Workspace
{
    //[Export(typeof(IWpfTextViewConnectionListener))]
    [ContentType(ContentTypeNames.ShaderToolsContentType)]
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    internal sealed class DocumentListener : IWpfTextViewConnectionListener
    {
        private readonly VisualStudioWorkspace _workspace;

        [ImportingConstructor]
        public DocumentListener(VisualStudioWorkspace workspace)
        {
            _workspace = workspace;
        }

        public void SubjectBuffersConnected(IWpfTextView textView, ConnectionReason reason, Collection<ITextBuffer> subjectBuffers)
        {
            _workspace.RegisterBuffers(textView, subjectBuffers);
        }

        public void SubjectBuffersDisconnected(IWpfTextView textView, ConnectionReason reason, Collection<ITextBuffer> subjectBuffers)
        {
            _workspace.UnregisterBuffers(textView, subjectBuffers);
        }
    }
}
