using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Windows.Media;
using ShaderTools.CodeAnalysis.Hlsl.Classification;
using ShaderTools.VisualStudio.LanguageServices.Classification;

namespace ShaderTools.VisualStudio.LanguageServices.Hlsl.Classification
{
    [Export(typeof(IClassificationColorProvider))]
    internal sealed class HlslClassificationColorProvider : IClassificationColorProvider
    {
        public ImmutableDictionary<string, Color> LightAndBlueColors
        {
            get
            {
                return new Dictionary<string, Color>
                    {
                    { HlslClassificationTypeNames.Punctuation, Colors.Black },
                    { HlslClassificationTypeNames.Semantic, Color.FromRgb(85, 107, 47) },
                    { HlslClassificationTypeNames.PackOffset, Colors.Purple },
                    { HlslClassificationTypeNames.RegisterLocation, Colors.LightCoral },
                    { HlslClassificationTypeNames.NamespaceIdentifier, Colors.Black },
                    { HlslClassificationTypeNames.GlobalVariableIdentifier, Color.FromRgb(72, 61, 139) },
                    { HlslClassificationTypeNames.FieldIdentifier, Color.FromRgb(139, 0, 139) },
                    { HlslClassificationTypeNames.LocalVariableIdentifier, Colors.Black },
                    { HlslClassificationTypeNames.ConstantBufferVariableIdentifier, Color.FromRgb(72, 61, 139) },
                    { HlslClassificationTypeNames.ParameterIdentifier, Colors.Black },
                    { HlslClassificationTypeNames.FunctionIdentifier, Color.FromRgb(0, 139, 139) },
                    { HlslClassificationTypeNames.MethodIdentifier, Color.FromRgb(0, 139, 139) },
                    { HlslClassificationTypeNames.ClassIdentifier, Color.FromRgb(0, 0, 139) },
                    { HlslClassificationTypeNames.StructIdentifier, Color.FromRgb(0, 0, 139) },
                    { HlslClassificationTypeNames.InterfaceIdentifier, Color.FromRgb(0, 0, 139) },
                    { HlslClassificationTypeNames.ConstantBufferIdentifier, Color.FromRgb(0, 0, 139) },
                    { HlslClassificationTypeNames.MacroIdentifier, Color.FromRgb(111, 0, 138) },
                }.ToImmutableDictionary();
            }
        }

        public ImmutableDictionary<string, Color> DarkColors
        {
            get
            {
                return new Dictionary<string, Color>
                {
                    { HlslClassificationTypeNames.Punctuation, Colors.White },
                    { HlslClassificationTypeNames.Semantic, Color.FromRgb(144, 238, 144) },
                    { HlslClassificationTypeNames.PackOffset, Colors.Pink },
                    { HlslClassificationTypeNames.RegisterLocation, Colors.LightCoral },
                    { HlslClassificationTypeNames.NamespaceIdentifier, Colors.White },
                    { HlslClassificationTypeNames.GlobalVariableIdentifier, Color.FromRgb(173, 216, 230) },
                    { HlslClassificationTypeNames.FieldIdentifier, Color.FromRgb(221, 160, 221) },
                    { HlslClassificationTypeNames.LocalVariableIdentifier, Color.FromRgb(220, 220, 220) },
                    { HlslClassificationTypeNames.ConstantBufferVariableIdentifier, Color.FromRgb(173, 216, 230) },
                    { HlslClassificationTypeNames.ParameterIdentifier, Color.FromRgb(220, 220, 220) },
                    { HlslClassificationTypeNames.FunctionIdentifier, Color.FromRgb(0, 255, 255) },
                    { HlslClassificationTypeNames.MethodIdentifier, Color.FromRgb(0, 255, 255) },
                    { HlslClassificationTypeNames.ClassIdentifier, Color.FromRgb(173, 216, 230) },
                    { HlslClassificationTypeNames.StructIdentifier, Color.FromRgb(173, 216, 230) },
                    { HlslClassificationTypeNames.InterfaceIdentifier, Color.FromRgb(173, 216, 230) },
                    { HlslClassificationTypeNames.ConstantBufferIdentifier, Color.FromRgb(173, 216, 230) },
                    { HlslClassificationTypeNames.MacroIdentifier, Color.FromRgb(189, 99, 197) },
                }.ToImmutableDictionary();
            }
        }
    }
}
