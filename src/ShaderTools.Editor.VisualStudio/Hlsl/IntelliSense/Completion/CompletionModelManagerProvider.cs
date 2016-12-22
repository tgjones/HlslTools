using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text.Editor;
using ShaderTools.Editor.VisualStudio.Hlsl.IntelliSense.Completion.CompletionProviders;

namespace ShaderTools.Editor.VisualStudio.Hlsl.IntelliSense.Completion
{
    [Export]
    internal sealed class CompletionModelManagerProvider
    {
        [Import]
        public ICompletionBroker CompletionBroker { get; set; }

        [Import]
        public CompletionProviderService CompletionProviderService { get; set; }

        public CompletionModelManager GetCompletionModel(ITextView textView)
        {
            return textView.Properties.GetOrCreateSingletonProperty(() => new CompletionModelManager(textView, CompletionBroker, CompletionProviderService));
        }
    }
}