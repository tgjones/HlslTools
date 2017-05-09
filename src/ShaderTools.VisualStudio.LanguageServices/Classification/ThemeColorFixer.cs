using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.PlatformUI;
using ShaderTools.CodeAnalysis.Editor.Implementation.Classification;

namespace ShaderTools.VisualStudio.LanguageServices.Classification
{
    [Export]
    internal sealed class ThemeColorFixer : IDisposable
    {
        private readonly IClassificationColorManager _classificationColorManager;

        [ImportingConstructor]
        public ThemeColorFixer(IClassificationColorManager classificationColorManager)
        {
            _classificationColorManager = classificationColorManager;

            VSColorTheme.ThemeChanged += OnThemeChanged;
        }

        private void OnThemeChanged(ThemeChangedEventArgs e)
        {
            _classificationColorManager.UpdateColors();
        }

        public void Dispose()
        {
            VSColorTheme.ThemeChanged -= OnThemeChanged;
        }
    }
}
