using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using ShaderTools.Editor.VisualStudio.Core.Tagging;
using ShaderTools.Editor.VisualStudio.ShaderLab.Util.Extensions;

namespace ShaderTools.Editor.VisualStudio.ShaderLab.Tagging.Classification
{
    [Export(typeof(ITaggerProvider))]
    [TagType(typeof(IClassificationTag))]
    [ContentType(ShaderLabConstants.ContentTypeName)]
    internal sealed class SyntaxTaggerProvider : ITaggerProvider
    {
        [Import]
        public ShaderLabClassificationService ClassificationService { get; set; }

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            // Ensure package is loaded.
            // TODO: Don't do this.
            var package = ShaderLabPackage.Instance;

            var syntaxTagger = AsyncTaggerUtility.CreateTagger<SyntaxTagger, T>(buffer,
                () => new SyntaxTagger(ClassificationService, buffer.GetBackgroundParser()));

            return syntaxTagger;
        }
    }
}