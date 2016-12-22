using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using ShaderTools.Editor.VisualStudio.Core.Tagging;
using ShaderTools.Editor.VisualStudio.Hlsl.Options;
using ShaderTools.Editor.VisualStudio.Hlsl.Util.Extensions;

namespace ShaderTools.Editor.VisualStudio.Hlsl.Tagging.Squiggles
{
    [Export(typeof(IViewTaggerProvider))]
    [TagType(typeof(IErrorTag))]
    [ContentType(HlslConstants.ContentTypeName)]
    internal sealed class SemanticErrorTaggerProvider : IViewTaggerProvider
    {
        [Import]
        public IHlslOptionsService OptionsService { get; set; }

        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer)
            where T : ITag
        {
            return AsyncTaggerUtility.CreateTagger<SemanticErrorTagger, T>(buffer,
                () => new SemanticErrorTagger(textView, buffer, buffer.GetBackgroundParser(), OptionsService));
        }
    }
}