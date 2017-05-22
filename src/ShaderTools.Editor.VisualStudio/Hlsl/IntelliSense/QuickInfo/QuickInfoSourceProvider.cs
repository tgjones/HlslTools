using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using ShaderTools.CodeAnalysis.Editor.Shared.Utilities;
using ShaderTools.Editor.VisualStudio.Core.Glyphs;

namespace ShaderTools.Editor.VisualStudio.Hlsl.IntelliSense.QuickInfo
{
    //[Export(typeof(IQuickInfoSourceProvider))]
    //[Name("QuickInfoSourceProvider")]
    //[ContentType(HlslConstants.ContentTypeName)]
    internal sealed class QuickInfoSourceProvider : IQuickInfoSourceProvider
    {
        [Import]
        public IClassificationFormatMapService ClassificationFormatMapService { get; set; }

        [Import]
        public ClassificationTypeMap ClassificationTypeMap { get; set; }

        [Import]
        public DispatcherGlyphService DispatcherGlyphService { get; set; }

        public IQuickInfoSource TryCreateQuickInfoSource(ITextBuffer textBuffer)
        {
            return new QuickInfoSource(ClassificationFormatMapService, ClassificationTypeMap, DispatcherGlyphService);
        }
    }
}