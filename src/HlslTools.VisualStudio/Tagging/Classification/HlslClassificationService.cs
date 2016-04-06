using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text.Classification;

namespace HlslTools.VisualStudio.Tagging.Classification
{
    [Export]
    internal sealed class HlslClassificationService
    {
        private readonly IClassificationTypeRegistryService _classificationTypeRegistryService;
        private readonly IStandardClassificationService _standardClassificationService;

        private IClassificationType _punctuation;
        private IClassificationType _semantic;
        private IClassificationType _packOffset;
        private IClassificationType _registerLocation;
        private IClassificationType _namespaceIdentifier;
        private IClassificationType _globalVariableIdentifier;
        private IClassificationType _fieldIdentifier;
        private IClassificationType _localVariableIdentifier;
        private IClassificationType _constantBufferVariableIdentifier;
        private IClassificationType _parameterIdentifier;
        private IClassificationType _functionIdentifier;
        private IClassificationType _methodIdentifier;
        private IClassificationType _classIdentifier;
        private IClassificationType _structIdentifier;
        private IClassificationType _interfaceIdentifier;
        private IClassificationType _constantBufferIdentifier;

        [ImportingConstructor]
        public HlslClassificationService(IClassificationTypeRegistryService classificationTypeRegistryService, IStandardClassificationService standardClassificationService)
        {
            _classificationTypeRegistryService = classificationTypeRegistryService;
            _standardClassificationService = standardClassificationService;
        }

        private IClassificationType Get(ref IClassificationType target, string name)
        {
            return target ?? (target = _classificationTypeRegistryService.GetClassificationType(name));
        }

        public IClassificationType WhiteSpace => _standardClassificationService.WhiteSpace;
        public IClassificationType Comment => _standardClassificationService.Comment;
        public IClassificationType Identifier => _standardClassificationService.Identifier;
        public IClassificationType Keyword => _standardClassificationService.Keyword;
        public IClassificationType Operator => _standardClassificationService.Operator;
        public IClassificationType NumberLiteral => _standardClassificationService.NumberLiteral;
        public IClassificationType StringLiteral => _standardClassificationService.StringLiteral;
        public IClassificationType PreprocessorKeyword => _standardClassificationService.PreprocessorKeyword;
        public IClassificationType ExcludedCode => _standardClassificationService.ExcludedCode;
        public IClassificationType Other => _standardClassificationService.Other;

        public IClassificationType Punctuation => Get(ref _punctuation, SemanticClassificationMetadata.PunctuationClassificationTypeName);
        public IClassificationType Semantic => Get(ref _semantic, SemanticClassificationMetadata.SemanticClassificationTypeName);
        public IClassificationType PackOffset => Get(ref _packOffset, SemanticClassificationMetadata.PackOffsetClassificationTypeName);
        public IClassificationType RegisterLocation => Get(ref _registerLocation, SemanticClassificationMetadata.RegisterLocationClassificationTypeName);
        public IClassificationType NamespaceIdentifier => Get(ref _namespaceIdentifier, SemanticClassificationMetadata.NamespaceClassificationTypeName);
        public IClassificationType GlobalVariableIdentifier => Get(ref _globalVariableIdentifier, SemanticClassificationMetadata.GlobalVariableClassificationTypeName);
        public IClassificationType FieldIdentifier => Get(ref _fieldIdentifier, SemanticClassificationMetadata.FieldIdentifierClassificationTypeName);
        public IClassificationType LocalVariableIdentifier => Get(ref _localVariableIdentifier, SemanticClassificationMetadata.LocalVariableClassificationTypeName);
        public IClassificationType ConstantBufferVariableIdentifier => Get(ref _constantBufferVariableIdentifier, SemanticClassificationMetadata.ConstantBufferVariableClassificationTypeName);
        public IClassificationType ParameterIdentifier => Get(ref _parameterIdentifier, SemanticClassificationMetadata.ParameterClassificationTypeName);
        public IClassificationType FunctionIdentifier => Get(ref _functionIdentifier, SemanticClassificationMetadata.FunctionClassificationTypeName);
        public IClassificationType MethodIdentifier => Get(ref _methodIdentifier, SemanticClassificationMetadata.MethodClassificationTypeName);
        public IClassificationType ClassIdentifier => Get(ref _classIdentifier, SemanticClassificationMetadata.ClassIdentifierClassificationTypeName);
        public IClassificationType StructIdentifier => Get(ref _structIdentifier, SemanticClassificationMetadata.StructIdentifierClassificationTypeName);
        public IClassificationType InterfaceIdentifier => Get(ref _interfaceIdentifier, SemanticClassificationMetadata.InterfaceIdentifierClassificationTypeName);
        public IClassificationType ConstantBufferIdentifier => Get(ref _constantBufferIdentifier, SemanticClassificationMetadata.ConstantBufferIdentifierClassificationTypeName);
    }
}