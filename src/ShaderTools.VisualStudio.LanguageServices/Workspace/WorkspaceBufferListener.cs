using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using ShaderTools.CodeAnalysis.Editor;

namespace ShaderTools.VisualStudio.LanguageServices
{
    [Export(typeof(IWpfTextViewConnectionListener))]
    [ContentType(ContentTypeNames.ShaderToolsContentType)]
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    internal sealed class WorkspaceBufferListener : IWpfTextViewConnectionListener
    {
        private readonly VisualStudioWorkspace _workspace;

        [ImportingConstructor]
        public WorkspaceBufferListener(VisualStudioWorkspace workspace)
        {
            _workspace = workspace;
        }

        public void SubjectBuffersConnected(IWpfTextView textView, ConnectionReason reason, Collection<ITextBuffer> subjectBuffers)
        {
            foreach (var subjectBuffer in subjectBuffers)
                _workspace.OnSubjectBufferConnected(textView, subjectBuffer);
        }

        public void SubjectBuffersDisconnected(IWpfTextView textView, ConnectionReason reason, Collection<ITextBuffer> subjectBuffers)
        {
            foreach (var subjectBuffer in subjectBuffers)
                _workspace.OnSubjectBufferDisconnected(textView, subjectBuffer);
        }
    }
}
