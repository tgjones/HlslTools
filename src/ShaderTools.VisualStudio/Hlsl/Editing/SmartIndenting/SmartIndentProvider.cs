using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace ShaderTools.VisualStudio.Hlsl.Editing.SmartIndenting
{
    [Export(typeof(ISmartIndentProvider))]
    [ContentType(HlslConstants.ContentTypeName)]
    internal sealed class SmartIndentProvider : ISmartIndentProvider
    {
        public ISmartIndent CreateSmartIndent(ITextView textView)
        {
            return textView.Properties.GetOrCreateSingletonProperty(() => new SmartIndent(HlslPackage.Instance));
        }
    }
}