using System.Collections.Generic;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using ShaderTools.VisualStudio.Core.Util;

namespace ShaderTools.VisualStudio.Core.Tagging.Classification
{
    // Uses some code from https://github.com/fsprojects/VisualFSharpPowerTools/blob/master/src/FSharpVSPowerTools/SyntaxConstructClassifierProvider.cs
    // to update colours when theme changes.
    internal abstract class ClassificationColorManagerBase
    {
        private readonly ThemeManager _themeManager;
        private readonly IClassificationFormatMapService _classificationFormatMapService;
        private readonly IClassificationTypeRegistryService _classificationTypeRegistryService;
        private readonly IDictionary<VisualStudioTheme, IDictionary<string, Color>> _themeColors;
        private VisualStudioTheme _lastTheme;

        protected ClassificationColorManagerBase(ThemeManager themeManager,
            IClassificationFormatMapService classificationFormatMapService,
            IClassificationTypeRegistryService classificationTypeRegistryService)
        {
            _themeManager = themeManager;
            _classificationFormatMapService = classificationFormatMapService;
            _classificationTypeRegistryService = classificationTypeRegistryService;
            _themeColors = new Dictionary<VisualStudioTheme, IDictionary<string, Color>>();
            _lastTheme = VisualStudioTheme.Unknown;

            // Light / Blue theme colors
            var lightAndBlueColors = CreateLightAndBlueColors();
            _themeColors.Add(VisualStudioTheme.Blue, lightAndBlueColors);
            _themeColors.Add(VisualStudioTheme.Light, lightAndBlueColors);
            _themeColors.Add(VisualStudioTheme.Unknown, lightAndBlueColors);

            // Dark theme colors

            var darkColors = CreateDarkColors();
            _themeColors.Add(VisualStudioTheme.Dark, darkColors);
        }

        protected abstract Dictionary<string, Color> CreateLightAndBlueColors();
        protected abstract Dictionary<string, Color> CreateDarkColors();

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