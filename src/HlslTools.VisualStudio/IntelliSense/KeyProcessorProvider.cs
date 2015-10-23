using System.ComponentModel.Composition;
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

        public KeyProcessor GetAssociatedProcessor(IWpfTextView wpfTextView)
        {
            return wpfTextView.Properties.GetOrCreateSingletonProperty(() => new HlslKeyProcessor(wpfTextView, IntellisenseSessionStackMapService));
        }
    }
}