using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using ShaderTools.VisualStudio.Core.Tagging;
using ShaderTools.VisualStudio.ShaderLab.Util.Extensions;

namespace ShaderTools.VisualStudio.ShaderLab.Tagging.Classification
{
    [Export(typeof(ITaggerProvider))]
    [TagType(typeof(IClassificationTag))]
    [ContentType(ShaderLabConstants.ContentTypeName)]
    internal sealed class SyntaxTaggerProvider : ITaggerProvider
    {
        private static bool _languagePackageLoaded;

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