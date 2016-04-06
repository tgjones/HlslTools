using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Media;
using HlslTools.VisualStudio.Util;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Formatting;

namespace HlslTools.VisualStudio.Tagging.Classification
{
    [Export]
    internal sealed class ClassificationColorManager
    {
        private readonly ThemeManager _themeManager;
        private readonly IClassificationFormatMapService _classificationFormatMapService;
        private readonly IClassificationTypeRegistryService _classificationTypeRegistryService;
        private readonly IDictionary<VisualStudioTheme, IDictionary<string, Color>> _themeColors;
        private VisualStudioTheme _lastTheme;

        [ImportingConstructor]
        public ClassificationColorManager(ThemeManager themeManager,
            IClassificationFormatMapService classificationFormatMapService,
            IClassificationTypeRegistryService classificationTypeRegistryService)
        {
            _themeManager = themeManager;
            _classificationFormatMapService = classificationFormatMapService;
            _classificationTypeRegistryService = classificationTypeRegistryService;
            _themeColors = new Dictionary<VisualStudioTheme, IDictionary<string, Color>>();
            _lastTheme = VisualStudioTheme.Unknown;

            // Light / Blue theme colors
            var lightAndBlueColors = new Dictionary<string, Color>
            {
                { SemanticClassificationMetadata.PunctuationClassificationTypeName, Colors.Black },
                { SemanticClassificationMetadata.SemanticClassificationTypeName, Color.FromRgb(85, 107, 47) },
                { SemanticClassificationMetadata.PackOffsetClassificationTypeName, Colors.Purple },
                { SemanticClassificationMetadata.RegisterLocationClassificationTypeName, Colors.LightCoral },
                { SemanticClassificationMetadata.NamespaceClassificationTypeName, Colors.Black },
                { SemanticClassificationMetadata.GlobalVariableClassificationTypeName, Color.FromRgb(72, 61, 139) },
                { SemanticClassificationMetadata.FieldIdentifierClassificationTypeName, Color.FromRgb(139, 0, 139) },
                { SemanticClassificationMetadata.LocalVariableClassificationTypeName, Colors.Black },
                { SemanticClassificationMetadata.ConstantBufferVariableClassificationTypeName, Color.FromRgb(72, 61, 139) },
                { SemanticClassificationMetadata.ParameterClassificationTypeName, Colors.Black },
                { SemanticClassificationMetadata.FunctionClassificationTypeName, Color.FromRgb(0, 139, 139) },
                { SemanticClassificationMetadata.MethodClassificationTypeName, Color.FromRgb(0, 120, 180) },
                { SemanticClassificationMetadata.ClassIdentifierClassificationTypeName, Color.FromRgb(0, 0, 139) },
                { SemanticClassificationMetadata.StructIdentifierClassificationTypeName, Color.FromRgb(0, 0, 139) },
                { SemanticClassificationMetadata.InterfaceIdentifierClassificationTypeName, Color.FromRgb(0, 0, 139) },
                { SemanticClassificationMetadata.ConstantBufferIdentifierClassificationTypeName, Color.FromRgb(0, 0, 139) }
            };

            _themeColors.Add(VisualStudioTheme.Blue, lightAndBlueColors);
            _themeColors.Add(VisualStudioTheme.Light, lightAndBlueColors);
            _themeColors.Add(VisualStudioTheme.Unknown, lightAndBlueColors);

            // Dark theme colors

            var darkColors = new Dictionary<string, Color>
            {
                { SemanticClassificationMetadata.PunctuationClassificationTypeName, Colors.White },
                { SemanticClassificationMetadata.SemanticClassificationTypeName, Color.FromRgb(144, 238, 144) },
                { SemanticClassificationMetadata.PackOffsetClassificationTypeName, Colors.Pink },
                { SemanticClassificationMetadata.RegisterLocationClassificationTypeName, Colors.LightCoral },
                { SemanticClassificationMetadata.NamespaceClassificationTypeName, Colors.White },
                { SemanticClassificationMetadata.GlobalVariableClassificationTypeName, Color.FromRgb(173, 216, 230) },
                { SemanticClassificationMetadata.FieldIdentifierClassificationTypeName, Color.FromRgb(221, 160, 221) },
                { SemanticClassificationMetadata.LocalVariableClassificationTypeName, Color.FromRgb(220, 220, 220) },
                { SemanticClassificationMetadata.ConstantBufferVariableClassificationTypeName, Color.FromRgb(173, 216, 230) },
                { SemanticClassificationMetadata.ParameterClassificationTypeName, Color.FromRgb(220, 220, 220) },
                { SemanticClassificationMetadata.FunctionClassificationTypeName, Color.FromRgb(0, 255, 255) },
                { SemanticClassificationMetadata.MethodClassificationTypeName, Color.FromRgb(0, 220, 220) },
                { SemanticClassificationMetadata.ClassIdentifierClassificationTypeName, Color.FromRgb(173, 216, 230) },
                { SemanticClassificationMetadata.StructIdentifierClassificationTypeName, Color.FromRgb(173, 216, 230) },
                { SemanticClassificationMetadata.InterfaceIdentifierClassificationTypeName, Color.FromRgb(173, 216, 230) },
                { SemanticClassificationMetadata.ConstantBufferIdentifierClassificationTypeName, Color.FromRgb(173, 216, 230) }
            };

            _themeColors.Add(VisualStudioTheme.Dark, darkColors);
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
            var formatMap = _classificationFormatMapService.GetClassificationFormatMap(category: "text");

            try
            {
                formatMap.BeginBatchUpdate();
                foreach (var pair in colors)
                {
                    var type = pair.Key;
                    var color = pair.Value;

                    var classificationType = _classificationTypeRegistryService.GetClassificationType(type);
                    var oldProp = formatMap.GetTextProperties(classificationType);

                    var brush = new SolidColorBrush(color);

                    var newProp = TextFormattingRunProperties.CreateTextFormattingRunProperties(
                        brush, null, oldProp.Typeface, null, null, oldProp.TextDecorations,
                        oldProp.TextEffects, oldProp.CultureInfo);

                    formatMap.SetTextProperties(classificationType, newProp);
                }
            }
            finally
            {
                formatMap.EndBatchUpdate();
            }
        }
    }
}