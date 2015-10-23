using System.ComponentModel.Composition;
using HlslTools.VisualStudio.IntelliSense.Completion.CompletionProviders;
using HlslTools.VisualStudio.Text;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text.Editor;

namespace HlslTools.VisualStudio.IntelliSense.Completion
{
    [Export]
    internal sealed class CompletionModelManagerProvider
    {
        [Import]
        public ICompletionBroker CompletionBroker { get; set; }

        [Import]
        public CompletionProviderService CompletionProviderService { get; set; }

        [Import]
        public VisualStudioSourceTextFactory VisualStudioSourceTextFactory { get; set; }

        public CompletionModelManager GetCompletionModel(ITextView textView)
        {
            return textView.Properties.GetOrCreateSingletonProperty(() => new CompletionModelManager(textView, CompletionBroker, CompletionProviderService, VisualStudioSourceTextFactory));
        }
    }
}