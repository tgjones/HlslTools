using System;
using System.ComponentModel.Composition;
using HlslTools.VisualStudio.Tagging.Highlighting.Highlighters;
using HlslTools.VisualStudio.Util.Extensions;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace HlslTools.VisualStudio.Tagging.Highlighting
{
    [Export(typeof(IViewTaggerProvider))]
    [TagType(typeof(HighlightTag))]
    [ContentType(HlslConstants.ContentTypeName)]
    internal sealed class HighlightingTaggerProvider : IViewTaggerProvider
    {
        [Import]
        public HighlighterService HighlighterService { get; set; }

        [Import(typeof(SVsServiceProvider))]
        public IServiceProvider ServiceProvider { get; set; }

        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
            return AsyncTaggerUtility.CreateTagger<HighlightingTagger, T>(buffer,
                () => new HighlightingTagger(buffer.GetBackgroundParser(), textView, HighlighterService.Highlighters, ServiceProvider));
        }
    }
}