using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace ShaderTools.Editor.VisualStudio.Hlsl.Tagging.Classification
{
    internal sealed class SemanticClassificationMetadata
    {
        public const string PunctuationClassificationTypeName = "Hlsl.Punctuation";
        public const string SemanticClassificationTypeName = "Hlsl.Semantic";
        public const string PackOffsetClassificationTypeName = "Hlsl.PackOffset";
        public const string RegisterLocationClassificationTypeName = "Hlsl.RegisterLocation";
        public const string NamespaceClassificationTypeName = "Hlsl.Namespace";
        public const string GlobalVariableClassificationTypeName = "Hlsl.GlobalVariable";
        public const string FieldIdentifierClassificationTypeName = "Hlsl.Field";
        public const string LocalVariableClassificationTypeName = "Hlsl.LocalVariable";
        public const string ConstantBufferVariableClassificationTypeName = "Hlsl.ConstantBufferVariable";
        public const string ParameterClassificationTypeName = "Hlsl.Parameter";
        public const string FunctionClassificationTypeName = "Hlsl.Function";
        public const string MethodClassificationTypeName = "Hlsl.Method";
        public const string ClassIdentifierClassificationTypeName = "Hlsl.Class";
        public const string StructIdentifierClassificationTypeName = "Hlsl.Struct";
        public const string InterfaceIdentifierClassificationTypeName = "Hlsl.Interface";
        public const string ConstantBufferIdentifierClassificationTypeName = "Hlsl.ConstantBuffer";

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
        [Name(NamespaceClassificationTypeName)]
        [BaseDefinition(PredefinedClassificationTypeNames.FormalLanguage)]
        public ClassificationTypeDefinition NamespaceIdentifierType;

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
        [Name(ConstantBufferVariableClassificationTypeName)]
        [BaseDefinition(PredefinedClassificationTypeNames.FormalLanguage)]
        public ClassificationTypeDefinition ConstantBufferVariableIdentifierType;

        [Export]
        [Name(ParameterClassificationTypeName)]
        [BaseDefinition(PredefinedClassificationTypeNames.FormalLanguage)]
        public ClassificationTypeDefinition ParameterIdentifierType;

        [Export]
        [Name(FunctionClassificationTypeName)]
        [BaseDefinition(PredefinedClassificationTypeNames.FormalLanguage)]
        public ClassificationTypeDefinition FunctionIdentifierType;

        [Export]
        [Name(MethodClassificationTypeName)]
        [BaseDefinition(PredefinedClassificationTypeNames.FormalLanguage)]
        public ClassificationTypeDefinition MethodIdentifierType;

        [Export]
        [Name(ClassIdentifierClassificationTypeName)]
        [BaseDefinition(PredefinedClassificationTypeNames.FormalLanguage)]
        public ClassificationTypeDefinition ClassIdentifierType;

        [Export]
        [Name(StructIdentifierClassificationTypeName)]
        [BaseDefinition(PredefinedClassificationTypeNames.FormalLanguage)]
        public ClassificationTypeDefinition StructIdentifierType;

        [Export]
        [Name(InterfaceIdentifierClassificationTypeName)]
        [BaseDefinition(PredefinedClassificationTypeNames.FormalLanguage)]
        public ClassificationTypeDefinition InterfaceIdentifierType;

        [Export]
        [Name(ConstantBufferIdentifierClassificationTypeName)]
        [BaseDefinition(PredefinedClassificationTypeNames.FormalLanguage)]
        public ClassificationTypeDefinition ConstantBufferIdentifierType;

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
                DisplayName = "HLSL Punctuation";
                ForegroundColor = colorManager.GetDefaultColor(PunctuationClassificationTypeName);
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [Name(SemanticClassificationTypeName)]
        [ClassificationType(ClassificationTypeNames = SemanticClassificationTypeName)]
        [UserVisible(true)]
        [Order(After = PredefinedClassificationTypeNames.String)]
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
        [Order(After = PredefinedClassificationTypeNames.String)]
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
        [Order(After = PredefinedClassificationTypeNames.String)]
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
        [Name(NamespaceClassificationTypeName)]
        [ClassificationType(ClassificationTypeNames = NamespaceClassificationTypeName)]
        [UserVisible(true)]
        [Order(After = PredefinedClassificationTypeNames.String)]
        public sealed class NamespaceIdentifierFormat : ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public NamespaceIdentifierFormat(ClassificationColorManager colorManager)
            {
                DisplayName = "HLSL Namespace Identifier";
                ForegroundColor = colorManager.GetDefaultColor(NamespaceClassificationTypeName);
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [Name(GlobalVariableClassificationTypeName)]
        [ClassificationType(ClassificationTypeNames = GlobalVariableClassificationTypeName)]
        [UserVisible(true)]
        [Order(After = PredefinedClassificationTypeNames.String)]
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
        [Order(After = PredefinedClassificationTypeNames.String)]
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
        [Order(After = PredefinedClassificationTypeNames.String)]
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
        [Name(ConstantBufferVariableClassificationTypeName)]
        [ClassificationType(ClassificationTypeNames = ConstantBufferVariableClassificationTypeName)]
        [UserVisible(true)]
        [Order(After = PredefinedClassificationTypeNames.String)]
        public sealed class ConstantBufferVariableIdentifierFormat : ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public ConstantBufferVariableIdentifierFormat(ClassificationColorManager colorManager)
            {
                DisplayName = "HLSL Constant Buffer Variable Identifier";
                ForegroundColor = colorManager.GetDefaultColor(ConstantBufferVariableClassificationTypeName);
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [Name(ParameterClassificationTypeName)]
        [ClassificationType(ClassificationTypeNames = ParameterClassificationTypeName)]
        [UserVisible(true)]
        [Order(After = PredefinedClassificationTypeNames.String)]
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
        [Order(After = PredefinedClassificationTypeNames.String)]
        public sealed class FunctionIdentifierFormat : ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public FunctionIdentifierFormat(ClassificationColorManager colorManager)
            {
                DisplayName = "HLSL Function Identifier";
                ForegroundColor = colorManager.GetDefaultColor(FunctionClassificationTypeName);
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [Name(MethodClassificationTypeName)]
        [ClassificationType(ClassificationTypeNames = MethodClassificationTypeName)]
        [UserVisible(true)]
        [Order(After = PredefinedClassificationTypeNames.String)]
        public sealed class MethodIdentifierFormat : ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public MethodIdentifierFormat(ClassificationColorManager colorManager)
            {
                DisplayName = "HLSL Method Identifier";
                ForegroundColor = colorManager.GetDefaultColor(MethodClassificationTypeName);
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [Name(ClassIdentifierClassificationTypeName)]
        [ClassificationType(ClassificationTypeNames = ClassIdentifierClassificationTypeName)]
        [UserVisible(true)]
        [Order(After = PredefinedClassificationTypeNames.String)]
        public sealed class ClassIdentifierFormat : ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public ClassIdentifierFormat(ClassificationColorManager colorManager)
            {
                DisplayName = "HLSL Class Identifier";
                ForegroundColor = colorManager.GetDefaultColor(ClassIdentifierClassificationTypeName);
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [Name(StructIdentifierClassificationTypeName)]
        [ClassificationType(ClassificationTypeNames = StructIdentifierClassificationTypeName)]
        [UserVisible(true)]
        [Order(After = PredefinedClassificationTypeNames.String)]
        public sealed class StructIdentifierFormat : ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public StructIdentifierFormat(ClassificationColorManager colorManager)
            {
                DisplayName = "HLSL Struct Identifier";
                ForegroundColor = colorManager.GetDefaultColor(StructIdentifierClassificationTypeName);
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [Name(InterfaceIdentifierClassificationTypeName)]
        [ClassificationType(ClassificationTypeNames = InterfaceIdentifierClassificationTypeName)]
        [UserVisible(true)]
        [Order(After = PredefinedClassificationTypeNames.String)]
        public sealed class InterfaceIdentifierFormat : ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public InterfaceIdentifierFormat(ClassificationColorManager colorManager)
            {
                DisplayName = "HLSL Interface Identifier";
                ForegroundColor = colorManager.GetDefaultColor(InterfaceIdentifierClassificationTypeName);
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [Name(ConstantBufferIdentifierClassificationTypeName)]
        [ClassificationType(ClassificationTypeNames = ConstantBufferIdentifierClassificationTypeName)]
        [UserVisible(true)]
        [Order(After = PredefinedClassificationTypeNames.String)]
        public sealed class ConstantBufferIdentifierFormat : ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public ConstantBufferIdentifierFormat(ClassificationColorManager colorManager)
            {
                DisplayName = "HLSL Constant Buffer Identifier";
                ForegroundColor = colorManager.GetDefaultColor(ConstantBufferIdentifierClassificationTypeName);
            }
        }
    }
}