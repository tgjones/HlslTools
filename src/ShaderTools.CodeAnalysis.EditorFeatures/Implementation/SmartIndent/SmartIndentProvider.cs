using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace ShaderTools.CodeAnalysis.Editor.Implementation.SmartIndent
{
    [Export(typeof(ISmartIndentProvider))]
    [ContentType(ContentTypeNames.ShaderToolsContentType)]
    internal sealed class SmartIndentProvider : ISmartIndentProvider
    {
        public ISmartIndent CreateSmartIndent(ITextView textView)
        {
            return textView.Properties.GetOrCreateSingletonProperty(() => new SmartIndent(textView));
        }
    }
}