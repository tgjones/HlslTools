using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

namespace HlslTools.VisualStudio.IntelliSense.QuickInfo
{
    [Export(typeof(IQuickInfoSourceProvider))]
    [Name("QuickInfoSourceProvider")]
    [ContentType(HlslConstants.ContentTypeName)]
    internal sealed class QuickInfoSourceProvider : IQuickInfoSourceProvider
    {
        public IQuickInfoSource TryCreateQuickInfoSource(ITextBuffer textBuffer)
        {
            return new QuickInfoSource();
        }
    }
}