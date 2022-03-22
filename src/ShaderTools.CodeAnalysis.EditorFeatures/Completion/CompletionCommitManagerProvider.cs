using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Text.Editor;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

namespace ShaderTools.CodeAnalysis.Editor.Completion
{
    [Export(typeof(IAsyncCompletionCommitManagerProvider))]
    [ContentType(ContentTypeNames.ShaderToolsContentType)]
    [Name(nameof(CompletionCommitManagerProvider))]
    internal sealed class CompletionCommitManagerProvider : IAsyncCompletionCommitManagerProvider
    {
        public IAsyncCompletionCommitManager GetOrCreate(ITextView textView)
        {
            return textView.Properties.GetOrCreateSingletonProperty(() => new CompletionCommitManager());
        }
    }
}
