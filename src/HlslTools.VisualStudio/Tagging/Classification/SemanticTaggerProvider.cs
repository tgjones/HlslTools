using System;
using System.ComponentModel.Composition;
using HlslTools.VisualStudio.Text;
using HlslTools.VisualStudio.Util.Extensions;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace HlslTools.VisualStudio.Tagging.Classification
{
    [Export(typeof(ITaggerProvider))]
    [TagType(typeof(IClassificationTag))]
    [ContentType(HlslConstants.ContentTypeName)]
    internal sealed class SemanticTaggerProvider : ITaggerProvider, IDisposable
    {
        private readonly HlslClassificationService _classificationService;
        private readonly ClassificationColorManager _classificationColorManager;
        private readonly ShellEventListener _shellEventListener;
        private readonly VisualStudioSourceTextFactory _sourceTextFactory;

        [ImportingConstructor]
        public SemanticTaggerProvider(HlslClassificationService classificationService, 
            ClassificationColorManager classificationColorManager,
            ShellEventListener shellEventListener,
            VisualStudioSourceTextFactory sourceTextFactory)
        {
            _classificationService = classificationService;
            _classificationColorManager = classificationColorManager;
            _shellEventListener = shellEventListener;
            _sourceTextFactory = sourceTextFactory;

            _shellEventListener.ThemeChanged += UpdateTheme;
        }

        private void UpdateTheme(object sender, EventArgs e)
        {
            _classificationColorManager.UpdateColors();
        }

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            return AsyncTaggerUtility.CreateTagger<SemanticTagger, T>(buffer,
                () => new SemanticTagger(_classificationService, buffer.GetBackgroundParser(_sourceTextFactory)),
                _sourceTextFactory);
        }

        public void Dispose()
        {
            _shellEventListener.ThemeChanged -= UpdateTheme;
        }
    }
}