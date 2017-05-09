using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using ShaderTools.CodeAnalysis.Editor.Implementation.Classification;

namespace ShaderTools.VisualStudio.LanguageServices.Classification
{
    [Export(typeof(IClassificationColorManager))]
    internal sealed class ClassificationColorManager : IClassificationColorManager
    {
        private readonly ThemeManager _themeManager;
        private readonly IClassificationFormatMapService _classificationFormatMapService;
        private readonly IClassificationTypeRegistryService _classificationTypeRegistryService;
        private readonly ImmutableDictionary<VisualStudioTheme, ImmutableDictionary<string, Color>> _themeColors;
        private VisualStudioTheme _lastTheme;

        [ImportingConstructor]
        public ClassificationColorManager(
            ThemeManager themeManager,
            IClassificationFormatMapService classificationFormatMapService,
            IClassificationTypeRegistryService classificationTypeRegistryService,
            [ImportMany] IEnumerable<IClassificationColorProvider> classificationColorProviders)
        {
            _themeManager = themeManager;
            _classificationFormatMapService = classificationFormatMapService;
            _classificationTypeRegistryService = classificationTypeRegistryService;
            _lastTheme = VisualStudioTheme.Unknown;

            var themeColors = ImmutableDictionary<VisualStudioTheme, ImmutableDictionary<string, Color>>.Empty.ToBuilder();

            void addToThemeColors(VisualStudioTheme theme, ImmutableDictionary<string, Color> newColors)
            {
                if (!themeColors.TryGetValue(theme, out var colors))
                    colors = ImmutableDictionary<string, Color>.Empty;

                colors = colors.AddRange(newColors);

                themeColors[theme] = colors;
            }

            foreach (var colorProvider in classificationColorProviders)
            {
                // Light / Blue theme colors
                var lightAndBlueColors = colorProvider.LightAndBlueColors;
                addToThemeColors(VisualStudioTheme.Blue, lightAndBlueColors);
                addToThemeColors(VisualStudioTheme.Light, lightAndBlueColors);
                addToThemeColors(VisualStudioTheme.Unknown, lightAndBlueColors);

                // Dark theme colors
                var darkColors = colorProvider.DarkColors;
                addToThemeColors(VisualStudioTheme.Dark, darkColors);
            }

            _themeColors = themeColors.ToImmutable();
        }

        public Color GetDefaultColor(string category)
        {
            var currentTheme = _themeManager.GetCurrentTheme();
            return _themeColors[currentTheme][category];
        }

        public void UpdateColors()
        {
            var currentTheme = _themeManager.GetCurrentTheme();

            if (currentTheme == VisualStudioTheme.Unknown || currentTheme == _lastTheme)
                return;

            _lastTheme = currentTheme;

            var colors = _themeColors[currentTheme];
            var formatMap = _classificationFormatMapService.GetClassificationFormatMap("text");

            try
            {
                formatMap.BeginBatchUpdate();
                foreach (var pair in colors)
                {
                    var type = pair.Key;
                    var color = pair.Value;

                    var classificationType = _classificationTypeRegistryService.GetClassificationType(type);

                    var props = formatMap.GetTextProperties(classificationType);
                    props = props.SetForeground(color);

                    formatMap.SetTextProperties(classificationType, props);
                }
            }
            finally
            {
                formatMap.EndBatchUpdate();
            }
        }
    }
}
