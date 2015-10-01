using System.ComponentModel.Composition;
using HlslTools.VisualStudio.ErrorList;
using HlslTools.VisualStudio.Options;
using HlslTools.VisualStudio.Util.Extensions;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace HlslTools.VisualStudio.Tagging.Squiggles
{
    [Export(typeof(IViewTaggerProvider))]
    [TagType(typeof(IErrorTag))]
    [ContentType(HlslConstants.ContentTypeName)]
    internal sealed class SyntaxErrorTaggerProvider : IViewTaggerProvider
    {
        [Import]
        public IOptionsService OptionsService { get; set; }

        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
            var errorListHelper = buffer.GetErrorListHelper();
            if (errorListHelper == null)
                return null;

            return AsyncTaggerUtility.CreateTagger<SyntaxErrorTagger, T>(buffer,
                () => new SyntaxErrorTagger(textView, buffer.GetBackgroundParser(), errorListHelper, OptionsService));
        }
    }
}