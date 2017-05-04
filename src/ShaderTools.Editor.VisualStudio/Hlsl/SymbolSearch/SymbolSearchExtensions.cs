using System;
using System.Collections.Generic;
using System.Linq;
using ShaderTools.CodeAnalysis.Hlsl.Compilation;
using ShaderTools.CodeAnalysis.Hlsl.Symbols;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.Editor.VisualStudio.Hlsl.SymbolSearch
{
    internal static class SymbolSearchExtensions
    {
        public static SymbolSpan? FindSymbol(this SemanticModel semanticModel, SourceLocation position)
        {
            if (semanticModel == null)
                throw new ArgumentNullException(nameof(semanticModel));

            var syntaxTree = semanticModel.SyntaxTree;
            return syntaxTree.Root.FindNodes(position)
                .SelectMany(n => GetSymbolSpans(semanticModel, n))
                .Where(s => s.Span.File.IsRootFile && s.SourceRange.ContainsOrTouches(position))
                .Select(s => s).Cast<SymbolSpan?>().FirstOrDefault();
        }

        public static IEnumerable<SymbolSpan> FindUsages(this SemanticModel semanticModel, Symbol symbol)
        {
            if (semanticModel == null)
                throw new ArgumentNullException(nameof(semanticModel));

            if (symbol == null)
                throw new ArgumentNullException(nameof(symbol));

            var syntaxTree = semanticModel.SyntaxTree;

            return from n in syntaxTree.Root.DescendantNodes()
                from s in GetSymbolSpans(semanticModel, n)
                where s.Symbol.Equals(symbol)
                select s;
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
                        yield return SymbolSpan.CreateDefinition(symbol, expression.Identifier.SourceRange, expression.Identifier.Span);
                    break;
                }
                case SyntaxKind.ClassType:
                case SyntaxKind.StructType:
                {
                    var expression = (StructTypeSyntax) node;
                    var symbol = semanticModel.GetDeclaredSymbol(expression);
                    if (symbol != null && expression.Name != null)
                        yield return SymbolSpan.CreateDefinition(symbol, expression.Name.SourceRange, expression.Name.Span);
                    break;
                }
                case SyntaxKind.InterfaceType:
                {
                    var expression = (InterfaceTypeSyntax) node;
                    var symbol = semanticModel.GetDeclaredSymbol(expression);
                    if (symbol != null)
                        yield return SymbolSpan.CreateDefinition(symbol, expression.Name.SourceRange, expression.Name.Span);
                    break;
                }
                case SyntaxKind.IdentifierName:
                {
                    var expression = (IdentifierNameSyntax) node;
                    var symbol = semanticModel.GetSymbol(expression);
                    if (symbol != null)
                        yield return SymbolSpan.CreateReference(symbol, expression.Name.SourceRange, expression.Name.Span);
                    break;
                }
                case SyntaxKind.IdentifierDeclarationName:
                {
                    var expression = (IdentifierDeclarationNameSyntax) node;
                    var symbol = semanticModel.GetSymbol(expression);
                    if (symbol != null)
                        yield return SymbolSpan.CreateDefinition(symbol, expression.Name.SourceRange, expression.Name.Span);
                    break;
                }
                case SyntaxKind.FieldAccessExpression:
                {
                    var expression = (FieldAccessExpressionSyntax) node;
                    var symbol = semanticModel.GetSymbol(expression);
                    if (symbol != null)
                        yield return SymbolSpan.CreateReference(symbol, expression.Name.SourceRange, expression.Name.Span);
                    break;
                }
                case SyntaxKind.MethodInvocationExpression:
                {
                    var expression = (MethodInvocationExpressionSyntax) node;
                    var symbol = semanticModel.GetSymbol(expression);
                    if (symbol != null)
                        yield return SymbolSpan.CreateReference(symbol, expression.Name.SourceRange, expression.Name.Span);
                    break;
                }
                case SyntaxKind.FunctionInvocationExpression:
                {
                    var expression = (FunctionInvocationExpressionSyntax) node;
                    var symbol = semanticModel.GetSymbol(expression);
                    if (symbol != null)
                        yield return SymbolSpan.CreateReference(symbol, 
                            expression.Name.GetUnqualifiedName().Name.SourceRange, 
                            expression.Name.GetUnqualifiedName().Name.Span);
                    break;
                }
                case SyntaxKind.FunctionDefinition:
                {
                    var expression = (FunctionDefinitionSyntax) node;
                    var symbol = semanticModel.GetDeclaredSymbol(expression);
                    if (symbol != null)
                        yield return SymbolSpan.CreateDefinition(symbol,
                            expression.Name.GetUnqualifiedName().Name.SourceRange,
                            expression.Name.GetUnqualifiedName().Name.Span);
                    break;
                }
                case SyntaxKind.FunctionDeclaration:
                {
                    var expression = (FunctionDeclarationSyntax) node;
                    var symbol = semanticModel.GetDeclaredSymbol(expression);
                    if (symbol != null)
                        yield return SymbolSpan.CreateDefinition(symbol,
                            expression.Name.GetUnqualifiedName().Name.SourceRange,
                            expression.Name.GetUnqualifiedName().Name.Span);
                    break;
                }
            }
        }
    }
}