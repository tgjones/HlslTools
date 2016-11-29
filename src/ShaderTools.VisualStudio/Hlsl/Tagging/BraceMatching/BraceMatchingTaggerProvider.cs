using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using ShaderTools.VisualStudio.Hlsl.Util.Extensions;

namespace ShaderTools.VisualStudio.Hlsl.Tagging.BraceMatching
{
    [Export(typeof(IViewTaggerProvider))]
    [TagType(typeof(ITextMarkerTag))]
    [ContentType(HlslConstants.ContentTypeName)]
    internal sealed class BraceMatchingTaggerProvider : IViewTaggerProvider
    {
        [Import]
        public BraceMatcher BraceMatcher { get; set; }

        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
            return AsyncTaggerUtility.CreateTagger<BraceMatchingTagger, T>(buffer,
                () => new BraceMatchingTagger(buffer.GetBackgroundParser(), textView, BraceMatcher));
        }
    }
}