using System.Collections.Generic;
using System.Threading;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using ShaderTools.Core.Symbols;
using ShaderTools.Hlsl.Compilation;
using ShaderTools.Hlsl.Symbols;
using ShaderTools.Hlsl.Syntax;

namespace ShaderTools.Editor.VisualStudio.Hlsl.Tagging.Classification
{
    internal sealed class SemanticTaggerVisitor : SyntaxWalker
    {
        private readonly SemanticModel _semanticModel;
        private readonly HlslClassificationService _classificationService;
        private readonly ITextSnapshot _snapshot;
        private readonly List<ITagSpan<IClassificationTag>> _results;
        private readonly CancellationToken _cancellationToken;

        public SemanticTaggerVisitor(SemanticModel semanticModel, HlslClassificationService classificationService, ITextSnapshot snapshot, List<ITagSpan<IClassificationTag>> results, CancellationToken cancellationToken)
        {
            _semanticModel = semanticModel;
            _classificationService = classificationService;
            _snapshot = snapshot;
            _results = results;
            _cancellationToken = cancellationToken;
        }

        public override void Visit(SyntaxNode node)
        {
            _cancellationToken.ThrowIfCancellationRequested();
            base.Visit(node);
        }

        public override void VisitAttribute(AttributeSyntax node)
        {
            var symbol = _semanticModel.GetSymbol(node);
            if (symbol != null)
                CreateTag(node.Name.Name, _classificationService.FunctionIdentifier);

            base.VisitAttribute(node);
        }

        public override void VisitInterfaceType(InterfaceTypeSyntax node)
        {
            var symbol = _semanticModel.GetDeclaredSymbol(node);
            if (symbol != null)
                CreateTag(node.Name, _classificationService.InterfaceIdentifier);

            base.VisitInterfaceType(node);
        }

        public override void VisitStructType(StructTypeSyntax node)
        {
            var symbol = _semanticModel.GetDeclaredSymbol(node);
            if (symbol != null)
                CreateTag(node.Name, node.IsClass ? _classificationService.ClassIdentifier : _classificationService.StructIdentifier);

            base.VisitStructType(node);
        }

        public override void VisitConstantBuffer(ConstantBufferSyntax node)
        {
            CreateTag(node.Name, _classificationService.ConstantBufferIdentifier);

            base.VisitConstantBuffer(node);
        }

        public override void VisitFunctionDefinition(FunctionDefinitionSyntax node)
        {
            var symbol = _semanticModel.GetDeclaredSymbol(node);
            if (symbol != null)
                CreateTag(node.Name.GetUnqualifiedName().Name, symbol.Parent != null && symbol.Parent.Kind == SymbolKind.Class
                    ? _classificationService.MethodIdentifier
                    : _classificationService.FunctionIdentifier);

            base.VisitFunctionDefinition(node);
        }

        public override void VisitVariableDeclarator(VariableDeclaratorSyntax node)
        {
            var symbol = _semanticModel.GetDeclaredSymbol(node);
            if (symbol != null)
                CreateTag(node.Identifier, GetClassificationType(symbol));

            base.VisitVariableDeclarator(node);
        }

        public override void VisitSemantic(SemanticSyntax node)
        {
            var symbol = _semanticModel.GetSymbol(node);
            if (symbol != null)
                CreateTag(node.Semantic, _classificationService.Semantic);

            base.VisitSemantic(node);
        }

        public override void VisitRegisterLocation(RegisterLocation node)
        {
            CreateTag(node.Register, _classificationService.RegisterLocation);
            base.VisitRegisterLocation(node);
        }

        public override void VisitLogicalRegisterSpace(LogicalRegisterSpace node)
        {
            CreateTag(node.SpaceToken, _classificationService.RegisterLocation);
            base.VisitLogicalRegisterSpace(node);
        }

        public override void VisitPackOffsetLocation(PackOffsetLocation node)
        {
            CreateTag(node.Register, _classificationService.PackOffset);
            if (node.ComponentPart != null)
            {
                CreateTag(node.ComponentPart.DotToken, _classificationService.PackOffset);
                CreateTag(node.ComponentPart.Component, _classificationService.PackOffset);
            }
            base.VisitPackOffsetLocation(node);
        }

        public override void VisitIdentifierName(IdentifierNameSyntax node)
        {
            var symbol = _semanticModel.GetSymbol(node);
            if (symbol != null)
                CreateTag(node.Name, GetClassificationType(symbol));

            base.VisitIdentifierName(node);
        }

        public override void VisitIdentifierDeclarationName(IdentifierDeclarationNameSyntax node)
        {
            var symbol = _semanticModel.GetSymbol(node);
            if (symbol != null)
                CreateTag(node.Name, GetClassificationType(symbol));

            base.VisitIdentifierDeclarationName(node);
        }

        public override void VisitFieldAccess(FieldAccessExpressionSyntax node)
        {
            var symbol = _semanticModel.GetSymbol(node);
            if (symbol != null)
                CreateTag(node.Name, _classificationService.FieldIdentifier);

            base.VisitFieldAccess(node);
        }

        public override void VisitMethodInvocationExpression(MethodInvocationExpressionSyntax node)
        {
            var symbol = _semanticModel.GetSymbol(node);
            if (symbol != null)
                CreateTag(node.Name, _classificationService.MethodIdentifier);

            base.VisitMethodInvocationExpression(node);
        }

        public override void VisitFunctionInvocationExpression(FunctionInvocationExpressionSyntax node)
        {
            var symbol = _semanticModel.GetSymbol(node);
            if (symbol != null)
                CreateTag(node.Name.GetUnqualifiedName().Name, _classificationService.FunctionIdentifier);

            base.VisitFunctionInvocationExpression(node);
        }

        private IClassificationType GetClassificationType(Symbol symbol)
        {
            switch (symbol.Kind)
            {
                case SymbolKind.Field:
                    return _classificationService.FieldIdentifier;
                case SymbolKind.Parameter:
                    return _classificationService.ParameterIdentifier;
                case SymbolKind.Variable:
                    return symbol.Parent == null
                        ? _classificationService.GlobalVariableIdentifier
                        : (symbol.Parent.Kind == SymbolKind.ConstantBuffer)
                            ? _classificationService.ConstantBufferVariableIdentifier
                            : _classificationService.LocalVariableIdentifier;
                case SymbolKind.Class:
                case SymbolKind.Struct:
                case SymbolKind.Interface:
                    return _classificationService.ClassIdentifier;
                case SymbolKind.Function:
                    return _classificationService.FunctionIdentifier;
                default:
                    return null;
            }
        }

        private void CreateTag(SyntaxToken token, IClassificationType classificationType)
        {
            if (token == null || !token.Span.IsInRootFile || token.MacroReference != null || classificationType == null)
                return;

            var snapshotSpan = new SnapshotSpan(_snapshot, token.Span.Start, token.Span.Length);
            var tag = new ClassificationTag(classificationType);
            var tagSpan = new TagSpan<IClassificationTag>(snapshotSpan, tag);

            _results.Add(tagSpan);
        }
    }
}