using System.ComponentModel.Composition;
using HlslTools.VisualStudio.Options;
using HlslTools.VisualStudio.Text;
using HlslTools.VisualStudio.Util.Extensions;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace HlslTools.VisualStudio.Tagging.Outlining
{
    [Export(typeof(ITaggerProvider))]
    [Export(typeof(OutliningTaggerProvider))]
    [ContentType(HlslConstants.ContentTypeName)]
    [TagType(typeof(IOutliningRegionTag))]
    internal sealed class OutliningTaggerProvider : ITaggerProvider
    {
        [Import]
        public IOptionsService OptionsService { get; set; }

        [Import]
        public VisualStudioSourceTextFactory SourceTextFactory { get; set; }

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            return AsyncTaggerUtility.CreateTagger<OutliningTagger, T>(buffer,
                () => new OutliningTagger(buffer.GetBackgroundParser(SourceTextFactory), OptionsService),
                SourceTextFactory);
        }
    }
}