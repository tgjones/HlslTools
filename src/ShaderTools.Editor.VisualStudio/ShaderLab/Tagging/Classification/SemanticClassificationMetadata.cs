using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace ShaderTools.Editor.VisualStudio.ShaderLab.Tagging.Classification
{
    internal sealed class SemanticClassificationMetadata
    {
        public const string PunctuationClassificationTypeName = "ShaderLab.Punctuation";
        public const string ShaderPropertyClassificationTypeName = "ShaderLab.ShaderProperty";
        public const string AttributeClassificationTypeName = "ShaderLab.Attribute";

#pragma warning disable 649

        [Export]
        [Name(PunctuationClassificationTypeName)]
        [BaseDefinition(PredefinedClassificationTypeNames.FormalLanguage)]
        public ClassificationTypeDefinition PunctuationType;

        [Export]
        [Name(ShaderPropertyClassificationTypeName)]
        [BaseDefinition(PredefinedClassificationTypeNames.FormalLanguage)]
        public ClassificationTypeDefinition ShaderPropertyType;

        [Export]
        [Name(AttributeClassificationTypeName)]
        [BaseDefinition(PredefinedClassificationTypeNames.FormalLanguage)]
        public ClassificationTypeDefinition AttributeType;

#pragma warning restore 649

        [Export(typeof(EditorFormatDefinition))]
        [Name(PunctuationClassificationTypeName)]
        [ClassificationType(ClassificationTypeNames = PunctuationClassificationTypeName)]
        [UserVisible(true)]
        [Order(After = PredefinedClassificationTypeNames.String)]
        public sealed class PunctuationFormat : ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public PunctuationFormat(ClassificationColorManager colorManager)
            {
                DisplayName = "ShaderLab Punctuation";
                ForegroundColor = colorManager.GetDefaultColor(PunctuationClassificationTypeName);
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [Name(ShaderPropertyClassificationTypeName)]
        [ClassificationType(ClassificationTypeNames = ShaderPropertyClassificationTypeName)]
        [UserVisible(true)]
        [Order(After = PredefinedClassificationTypeNames.String)]
        public sealed class ShaderPropertyFormat : ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public ShaderPropertyFormat(ClassificationColorManager colorManager)
            {
                DisplayName = "ShaderLab Shader Property";
                ForegroundColor = colorManager.GetDefaultColor(ShaderPropertyClassificationTypeName);
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [Name(AttributeClassificationTypeName)]
        [ClassificationType(ClassificationTypeNames = AttributeClassificationTypeName)]
        [UserVisible(true)]
        [Order(After = PredefinedClassificationTypeNames.String)]
        public sealed class AttributeFormat : ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public AttributeFormat(ClassificationColorManager colorManager)
            {
                DisplayName = "ShaderLab Attribute";
                ForegroundColor = colorManager.GetDefaultColor(AttributeClassificationTypeName);
            }
        }
    }
}