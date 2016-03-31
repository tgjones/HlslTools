using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;

namespace HlslTools.VisualStudio.Tagging.Highlighting
{
    [Export]
    internal sealed class HighlightingNavigationManagerProvider
    {
        [Import]
        public IViewTagAggregatorFactoryService AggregatorFactoryService { get; set; }

        public HighlightingNavigationManager GetHighlightingNavigationManager(ITextView textView)
        {
            return textView.Properties.GetOrCreateSingletonProperty(() =>
            {
                var tagAggregator = AggregatorFactoryService.CreateTagAggregator<HighlightTag>(textView);
                return new HighlightingNavigationManager(textView, tagAggregator);
            });
        }
    }
}