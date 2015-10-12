using System.ComponentModel.Composition;
using HlslTools.VisualStudio.Text;
using HlslTools.VisualStudio.Util.Extensions;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace HlslTools.VisualStudio.Tagging.BraceMatching
{
    [Export(typeof(IViewTaggerProvider))]
    [TagType(typeof(ITextMarkerTag))]
    [ContentType(HlslConstants.ContentTypeName)]
    internal sealed class BraceMatchingTaggerProvider : IViewTaggerProvider
    {
        [Import]
        public BraceMatcher BraceMatcher { get; set; }

        [Import]
        public VisualStudioSourceTextFactory SourceTextFactory { get; set; }

        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
            return AsyncTaggerUtility.CreateTagger<BraceMatchingTagger, T>(buffer,
                () => new BraceMatchingTagger(buffer.GetBackgroundParser(SourceTextFactory), textView, BraceMatcher),
                SourceTextFactory);
        }
    }
}