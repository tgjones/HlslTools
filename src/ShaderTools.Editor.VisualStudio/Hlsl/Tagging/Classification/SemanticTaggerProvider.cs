using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using ShaderTools.Editor.VisualStudio.Core.Tagging;
using ShaderTools.Editor.VisualStudio.Core.Tagging.Classification;
using ShaderTools.Editor.VisualStudio.Hlsl.Util.Extensions;

namespace ShaderTools.Editor.VisualStudio.Hlsl.Tagging.Classification
{
    [Export(typeof(ITaggerProvider))]
    [TagType(typeof(IClassificationTag))]
    [ContentType(HlslConstants.ContentTypeName)]
    internal sealed class SemanticTaggerProvider : SemanticTaggerProviderBase
    {
        private readonly HlslClassificationService _classificationService;

        [ImportingConstructor]
        public SemanticTaggerProvider(HlslClassificationService classificationService, 
            ClassificationColorManager classificationColorManager)
            : base(classificationColorManager)
        {
            _classificationService = classificationService;
        }

        public override ITagger<T> CreateTagger<T>(ITextBuffer buffer)
        {
            return AsyncTaggerUtility.CreateTagger<SemanticTagger, T>(buffer,
                () => new SemanticTagger(_classificationService, buffer.GetBackgroundParser()));
        }
    }
}