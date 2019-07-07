using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using ShaderTools.CodeAnalysis.Editor.Implementation.Classification;
using ShaderTools.CodeAnalysis.Hlsl.Classification;

namespace ShaderTools.CodeAnalysis.Editor.Hlsl.Classification
{
    internal sealed class ClassificationTypeDefinitions
    {
        [Export]
        [Name(HlslClassificationTypeNames.Punctuation)]
        [BaseDefinition(PredefinedClassificationTypeNames.FormalLanguage)]
        public ClassificationTypeDefinition PunctuationType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [Name(HlslClassificationTypeNames.Punctuation)]
        [ClassificationType(ClassificationTypeNames = HlslClassificationTypeNames.Punctuation)]
        [UserVisible(true)]
        [Order(After = PredefinedClassificationTypeNames.String)]
        public sealed class PunctuationFormat : ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public PunctuationFormat(IClassificationColorManager colorManager)
            {
                DisplayName = "HLSL Punctuation";
                ForegroundColor = colorManager.GetDefaultColor(HlslClassificationTypeNames.Punctuation);
            }
        }

        [Export]
        [Name(HlslClassificationTypeNames.Semantic)]
        [BaseDefinition(PredefinedClassificationTypeNames.FormalLanguage)]
        public ClassificationTypeDefinition SemanticType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [Name(HlslClassificationTypeNames.Semantic)]
        [ClassificationType(ClassificationTypeNames = HlslClassificationTypeNames.Semantic)]
        [UserVisible(true)]
        [Order(After = PredefinedClassificationTypeNames.String)]
        public sealed class SemanticFormat : ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public SemanticFormat(IClassificationColorManager colorManager)
            {
                DisplayName = "HLSL Semantic";
                ForegroundColor = colorManager.GetDefaultColor(HlslClassificationTypeNames.Semantic);
            }
        }

        [Export]
        [Name(HlslClassificationTypeNames.PackOffset)]
        [BaseDefinition(PredefinedClassificationTypeNames.FormalLanguage)]
        public ClassificationTypeDefinition PackOffsetType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [Name(HlslClassificationTypeNames.PackOffset)]
        [ClassificationType(ClassificationTypeNames = HlslClassificationTypeNames.PackOffset)]
        [UserVisible(true)]
        [Order(After = PredefinedClassificationTypeNames.String)]
        public sealed class PackOffsetFormat : ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public PackOffsetFormat(IClassificationColorManager colorManager)
            {
                DisplayName = "HLSL Pack Offset";
                ForegroundColor = colorManager.GetDefaultColor(HlslClassificationTypeNames.PackOffset);
            }
        }

        [Export]
        [Name(HlslClassificationTypeNames.RegisterLocation)]
        [BaseDefinition(PredefinedClassificationTypeNames.FormalLanguage)]
        public ClassificationTypeDefinition RegisterLocationType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [Name(HlslClassificationTypeNames.RegisterLocation)]
        [ClassificationType(ClassificationTypeNames = HlslClassificationTypeNames.RegisterLocation)]
        [UserVisible(true)]
        [Order(After = PredefinedClassificationTypeNames.String)]
        public sealed class RegisterLocationFormat : ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public RegisterLocationFormat(IClassificationColorManager colorManager)
            {
                DisplayName = "HLSL Register Location";
                ForegroundColor = colorManager.GetDefaultColor(HlslClassificationTypeNames.RegisterLocation);
            }
        }

        [Export]
        [Name(HlslClassificationTypeNames.NamespaceIdentifier)]
        [BaseDefinition(PredefinedClassificationTypeNames.FormalLanguage)]
        public ClassificationTypeDefinition NamespaceIdentifierType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [Name(HlslClassificationTypeNames.NamespaceIdentifier)]
        [ClassificationType(ClassificationTypeNames = HlslClassificationTypeNames.NamespaceIdentifier)]
        [UserVisible(true)]
        [Order(After = PredefinedClassificationTypeNames.String)]
        public sealed class NamespaceIdentifierFormat : ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public NamespaceIdentifierFormat(IClassificationColorManager colorManager)
            {
                DisplayName = "HLSL Namespace Identifier";
                ForegroundColor = colorManager.GetDefaultColor(HlslClassificationTypeNames.NamespaceIdentifier);
            }
        }

        [Export]
        [Name(HlslClassificationTypeNames.GlobalVariableIdentifier)]
        [BaseDefinition(PredefinedClassificationTypeNames.FormalLanguage)]
        public ClassificationTypeDefinition GlobalVariableIdentifierType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [Name(HlslClassificationTypeNames.GlobalVariableIdentifier)]
        [ClassificationType(ClassificationTypeNames = HlslClassificationTypeNames.GlobalVariableIdentifier)]
        [UserVisible(true)]
        [Order(After = PredefinedClassificationTypeNames.String)]
        public sealed class GlobalVariableIdentifierFormat : ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public GlobalVariableIdentifierFormat(IClassificationColorManager colorManager)
            {
                DisplayName = "HLSL Global Variable Identifier";
                ForegroundColor = colorManager.GetDefaultColor(HlslClassificationTypeNames.GlobalVariableIdentifier);
            }
        }

        [Export]
        [Name(HlslClassificationTypeNames.FieldIdentifier)]
        [BaseDefinition(PredefinedClassificationTypeNames.FormalLanguage)]
        public ClassificationTypeDefinition FieldIdentifierType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [Name(HlslClassificationTypeNames.FieldIdentifier)]
        [ClassificationType(ClassificationTypeNames = HlslClassificationTypeNames.FieldIdentifier)]
        [UserVisible(true)]
        [Order(After = PredefinedClassificationTypeNames.String)]
        public sealed class FieldIdentifierFormat : ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public FieldIdentifierFormat(IClassificationColorManager colorManager)
            {
                DisplayName = "HLSL Field Identifier";
                ForegroundColor = colorManager.GetDefaultColor(HlslClassificationTypeNames.FieldIdentifier);
            }
        }

        [Export]
        [Name(HlslClassificationTypeNames.LocalVariableIdentifier)]
        [BaseDefinition(PredefinedClassificationTypeNames.FormalLanguage)]
        public ClassificationTypeDefinition LocalVariableIdentifierType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [Name(HlslClassificationTypeNames.LocalVariableIdentifier)]
        [ClassificationType(ClassificationTypeNames = HlslClassificationTypeNames.LocalVariableIdentifier)]
        [UserVisible(true)]
        [Order(After = PredefinedClassificationTypeNames.String)]
        public sealed class LocalVariableIdentifierFormat : ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public LocalVariableIdentifierFormat(IClassificationColorManager colorManager)
            {
                DisplayName = "HLSL Local Variable Identifier";
                ForegroundColor = colorManager.GetDefaultColor(HlslClassificationTypeNames.LocalVariableIdentifier);
            }
        }

        [Export]
        [Name(HlslClassificationTypeNames.ConstantBufferVariableIdentifier)]
        [BaseDefinition(PredefinedClassificationTypeNames.FormalLanguage)]
        public ClassificationTypeDefinition ConstantBufferVariableIdentifierType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [Name(HlslClassificationTypeNames.ConstantBufferVariableIdentifier)]
        [ClassificationType(ClassificationTypeNames = HlslClassificationTypeNames.ConstantBufferVariableIdentifier)]
        [UserVisible(true)]
        [Order(After = PredefinedClassificationTypeNames.String)]
        public sealed class ConstantBufferVariableIdentifierFormat : ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public ConstantBufferVariableIdentifierFormat(IClassificationColorManager colorManager)
            {
                DisplayName = "HLSL Constant Buffer Variable Identifier";
                ForegroundColor = colorManager.GetDefaultColor(HlslClassificationTypeNames.ConstantBufferVariableIdentifier);
            }
        }

        [Export]
        [Name(HlslClassificationTypeNames.ParameterIdentifier)]
        [BaseDefinition(PredefinedClassificationTypeNames.FormalLanguage)]
        public ClassificationTypeDefinition ParameterIdentifierType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [Name(HlslClassificationTypeNames.ParameterIdentifier)]
        [ClassificationType(ClassificationTypeNames = HlslClassificationTypeNames.ParameterIdentifier)]
        [UserVisible(true)]
        [Order(After = PredefinedClassificationTypeNames.String)]
        public sealed class ParameterIdentifierFormat : ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public ParameterIdentifierFormat(IClassificationColorManager colorManager)
            {
                DisplayName = "HLSL Parameter Variable Identifier";
                ForegroundColor = colorManager.GetDefaultColor(HlslClassificationTypeNames.ParameterIdentifier);
            }
        }

        [Export]
        [Name(HlslClassificationTypeNames.FunctionIdentifier)]
        [BaseDefinition(PredefinedClassificationTypeNames.FormalLanguage)]
        public ClassificationTypeDefinition FunctionIdentifierType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [Name(HlslClassificationTypeNames.FunctionIdentifier)]
        [ClassificationType(ClassificationTypeNames = HlslClassificationTypeNames.FunctionIdentifier)]
        [UserVisible(true)]
        [Order(After = PredefinedClassificationTypeNames.String)]
        public sealed class FunctionIdentifierFormat : ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public FunctionIdentifierFormat(IClassificationColorManager colorManager)
            {
                DisplayName = "HLSL Function Identifier";
                ForegroundColor = colorManager.GetDefaultColor(HlslClassificationTypeNames.FunctionIdentifier);
            }
        }

        [Export]
        [Name(HlslClassificationTypeNames.MethodIdentifier)]
        [BaseDefinition(PredefinedClassificationTypeNames.FormalLanguage)]
        public ClassificationTypeDefinition MethodIdentifierType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [Name(HlslClassificationTypeNames.MethodIdentifier)]
        [ClassificationType(ClassificationTypeNames = HlslClassificationTypeNames.MethodIdentifier)]
        [UserVisible(true)]
        [Order(After = PredefinedClassificationTypeNames.String)]
        public sealed class MethodIdentifierFormat : ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public MethodIdentifierFormat(IClassificationColorManager colorManager)
            {
                DisplayName = "HLSL Method Identifier";
                ForegroundColor = colorManager.GetDefaultColor(HlslClassificationTypeNames.MethodIdentifier);
            }
        }

        [Export]
        [Name(HlslClassificationTypeNames.ClassIdentifier)]
        [BaseDefinition(PredefinedClassificationTypeNames.FormalLanguage)]
        public ClassificationTypeDefinition ClassIdentifierType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [Name(HlslClassificationTypeNames.ClassIdentifier)]
        [ClassificationType(ClassificationTypeNames = HlslClassificationTypeNames.ClassIdentifier)]
        [UserVisible(true)]
        [Order(After = PredefinedClassificationTypeNames.String)]
        public sealed class ClassIdentifierFormat : ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public ClassIdentifierFormat(IClassificationColorManager colorManager)
            {
                DisplayName = "HLSL Class Identifier";
                ForegroundColor = colorManager.GetDefaultColor(HlslClassificationTypeNames.ClassIdentifier);
            }
        }

        [Export]
        [Name(HlslClassificationTypeNames.StructIdentifier)]
        [BaseDefinition(PredefinedClassificationTypeNames.FormalLanguage)]
        public ClassificationTypeDefinition StructIdentifierType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [Name(HlslClassificationTypeNames.StructIdentifier)]
        [ClassificationType(ClassificationTypeNames = HlslClassificationTypeNames.StructIdentifier)]
        [UserVisible(true)]
        [Order(After = PredefinedClassificationTypeNames.String)]
        public sealed class StructIdentifierFormat : ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public StructIdentifierFormat(IClassificationColorManager colorManager)
            {
                DisplayName = "HLSL Struct Identifier";
                ForegroundColor = colorManager.GetDefaultColor(HlslClassificationTypeNames.StructIdentifier);
            }
        }

        [Export]
        [Name(HlslClassificationTypeNames.InterfaceIdentifier)]
        [BaseDefinition(PredefinedClassificationTypeNames.FormalLanguage)]
        public ClassificationTypeDefinition InterfaceIdentifierType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [Name(HlslClassificationTypeNames.InterfaceIdentifier)]
        [ClassificationType(ClassificationTypeNames = HlslClassificationTypeNames.InterfaceIdentifier)]
        [UserVisible(true)]
        [Order(After = PredefinedClassificationTypeNames.String)]
        public sealed class InterfaceIdentifierFormat : ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public InterfaceIdentifierFormat(IClassificationColorManager colorManager)
            {
                DisplayName = "HLSL Interface Identifier";
                ForegroundColor = colorManager.GetDefaultColor(HlslClassificationTypeNames.InterfaceIdentifier);
            }
        }

        [Export]
        [Name(HlslClassificationTypeNames.ConstantBufferIdentifier)]
        [BaseDefinition(PredefinedClassificationTypeNames.FormalLanguage)]
        public ClassificationTypeDefinition ConstantBufferIdentifierType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [Name(HlslClassificationTypeNames.ConstantBufferIdentifier)]
        [ClassificationType(ClassificationTypeNames = HlslClassificationTypeNames.ConstantBufferIdentifier)]
        [UserVisible(true)]
        [Order(After = PredefinedClassificationTypeNames.String)]
        public sealed class ConstantBufferIdentifierFormat : ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public ConstantBufferIdentifierFormat(IClassificationColorManager colorManager)
            {
                DisplayName = "HLSL Constant Buffer Identifier";
                ForegroundColor = colorManager.GetDefaultColor(HlslClassificationTypeNames.ConstantBufferIdentifier);
            }
        }

        [Export]
        [Name(HlslClassificationTypeNames.MacroIdentifier)]
        [BaseDefinition(PredefinedClassificationTypeNames.FormalLanguage)]
        public ClassificationTypeDefinition MacroIdentifierType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [Name(HlslClassificationTypeNames.MacroIdentifier)]
        [ClassificationType(ClassificationTypeNames = HlslClassificationTypeNames.MacroIdentifier)]
        [UserVisible(true)]
        [Order(After = PredefinedClassificationTypeNames.String)]
        public sealed class MacroIdentifierFormat : ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public MacroIdentifierFormat(IClassificationColorManager colorManager)
            {
                DisplayName = "HLSL Macro Identifier";
                ForegroundColor = colorManager.GetDefaultColor(HlslClassificationTypeNames.MacroIdentifier);
            }
        }
    }
}