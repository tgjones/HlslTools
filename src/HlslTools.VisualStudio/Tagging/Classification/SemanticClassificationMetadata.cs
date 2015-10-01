using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace HlslTools.VisualStudio.Tagging.Classification
{
    // Uses some code from https://github.com/fsprojects/VisualFSharpPowerTools/blob/master/src/FSharpVSPowerTools/SyntaxConstructClassifierProvider.cs
    // to update colours when theme changes.
    internal sealed class SemanticClassificationMetadata
    {
        public const string PunctuationClassificationTypeName = "Hlsl.Punctuation";
        public const string SemanticClassificationTypeName = "Hlsl.Semantic";
        public const string PackOffsetClassificationTypeName = "Hlsl.PackOffset";
        public const string RegisterLocationClassificationTypeName = "Hlsl.RegisterLocation";
        public const string GlobalVariableClassificationTypeName = "Hlsl.GlobalVariable";
        public const string FieldIdentifierClassificationTypeName = "Hlsl.Field";
        public const string LocalVariableClassificationTypeName = "Hlsl.LocalVariable";
        public const string ParameterClassificationTypeName = "Hlsl.Parameter";
        public const string FunctionClassificationTypeName = "Hlsl.Function";
        public const string ClassIdentifierClassificationTypeName = "Hlsl.Class";

#pragma warning disable 649

        [Export]
        [Name(PunctuationClassificationTypeName)]
        [BaseDefinition(PredefinedClassificationTypeNames.FormalLanguage)]
        public ClassificationTypeDefinition PunctuationType;

        [Export]
        [Name(SemanticClassificationTypeName)]
        [BaseDefinition(PredefinedClassificationTypeNames.FormalLanguage)]
        public ClassificationTypeDefinition SemanticType;

        [Export]
        [Name(PackOffsetClassificationTypeName)]
        [BaseDefinition(PredefinedClassificationTypeNames.FormalLanguage)]
        public ClassificationTypeDefinition PackOffsetType;

        [Export]
        [Name(RegisterLocationClassificationTypeName)]
        [BaseDefinition(PredefinedClassificationTypeNames.FormalLanguage)]
        public ClassificationTypeDefinition RegisterLocationType;

        [Export]
        [Name(GlobalVariableClassificationTypeName)]
        [BaseDefinition(PredefinedClassificationTypeNames.FormalLanguage)]
        public ClassificationTypeDefinition GlobalVariableIdentifierType;

        [Export]
        [Name(FieldIdentifierClassificationTypeName)]
        [BaseDefinition(PredefinedClassificationTypeNames.FormalLanguage)]
        public ClassificationTypeDefinition FieldIdentifierType;

        [Export]
        [Name(LocalVariableClassificationTypeName)]
        [BaseDefinition(PredefinedClassificationTypeNames.FormalLanguage)]
        public ClassificationTypeDefinition LocalVariableIdentifierType;

        [Export]
        [Name(ParameterClassificationTypeName)]
        [BaseDefinition(PredefinedClassificationTypeNames.FormalLanguage)]
        public ClassificationTypeDefinition ParameterIdentifierType;

        [Export]
        [Name(FunctionClassificationTypeName)]
        [BaseDefinition(PredefinedClassificationTypeNames.FormalLanguage)]
        public ClassificationTypeDefinition FunctionIdentifierType;

        [Export]
        [Name(ClassIdentifierClassificationTypeName)]
        [BaseDefinition(PredefinedClassificationTypeNames.FormalLanguage)]
        public ClassificationTypeDefinition ClassIdentifierType;

#pragma warning restore 649

        [Export(typeof(EditorFormatDefinition))]
        [Name(PunctuationClassificationTypeName)]
        [ClassificationType(ClassificationTypeNames = PunctuationClassificationTypeName)]
        [UserVisible(true)]
        public sealed class PunctuationFormat : ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public PunctuationFormat(ClassificationColorManager colorManager)
            {
                DisplayName = "HLSL Punctuation";
                ForegroundColor = colorManager.GetDefaultColor(PunctuationClassificationTypeName);
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [Name(SemanticClassificationTypeName)]
        [ClassificationType(ClassificationTypeNames = SemanticClassificationTypeName)]
        [UserVisible(true)]
        public sealed class SemanticFormat : ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public SemanticFormat(ClassificationColorManager colorManager)
            {
                DisplayName = "HLSL Semantic";
                ForegroundColor = colorManager.GetDefaultColor(SemanticClassificationTypeName);
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [Name(PackOffsetClassificationTypeName)]
        [ClassificationType(ClassificationTypeNames = PackOffsetClassificationTypeName)]
        [UserVisible(true)]
        public sealed class PackOffsetFormat : ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public PackOffsetFormat(ClassificationColorManager colorManager)
            {
                DisplayName = "HLSL Pack Offset";
                ForegroundColor = colorManager.GetDefaultColor(PackOffsetClassificationTypeName);
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [Name(RegisterLocationClassificationTypeName)]
        [ClassificationType(ClassificationTypeNames = RegisterLocationClassificationTypeName)]
        [UserVisible(true)]
        public sealed class RegisterLocationFormat : ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public RegisterLocationFormat(ClassificationColorManager colorManager)
            {
                DisplayName = "HLSL Register Location";
                ForegroundColor = colorManager.GetDefaultColor(RegisterLocationClassificationTypeName);
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [Name(GlobalVariableClassificationTypeName)]
        [ClassificationType(ClassificationTypeNames = GlobalVariableClassificationTypeName)]
        [UserVisible(true)]
        public sealed class GlobalVariableIdentifierFormat : ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public GlobalVariableIdentifierFormat(ClassificationColorManager colorManager)
            {
                DisplayName = "HLSL Global Variable Identifier";
                ForegroundColor = colorManager.GetDefaultColor(GlobalVariableClassificationTypeName);
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [Name(FieldIdentifierClassificationTypeName)]
        [ClassificationType(ClassificationTypeNames = FieldIdentifierClassificationTypeName)]
        [UserVisible(true)]
        public sealed class FieldIdentifierFormat : ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public FieldIdentifierFormat(ClassificationColorManager colorManager)
            {
                DisplayName = "HLSL Field Identifier";
                ForegroundColor = colorManager.GetDefaultColor(FieldIdentifierClassificationTypeName);
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [Name(LocalVariableClassificationTypeName)]
        [ClassificationType(ClassificationTypeNames = LocalVariableClassificationTypeName)]
        [UserVisible(true)]
        public sealed class LocalVariableIdentifierFormat : ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public LocalVariableIdentifierFormat(ClassificationColorManager colorManager)
            {
                DisplayName = "HLSL Local Variable Identifier";
                ForegroundColor = colorManager.GetDefaultColor(LocalVariableClassificationTypeName);
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [Name(ParameterClassificationTypeName)]
        [ClassificationType(ClassificationTypeNames = ParameterClassificationTypeName)]
        [UserVisible(true)]
        public sealed class ParameterIdentifierFormat : ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public ParameterIdentifierFormat(ClassificationColorManager colorManager)
            {
                DisplayName = "HLSL Parameter Variable Identifier";
                ForegroundColor = colorManager.GetDefaultColor(ParameterClassificationTypeName);
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [Name(FunctionClassificationTypeName)]
        [ClassificationType(ClassificationTypeNames = FunctionClassificationTypeName)]
        [UserVisible(true)]
        public sealed class FunctionIdentifierFormat : ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public FunctionIdentifierFormat(ClassificationColorManager colorManager)
            {
                DisplayName = "HLSL Global Function Identifier";
                ForegroundColor = colorManager.GetDefaultColor(FunctionClassificationTypeName);
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [Name(ClassIdentifierClassificationTypeName)]
        [ClassificationType(ClassificationTypeNames = ClassIdentifierClassificationTypeName)]
        [UserVisible(true)]
        public sealed class ClassIdentifierFormat : ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public ClassIdentifierFormat(ClassificationColorManager colorManager)
            {
                DisplayName = "HLSL Class Identifier";
                ForegroundColor = colorManager.GetDefaultColor(ClassIdentifierClassificationTypeName);
            }
        }
    }
}