using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using ShaderTools.VisualStudio.Core.Tagging;
using ShaderTools.VisualStudio.Core.Util.Extensions;
using ShaderTools.VisualStudio.Hlsl.Util.Extensions;

namespace ShaderTools.VisualStudio.Hlsl.Tagging.Classification
{
    [Export(typeof(ITaggerProvider))]
    [TagType(typeof(IClassificationTag))]
    [ContentType(HlslConstants.ContentTypeName)]
    internal sealed class SyntaxTaggerProvider : ITaggerProvider
    {
        private static bool _languagePackageLoaded;

        [Import]
        public SVsServiceProvider GlobalServiceProvider { get; private set; }

        [Import]
        public HlslClassificationService ClassificationService { get; set; }

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            if (!_languagePackageLoaded)
            {
                var languagePackage = GlobalServiceProvider.GetShell().LoadPackage<HlslToolsPackage>();
                _languagePackageLoaded = languagePackage != null;
            }

            var syntaxTagger = AsyncTaggerUtility.CreateTagger<SyntaxTagger, T>(buffer,
                () => new SyntaxTagger(ClassificationService, buffer.GetBackgroundParser()));

            return syntaxTagger;
        }
    }
}