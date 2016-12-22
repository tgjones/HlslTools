using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text.Editor;
using ShaderTools.Editor.VisualStudio.Hlsl.IntelliSense.QuickInfo.QuickInfoModelProviders;

namespace ShaderTools.Editor.VisualStudio.Hlsl.IntelliSense.QuickInfo
{
    [Export]
    internal sealed class QuickInfoManagerProvider
    {
        [Import]
        public IQuickInfoBroker QuickInfoBroker { get; set; }

        [Import]
        public QuickInfoModelProviderService QuickInfoModelProviderService { get; set; }

        public QuickInfoManager GetQuickInfoManager(ITextView textView)
        {
            return textView.Properties.GetOrCreateSingletonProperty(() => new QuickInfoManager(textView, QuickInfoBroker, QuickInfoModelProviderService));
        }
    }
}