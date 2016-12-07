using System;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

namespace ShaderTools.VisualStudio.Core.Tagging.Classification
{
    internal abstract class SemanticTaggerProviderBase : ITaggerProvider, IDisposable
    {
        private readonly ClassificationColorManagerBase _classificationColorManager;

        protected SemanticTaggerProviderBase(ClassificationColorManagerBase classificationColorManager)
        {
            _classificationColorManager = classificationColorManager;

            VSColorTheme.ThemeChanged += UpdateTheme;
        }

        private void UpdateTheme(ThemeChangedEventArgs e)
        {
            _classificationColorManager.UpdateColors();
        }

        public abstract ITagger<T> CreateTagger<T>(ITextBuffer buffer)
            where T : ITag;

        public void Dispose()
        {
            VSColorTheme.ThemeChanged -= UpdateTheme;
        }
    }
}