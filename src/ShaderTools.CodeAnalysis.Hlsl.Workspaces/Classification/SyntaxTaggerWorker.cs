using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis.Classification;
using Microsoft.CodeAnalysis.Text;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;

namespace ShaderTools.CodeAnalysis.Hlsl.Classification
{
    internal sealed class SyntaxTaggerWorker
    {
        private readonly List<ClassifiedSpan> _results;
        private readonly CancellationToken _cancellationToken;

        public SyntaxTaggerWorker(TextSpan textSpan, List<ClassifiedSpan> results, CancellationToken cancellationToken)
        {
            _results = results;
            _cancellationToken = cancellationToken;
        }

        public void ClassifySyntax(SyntaxTree syntaxTree)
        {
            ClassifyNode((SyntaxNode) syntaxTree.Root);
        }

        private void ClassifyNode(SyntaxNode node)
        {
            _cancellationToken.ThrowIfCancellationRequested();

            foreach (var syntaxNodeOrToken in node.ChildNodes)
                ClassifyNodeOrToken((SyntaxNode) syntaxNodeOrToken);
        }

        private void ClassifyNodeOrToken(SyntaxNode nodeOrToken)
        {
            if (!nodeOrToken.IsToken)
                ClassifyNode(nodeOrToken);
            else
                ClassifyToken((SyntaxToken)nodeOrToken);
        }

        private void ClassifyToken(SyntaxToken token)
        {
            foreach (var trivia in token.LeadingTrivia)
                ClassifyTrivia(trivia);

            if (token.MacroReference != null)
            {
                foreach (var node in token.MacroReference.OriginalNodes)
                    ClassifyNodeOrToken(node);
            }
            else
            {
                var kind = GetClassificationForToken(token);
                if (kind != null)
                    AddClassification(token, kind);
            }

            foreach (var trivia in token.TrailingTrivia)
                ClassifyTrivia(trivia);
        }

        private void ClassifyTrivia(SyntaxNode trivia)
        {
            if (trivia.Kind == SyntaxKind.WhitespaceTrivia || trivia.Kind == SyntaxKind.EndOfLineTrivia)
                AddClassification((SyntaxTrivia) trivia, ClassificationTypeNames.WhiteSpace);
            else if (trivia.Kind.IsComment())
                AddClassification((SyntaxTrivia) trivia, ClassificationTypeNames.Comment);
            else if (trivia.Kind == SyntaxKind.DisabledTextTrivia)
                AddClassification((SyntaxTrivia) trivia, ClassificationTypeNames.ExcludedCode);
            else
                ClassifyNode(trivia);
        }

        private void AddClassification(LocatedNode node, string classificationType)
        {
            if (classificationType == null)
                throw new ArgumentNullException(nameof(classificationType));

            if (node.SourceRange.Length > 0 && node.FileSpan.File.IsRootFile)
                _results.Add(CreateClassifiedSpan(node, classificationType));
        }

        private static ClassifiedSpan CreateClassifiedSpan(LocatedNode node, string classificationType)
        {
            return new ClassifiedSpan(node.FileSpan.Span, classificationType);
        }

        private string GetClassificationForToken(SyntaxToken token)
        {
            if (token.Kind == SyntaxKind.IdentifierToken && token.ContextualKind == SyntaxKind.IdentifierToken)
                return ClassificationTypeNames.Identifier;

            if (IsInPreprocessorDirective(token) && (token.Kind == SyntaxKind.HashToken || token.ContextualKind.IsPreprocessorKeyword()))
                return ClassificationTypeNames.PreprocessorKeyword;

            if (token.Kind.IsKeyword() || token.ContextualKind.IsKeyword())
                return ClassificationTypeNames.Keyword;

            if (token.Kind.IsPunctuation())
                return HlslClassificationTypeNames.Punctuation;

            if (token.Kind.IsOperator())
                return ClassificationTypeNames.Operator;

            if (token.Kind.IsWhitespace())
                return ClassificationTypeNames.WhiteSpace;

            if (token.Kind.IsComment())
                return ClassificationTypeNames.Comment;

            if (token.Kind == SyntaxKind.StringLiteralToken || token.Kind == SyntaxKind.BracketedStringLiteralToken || token.Kind == SyntaxKind.CharacterLiteralToken)
                return ClassificationTypeNames.StringLiteral;

            if (token.Kind.IsNumericLiteral())
                return ClassificationTypeNames.NumericLiteral;

            return null;
        }

        private static bool IsInPreprocessorDirective(SyntaxToken token)
        {
            return token.Parent is DirectiveTriviaSyntax;
        }
    }
}