using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using ShaderTools.Editor.VisualStudio.Core.Tagging;
using ShaderTools.Editor.VisualStudio.Hlsl.Tagging.Highlighting.Highlighters;
using ShaderTools.Editor.VisualStudio.Hlsl.Util.Extensions;

namespace ShaderTools.Editor.VisualStudio.Hlsl.Tagging.Highlighting
{
    [Export(typeof(IViewTaggerProvider))]
    [TagType(typeof(NavigableHighlightTag))]
    [ContentType(HlslConstants.ContentTypeName)]
    internal sealed class HighlightingTaggerProvider : IViewTaggerProvider
    {
        [Import]
        public HighlighterService HighlighterService { get; set; }

        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
            return AsyncTaggerUtility.CreateTagger<HighlightingTagger, T>(buffer,
                () => new HighlightingTagger(buffer, buffer.GetBackgroundParser(), textView, HighlighterService.Highlighters));
        }
    }
}