using System.ComponentModel.Composition;
using HlslTools.VisualStudio.IntelliSense.Completion;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace HlslTools.VisualStudio.IntelliSense
{
    //[Export(typeof(IKeyProcessorProvider))]
    [Name("KeyProcessorProvider")]
    [ContentType(HlslConstants.ContentTypeName)]
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    internal sealed class KeyProcessorProvider : IKeyProcessorProvider
    {
        [Import]
        public IIntellisenseSessionStackMapService IntellisenseSessionStackMapService { get; set; }

        [Import]
        public CompletionModelManagerProvider CompletionModelManagerProvider { get; set; }

        public KeyProcessor GetAssociatedProcessor(IWpfTextView wpfTextView)
        {
            return wpfTextView.Properties.GetOrCreateSingletonProperty(() =>
            {
                var completionModelManager = CompletionModelManagerProvider.GetCompletionModel(wpfTextView);
                return new HlslKeyProcessor(wpfTextView, IntellisenseSessionStackMapService, completionModelManager);
            });
        }
    }
}