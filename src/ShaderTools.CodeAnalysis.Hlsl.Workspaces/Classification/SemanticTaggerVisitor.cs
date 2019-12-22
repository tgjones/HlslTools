using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis.Classification;
using ShaderTools.CodeAnalysis.Hlsl.Compilation;
using ShaderTools.CodeAnalysis.Hlsl.Symbols;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Symbols;

namespace ShaderTools.CodeAnalysis.Hlsl.Classification
{
    internal sealed class SemanticTaggerVisitor : SyntaxWalker
    {
        private readonly SemanticModel _semanticModel;
        private readonly List<ClassifiedSpan> _results;
        private readonly CancellationToken _cancellationToken;

        public SemanticTaggerVisitor(SemanticModel semanticModel, List<ClassifiedSpan> results, CancellationToken cancellationToken)
        {
            _semanticModel = semanticModel;
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
                CreateTag(node.Name.GetUnqualifiedName().Name, HlslClassificationTypeNames.FunctionIdentifier);

            base.VisitAttribute(node);
        }

        public override void VisitInterfaceType(InterfaceTypeSyntax node)
        {
            var symbol = _semanticModel.GetDeclaredSymbol(node);
            if (symbol != null)
                CreateTag(node.Name, HlslClassificationTypeNames.InterfaceIdentifier);

            base.VisitInterfaceType(node);
        }

        public override void VisitStructType(StructTypeSyntax node)
        {
            var symbol = _semanticModel.GetDeclaredSymbol(node);
            if (symbol != null)
                CreateTag(node.Name, node.IsClass ? HlslClassificationTypeNames.ClassIdentifier : HlslClassificationTypeNames.StructIdentifier);

            base.VisitStructType(node);
        }

        public override void VisitConstantBuffer(ConstantBufferSyntax node)
        {
            CreateTag(node.Name, HlslClassificationTypeNames.ConstantBufferIdentifier);

            base.VisitConstantBuffer(node);
        }

        public override void VisitFunctionDefinition(FunctionDefinitionSyntax node)
        {
            var symbol = _semanticModel.GetDeclaredSymbol(node);
            if (symbol != null)
                CreateTag(node.Name.GetUnqualifiedName().Name, symbol.Parent != null && (symbol.Parent.Kind == SymbolKind.Class || symbol.Parent.Kind == SymbolKind.Struct)
                    ? HlslClassificationTypeNames.MethodIdentifier
                    : HlslClassificationTypeNames.FunctionIdentifier);

            base.VisitFunctionDefinition(node);
        }

        public override void VisitFunctionDeclaration(FunctionDeclarationSyntax node)
        {
            var symbol = _semanticModel.GetDeclaredSymbol(node);
            if (symbol != null)
                CreateTag(node.Name.GetUnqualifiedName().Name, symbol.Parent != null && (symbol.Parent.Kind == SymbolKind.Class || symbol.Parent.Kind == SymbolKind.Struct)
                    ? HlslClassificationTypeNames.MethodIdentifier
                    : HlslClassificationTypeNames.FunctionIdentifier);

            base.VisitFunctionDeclaration(node);
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
                CreateTag(node.Semantic, HlslClassificationTypeNames.Semantic);

            base.VisitSemantic(node);
        }

        public override void VisitRegisterLocation(RegisterLocation node)
        {
            CreateTag(node.Register, HlslClassificationTypeNames.RegisterLocation);
            base.VisitRegisterLocation(node);
        }

        public override void VisitLogicalRegisterSpace(LogicalRegisterSpace node)
        {
            CreateTag(node.SpaceToken, HlslClassificationTypeNames.RegisterLocation);
            base.VisitLogicalRegisterSpace(node);
        }

        public override void VisitPackOffsetLocation(PackOffsetLocation node)
        {
            CreateTag(node.Register, HlslClassificationTypeNames.PackOffset);
            if (node.ComponentPart != null)
            {
                CreateTag(node.ComponentPart.DotToken, HlslClassificationTypeNames.PackOffset);
                CreateTag(node.ComponentPart.Component, HlslClassificationTypeNames.PackOffset);
            }
            base.VisitPackOffsetLocation(node);
        }

        public override void VisitTypedefStatement(TypedefStatementSyntax node)
        {
            var symbol = _semanticModel.GetSymbol(node.Type);
            if (symbol != null)
            {
                var classificationType = GetClassificationType(symbol);
                foreach (var declarator in node.Declarators)
                    CreateTag(declarator.Identifier, classificationType);
            }
            base.VisitTypedefStatement(node);
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

        public override void VisitFieldAccessExpression(FieldAccessExpressionSyntax node)
        {
            var symbol = _semanticModel.GetSymbol(node);
            if (symbol != null)
                CreateTag(node.Name, HlslClassificationTypeNames.FieldIdentifier);

            base.VisitFieldAccessExpression(node);
        }

        public override void VisitMethodInvocationExpression(MethodInvocationExpressionSyntax node)
        {
            var symbol = _semanticModel.GetSymbol(node);
            if (symbol != null)
                CreateTag(node.Name, HlslClassificationTypeNames.MethodIdentifier);

            base.VisitMethodInvocationExpression(node);
        }

        public override void VisitFunctionInvocationExpression(FunctionInvocationExpressionSyntax node)
        {
            var symbol = _semanticModel.GetSymbol(node);
            if (symbol != null)
                CreateTag(node.Name.GetUnqualifiedName().Name, HlslClassificationTypeNames.FunctionIdentifier);

            base.VisitFunctionInvocationExpression(node);
        }

        public override void VisitSyntaxToken(SyntaxToken node)
        {
            void CreateMacroTag(SyntaxNode trivia)
            {
                if (trivia is DefineDirectiveTriviaSyntax dd)
                    CreateTag(dd.MacroName, HlslClassificationTypeNames.MacroIdentifier);
            }

            foreach (var trivia in node.LeadingTrivia)
                CreateMacroTag(trivia);

            foreach (var trivia in node.TrailingTrivia)
                CreateMacroTag(trivia);

            if (node.MacroReference != null)
                CreateTag(node.MacroReference.NameToken, HlslClassificationTypeNames.MacroIdentifier);

            base.VisitSyntaxToken(node);
        }

        private static string GetClassificationType(Symbol symbol)
        {
            switch (symbol.Kind)
            {
                case SymbolKind.Field:
                    return HlslClassificationTypeNames.FieldIdentifier;
                case SymbolKind.Parameter:
                    return HlslClassificationTypeNames.ParameterIdentifier;
                case SymbolKind.Variable:
                    return symbol.Parent == null
                        ? HlslClassificationTypeNames.GlobalVariableIdentifier
                        : (symbol.Parent.Kind == SymbolKind.ConstantBuffer)
                            ? HlslClassificationTypeNames.ConstantBufferVariableIdentifier
                            : HlslClassificationTypeNames.LocalVariableIdentifier;
                case SymbolKind.Class:
                    return HlslClassificationTypeNames.ClassIdentifier;
                case SymbolKind.Struct:
                    return HlslClassificationTypeNames.StructIdentifier;
                case SymbolKind.Interface:
                    return HlslClassificationTypeNames.InterfaceIdentifier;
                case SymbolKind.TypeAlias:
                    return GetClassificationType(((TypeAliasSymbol) symbol).ValueType);
                case SymbolKind.Function:
                    return HlslClassificationTypeNames.FunctionIdentifier;
                default:
                    return null;
            }
        }

        private void CreateTag(SyntaxToken token, string classificationType)
        {
            if (token == null || !token.FileSpan.IsInRootFile || token.MacroReference != null || classificationType == null)
                return;

            _results.Add(new ClassifiedSpan(token.FileSpan.Span, classificationType));
        }
    }
}