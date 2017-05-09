
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using ShaderTools.CodeAnalysis.Editor.Implementation.Classification;
using ShaderTools.CodeAnalysis.ShaderLab.Classification;

namespace ShaderTools.CodeAnalysis.ShaderLab.EditorFeatures.Classification
{
    internal sealed class ClassificationTypeDefinitions
    {
        [Export]
        [Name(ShaderLabClassificationTypeNames.Punctuation)]
        [BaseDefinition(PredefinedClassificationTypeNames.FormalLanguage)]
        public ClassificationTypeDefinition PunctuationType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [Name(ShaderLabClassificationTypeNames.Punctuation)]
        [ClassificationType(ClassificationTypeNames = ShaderLabClassificationTypeNames.Punctuation)]
        [UserVisible(true)]
        [Order(After = PredefinedClassificationTypeNames.String)]
        public sealed class PunctuationFormat : ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public PunctuationFormat(IClassificationColorManager colorManager)
            {
                DisplayName = "ShaderLab Punctuation";
                ForegroundColor = colorManager.GetDefaultColor(ShaderLabClassificationTypeNames.Punctuation);
            }
        }

        [Export]
        [Name(ShaderLabClassificationTypeNames.ShaderProperty)]
        [BaseDefinition(PredefinedClassificationTypeNames.FormalLanguage)]
        public ClassificationTypeDefinition ShaderPropertyType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [Name(ShaderLabClassificationTypeNames.ShaderProperty)]
        [ClassificationType(ClassificationTypeNames = ShaderLabClassificationTypeNames.ShaderProperty)]
        [UserVisible(true)]
        [Order(After = PredefinedClassificationTypeNames.String)]
        public sealed class ShaderPropertyFormat : ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public ShaderPropertyFormat(IClassificationColorManager colorManager)
            {
                DisplayName = "ShaderLab Shader Property";
                ForegroundColor = colorManager.GetDefaultColor(ShaderLabClassificationTypeNames.ShaderProperty);
            }
        }

        [Export]
        [Name(ShaderLabClassificationTypeNames.Attribute)]
        [BaseDefinition(PredefinedClassificationTypeNames.FormalLanguage)]
        public ClassificationTypeDefinition AttributeType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [Name(ShaderLabClassificationTypeNames.Attribute)]
        [ClassificationType(ClassificationTypeNames = ShaderLabClassificationTypeNames.Attribute)]
        [UserVisible(true)]
        [Order(After = PredefinedClassificationTypeNames.String)]
        public sealed class AttributeFormat : ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public AttributeFormat(IClassificationColorManager colorManager)
            {
                DisplayName = "ShaderLab Attribute";
                ForegroundColor = colorManager.GetDefaultColor(ShaderLabClassificationTypeNames.Attribute);
            }
        }
    }
}
