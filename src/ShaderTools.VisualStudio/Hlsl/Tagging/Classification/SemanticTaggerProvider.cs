using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using ShaderTools.VisualStudio.Core.Tagging;
using ShaderTools.VisualStudio.Hlsl.Util.Extensions;

namespace ShaderTools.VisualStudio.Hlsl.Tagging.Classification
{
    [Export(typeof(ITaggerProvider))]
    [TagType(typeof(IClassificationTag))]
    [ContentType(HlslConstants.ContentTypeName)]
    internal sealed class SemanticTaggerProvider : ITaggerProvider, IDisposable
    {
        private readonly HlslClassificationService _classificationService;
        private readonly ClassificationColorManager _classificationColorManager;

        [ImportingConstructor]
        public SemanticTaggerProvider(HlslClassificationService classificationService, 
            ClassificationColorManager classificationColorManager)
        {
            _classificationService = classificationService;
            _classificationColorManager = classificationColorManager;

            VSColorTheme.ThemeChanged += UpdateTheme;
        }

        private void UpdateTheme(ThemeChangedEventArgs e)
        {
            _classificationColorManager.UpdateColors();
        }

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            return AsyncTaggerUtility.CreateTagger<SemanticTagger, T>(buffer,
                () => new SemanticTagger(_classificationService, buffer.GetBackgroundParser()));
        }

        public void Dispose()
        {
            VSColorTheme.ThemeChanged -= UpdateTheme;
        }
    }
}