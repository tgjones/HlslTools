using System.ComponentModel.Composition;
using HlslTools.VisualStudio.Util.Extensions;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace HlslTools.VisualStudio.Editing.SmartIndenting
{
    [Export(typeof(ISmartIndentProvider))]
    [ContentType(HlslConstants.ContentTypeName)]
    internal sealed class SmartIndentProvider : ISmartIndentProvider
    {
        public ISmartIndent CreateSmartIndent(ITextView textView)
        {
            return textView.Properties.GetOrCreateSingletonProperty(() => new SmartIndent(HlslToolsPackage.Instance.AsVsServiceProvider()));
        }
    }
}