using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text.Classification;

namespace ShaderTools.VisualStudio.ShaderLab.Tagging.Classification
{
    [Export]
    internal sealed class ShaderLabClassificationService
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
        public ShaderLabClassificationService(IClassificationTypeRegistryService classificationTypeRegistryService, IStandardClassificationService standardClassificationService)
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
        public IClassificationType Other => _standardClassificationService.Other;

        public IClassificationType Punctuation => Get(ref _punctuation, SemanticClassificationMetadata.PunctuationClassificationTypeName);
    }
}
