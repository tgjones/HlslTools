using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;
using ShaderTools.VisualStudio.Hlsl.Glyphs;

namespace ShaderTools.VisualStudio.Hlsl.IntelliSense.Completion
{
    [Export(typeof(ICompletionSourceProvider))]
    [ContentType(HlslConstants.ContentTypeName)]
    [Name(nameof(CompletionSourceProvider))]
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