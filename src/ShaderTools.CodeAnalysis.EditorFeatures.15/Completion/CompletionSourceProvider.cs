using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace ShaderTools.CodeAnalysis.Editor.Completion
{
    [Export(typeof(IAsyncCompletionSourceProvider))]
    [ContentType(ContentTypeNames.ShaderToolsContentType)]
    [Name(nameof(CompletionSourceProvider))]
    internal sealed class CompletionSourceProvider : IAsyncCompletionSourceProvider
    {
        public IAsyncCompletionSource GetOrCreate(ITextView textView)
        {
            return textView.Properties.GetOrCreateSingletonProperty(() => new CompletionSource(textView));
        }
    }
}
