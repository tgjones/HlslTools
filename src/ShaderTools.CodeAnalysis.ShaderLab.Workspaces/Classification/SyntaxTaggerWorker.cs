using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis.Classification;
using Microsoft.CodeAnalysis.Text;
using ShaderTools.CodeAnalysis.ShaderLab.Syntax;

namespace ShaderTools.CodeAnalysis.ShaderLab.Classification
{
    internal sealed class SyntaxTaggerWorker
    {
        private readonly List<ClassifiedSpan> _results;
        private readonly CancellationToken _cancellationToken;

        public SyntaxTaggerWorker(List<ClassifiedSpan> results, CancellationToken cancellationToken)
        {
            _results = results;
            _cancellationToken = cancellationToken;
        }

        public void ClassifySyntax(SyntaxTree syntaxTree)
        {
            ClassifyNode(syntaxTree, (SyntaxNode) syntaxTree.Root);
        }

        private void ClassifyNode(SyntaxTree syntaxTree, SyntaxNode node)
        {
            _cancellationToken.ThrowIfCancellationRequested();

            foreach (var syntaxNodeOrToken in node.ChildNodes)
                ClassifyNodeOrToken(syntaxTree, (SyntaxNode) syntaxNodeOrToken);
        }

        private void ClassifyNodeOrToken(SyntaxTree syntaxTree, SyntaxNode nodeOrToken)
        {
            if (!nodeOrToken.IsToken)
                ClassifyNode(syntaxTree, nodeOrToken);
            else
                ClassifyToken(syntaxTree, (SyntaxToken)nodeOrToken);
        }

        private void ClassifyToken(SyntaxTree syntaxTree, SyntaxToken token)
        {
            foreach (var trivia in token.LeadingTrivia)
                ClassifyTrivia(syntaxTree, trivia);

            var kind = GetClassificationForToken(token);
            if (kind != null)
                AddClassification(syntaxTree, token, kind);

            foreach (var trivia in token.TrailingTrivia)
                ClassifyTrivia(syntaxTree, trivia);
        }

        private void ClassifyTrivia(SyntaxTree syntaxTree, SyntaxNode trivia)
        {
            if (trivia.Kind == SyntaxKind.WhitespaceTrivia || trivia.Kind == SyntaxKind.EndOfLineTrivia)
                AddClassification(syntaxTree, (SyntaxTrivia) trivia, ClassificationTypeNames.WhiteSpace);
            else if (trivia.Kind.IsComment())
                AddClassification(syntaxTree, (SyntaxTrivia)trivia, ClassificationTypeNames.Comment);
            else
                ClassifyNode(syntaxTree, trivia);
        }

        private void AddClassification(SyntaxTree syntaxTree, SyntaxNode node, string classificationType)
        {
            if (classificationType == null)
                throw new ArgumentNullException(nameof(classificationType));

            if (node.SourceRange.Length > 0)
            {
                var textSpan = syntaxTree.GetSourceFileSpan(node.SourceRange);
                _results.Add(CreateClassifiedSpan(textSpan.Span, node, classificationType));
            }
        }

        private ClassifiedSpan CreateClassifiedSpan(TextSpan span, SyntaxNode node, string classificationType)
        {
            return new ClassifiedSpan(span, classificationType);
        }

        private string GetClassificationForToken(SyntaxToken token)
        {
            if (token.Parent != null 
                && token.Parent.Kind == SyntaxKind.ShaderProperty 
                && token == ((ShaderPropertySyntax) token.Parent).NameToken)
                return ShaderLabClassificationTypeNames.ShaderProperty;

            if (token.Kind == SyntaxKind.IdentifierToken
                && token.Parent != null
                && token.Parent.Kind == SyntaxKind.ShaderPropertyAttribute
                && token == ((ShaderPropertyAttributeSyntax) token.Parent).Name)
                return ShaderLabClassificationTypeNames.Attribute;

            if (token.Kind == SyntaxKind.IdentifierToken && token.ContextualKind == SyntaxKind.IdentifierToken)
                return ClassificationTypeNames.Identifier;

            if (token.Kind.IsKeyword())
                return ClassificationTypeNames.Keyword;

            if (token.Kind.IsPunctuation())
                return ShaderLabClassificationTypeNames.Punctuation;

            if (token.Kind.IsOperator())
                return ClassificationTypeNames.Operator;

            if (token.Kind.IsWhitespace())
                return ClassificationTypeNames.WhiteSpace;

            if (token.Kind.IsComment())
                return ClassificationTypeNames.Comment;

            if (token.Kind == SyntaxKind.StringLiteralToken || token.Kind == SyntaxKind.BracketedStringLiteralToken)
                return ClassificationTypeNames.StringLiteral;

            if (token.Kind.IsNumericLiteral())
                return ClassificationTypeNames.NumericLiteral;

            return null;
        }
    }
}