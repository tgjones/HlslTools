using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using ShaderTools.Editor.VisualStudio.Core.Tagging.Classification;
using ShaderTools.Editor.VisualStudio.Core.Util;

namespace ShaderTools.Editor.VisualStudio.Hlsl.Tagging.Classification
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
                { SemanticClassificationMetadata.MethodClassificationTypeName, Color.FromRgb(0, 139, 139) },
                { SemanticClassificationMetadata.ClassIdentifierClassificationTypeName, Color.FromRgb(0, 0, 139) },
                { SemanticClassificationMetadata.StructIdentifierClassificationTypeName, Color.FromRgb(0, 0, 139) },
                { SemanticClassificationMetadata.InterfaceIdentifierClassificationTypeName, Color.FromRgb(0, 0, 139) },
                { SemanticClassificationMetadata.ConstantBufferIdentifierClassificationTypeName, Color.FromRgb(0, 0, 139) }
            };
        }

        protected override Dictionary<string, Color> CreateDarkColors()
        {
            return new Dictionary<string, Color>
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
                { SemanticClassificationMetadata.MethodClassificationTypeName, Color.FromRgb(0, 255, 255) },
                { SemanticClassificationMetadata.ClassIdentifierClassificationTypeName, Color.FromRgb(173, 216, 230) },
                { SemanticClassificationMetadata.StructIdentifierClassificationTypeName, Color.FromRgb(173, 216, 230) },
                { SemanticClassificationMetadata.InterfaceIdentifierClassificationTypeName, Color.FromRgb(173, 216, 230) },
                { SemanticClassificationMetadata.ConstantBufferIdentifierClassificationTypeName, Color.FromRgb(173, 216, 230) }
            };
        }
    }
}