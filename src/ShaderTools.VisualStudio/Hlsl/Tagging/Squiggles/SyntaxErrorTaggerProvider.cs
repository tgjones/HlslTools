using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using ShaderTools.VisualStudio.Core.Tagging;
using ShaderTools.VisualStudio.Hlsl.Options;
using ShaderTools.VisualStudio.Hlsl.Util.Extensions;

namespace ShaderTools.VisualStudio.Hlsl.Tagging.Squiggles
{
    [Export(typeof(IViewTaggerProvider))]
    [TagType(typeof(IErrorTag))]
    [ContentType(HlslConstants.ContentTypeName)]
    internal sealed class SyntaxErrorTaggerProvider : IViewTaggerProvider
    {
        [Import]
        public IHlslOptionsService OptionsService { get; set; }

        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
            return AsyncTaggerUtility.CreateTagger<SyntaxErrorTagger, T>(buffer,
                () => new SyntaxErrorTagger(textView, buffer, buffer.GetBackgroundParser(), OptionsService));
        }
    }
}