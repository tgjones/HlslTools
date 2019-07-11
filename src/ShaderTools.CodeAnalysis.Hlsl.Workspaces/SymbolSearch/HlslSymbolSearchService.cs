using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.Host.Mef;
using ShaderTools.CodeAnalysis.Compilation;
using ShaderTools.CodeAnalysis.Hlsl.Compilation;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Symbols;
using ShaderTools.CodeAnalysis.SymbolSearch;
using ShaderTools.CodeAnalysis.Syntax;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Hlsl.SymbolSearch
{
    [ExportLanguageService(typeof(ISymbolSearchService), LanguageNames.Hlsl)]
    internal sealed class HlslSymbolSearchService : ISymbolSearchService
    {
        public SymbolSpan? FindSymbol(SemanticModelBase semanticModel, SourceLocation position)
        {
            if (semanticModel == null)
                throw new ArgumentNullException(nameof(semanticModel));

            var syntaxTreeRoot = (SyntaxNode) semanticModel.SyntaxTree.Root;
            return syntaxTreeRoot.FindNodes(position)
                .SelectMany(n => GetSymbolSpans((SemanticModel) semanticModel, n))
                .Where(s => s.Span.File.IsRootFile && s.SourceRange.ContainsOrTouches(position))
                .Select(s => s).Cast<SymbolSpan?>().FirstOrDefault();
        }

        public ImmutableArray<SymbolSpan> FindUsages(SemanticModelBase semanticModel, ISymbol symbol)
        {
            if (semanticModel == null)
                throw new ArgumentNullException(nameof(semanticModel));

            if (symbol == null)
                throw new ArgumentNullException(nameof(symbol));

            var syntaxTreeRoot = (SyntaxNode) semanticModel.SyntaxTree.Root;

            return (from n in syntaxTreeRoot.DescendantNodes()
                   from s in GetSymbolSpans((SemanticModel) semanticModel, (SyntaxNode) n)
                   where s.Symbol.Equals(symbol)
                   select s).ToImmutableArray();
        }

        private static IEnumerable<SymbolSpan> GetSymbolSpans(SemanticModel semanticModel, SyntaxNode node)
        {
            switch (node.Kind)
            {
                case SyntaxKind.VariableDeclarator:
                {
                    var expression = (VariableDeclaratorSyntax) node;
                    var symbol = semanticModel.GetDeclaredSymbol(expression);
                    if (symbol != null)
                        yield return SymbolSpan.CreateDefinition(symbol, expression.Identifier.SourceRange, expression.Identifier.FileSpan);
                    break;
                }
                case SyntaxKind.ClassType:
                case SyntaxKind.StructType:
                {
                    var expression = (StructTypeSyntax) node;
                    var symbol = semanticModel.GetDeclaredSymbol(expression);
                    if (symbol != null && expression.Name != null)
                        yield return SymbolSpan.CreateDefinition(symbol, expression.Name.SourceRange, expression.Name.FileSpan);
                    break;
                }
                case SyntaxKind.InterfaceType:
                {
                    var expression = (InterfaceTypeSyntax) node;
                    var symbol = semanticModel.GetDeclaredSymbol(expression);
                    if (symbol != null)
                        yield return SymbolSpan.CreateDefinition(symbol, expression.Name.SourceRange, expression.Name.FileSpan);
                    break;
                }
                case SyntaxKind.IdentifierName:
                {
                    var expression = (IdentifierNameSyntax) node;
                    var symbol = semanticModel.GetSymbol(expression);
                    if (symbol != null)
                        yield return SymbolSpan.CreateReference(symbol, expression.Name.SourceRange, expression.Name.FileSpan);
                    break;
                }
                case SyntaxKind.IdentifierDeclarationName:
                {
                    var expression = (IdentifierDeclarationNameSyntax) node;
                    var symbol = semanticModel.GetSymbol(expression);
                    if (symbol != null)
                        yield return SymbolSpan.CreateDefinition(symbol, expression.Name.SourceRange, expression.Name.FileSpan);
                    break;
                }
                case SyntaxKind.FieldAccessExpression:
                {
                    var expression = (FieldAccessExpressionSyntax) node;
                    var symbol = semanticModel.GetSymbol(expression);
                    if (symbol != null)
                        yield return SymbolSpan.CreateReference(symbol, expression.Name.SourceRange, expression.Name.FileSpan);
                    break;
                }
                case SyntaxKind.MethodInvocationExpression:
                {
                    var expression = (MethodInvocationExpressionSyntax) node;
                    var symbol = semanticModel.GetSymbol(expression);
                    if (symbol != null)
                        yield return SymbolSpan.CreateReference(symbol, expression.Name.SourceRange, expression.Name.FileSpan);
                    break;
                }
                case SyntaxKind.FunctionInvocationExpression:
                {
                    var expression = (FunctionInvocationExpressionSyntax) node;
                    var symbol = semanticModel.GetSymbol(expression);
                    if (symbol != null)
                        yield return SymbolSpan.CreateReference(symbol, 
                            expression.Name.GetUnqualifiedName().Name.SourceRange, 
                            expression.Name.GetUnqualifiedName().Name.FileSpan);
                    break;
                }
                case SyntaxKind.FunctionDefinition:
                {
                    var expression = (FunctionDefinitionSyntax) node;
                    var symbol = semanticModel.GetDeclaredSymbol(expression);
                    if (symbol != null)
                        yield return SymbolSpan.CreateDefinition(symbol,
                            expression.Name.GetUnqualifiedName().Name.SourceRange,
                            expression.Name.GetUnqualifiedName().Name.FileSpan);
                    break;
                }
                case SyntaxKind.FunctionDeclaration:
                {
                    var expression = (FunctionDeclarationSyntax) node;
                    var symbol = semanticModel.GetDeclaredSymbol(expression);
                    if (symbol != null)
                        yield return SymbolSpan.CreateDefinition(symbol,
                            expression.Name.GetUnqualifiedName().Name.SourceRange,
                            expression.Name.GetUnqualifiedName().Name.FileSpan);
                    break;
                }
            }
        }
    }
}