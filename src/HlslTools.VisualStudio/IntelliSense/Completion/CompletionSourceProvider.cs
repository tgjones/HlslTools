using System.ComponentModel.Composition;
using HlslTools.VisualStudio.Glyphs;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

namespace HlslTools.VisualStudio.IntelliSense.Completion
{
    //[Export(typeof(ICompletionSourceProvider))]
    [ContentType(HlslConstants.ContentTypeName)]
    internal sealed class CompletionSourceProvider : ICompletionSourceProvider
    {
        [Import]
        public DispatcherGlyphService GlyphService { get; set; }

        public ICompletionSource TryCreateCompletionSource(ITextBuffer textBuffer)
        {
            return new CompletionSource(GlyphService);
        }
    }
}