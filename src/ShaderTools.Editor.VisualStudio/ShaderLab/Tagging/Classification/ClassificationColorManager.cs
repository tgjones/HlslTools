using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using ShaderTools.Editor.VisualStudio.Core.Tagging.Classification;
using ShaderTools.Editor.VisualStudio.Core.Util;

namespace ShaderTools.Editor.VisualStudio.ShaderLab.Tagging.Classification
{
    [Export]
    internal sealed class ClassificationColorManager : ClassificationColorManagerBase
    {
        [ImportingConstructor]
        public ClassificationColorManager(ThemeManager themeManager, IClassificationFormatMapService classificationFormatMapService, IClassificationTypeRegistryService classificationTypeRegistryService)
            : base(themeManager, classificationFormatMapService, classificationTypeRegistryService)
        {
        }

        protected override Dictionary<string, Color> CreateLightAndBlueColors()
        {
            return new Dictionary<string, Color>
            {
                { SemanticClassificationMetadata.PunctuationClassificationTypeName, Colors.Black },
                { SemanticClassificationMetadata.ShaderPropertyClassificationTypeName, Color.FromRgb(139, 0, 139) },
                { SemanticClassificationMetadata.AttributeClassificationTypeName, Color.FromRgb(0, 0, 139) }
            };
        }

        protected override Dictionary<string, Color> CreateDarkColors()
        {
            return new Dictionary<string, Color>
            {
                { SemanticClassificationMetadata.PunctuationClassificationTypeName, Colors.White },
                { SemanticClassificationMetadata.ShaderPropertyClassificationTypeName, Color.FromRgb(221, 160, 221) },
                { SemanticClassificationMetadata.AttributeClassificationTypeName, Color.FromRgb(173, 216, 230) }
            };
        }
    }
}