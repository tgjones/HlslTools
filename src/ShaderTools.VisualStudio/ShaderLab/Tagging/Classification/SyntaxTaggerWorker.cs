using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using ShaderTools.Unity.Syntax;

namespace ShaderTools.VisualStudio.ShaderLab.Tagging.Classification
{
    internal sealed class SyntaxTaggerWorker
    {
        private readonly ShaderLabClassificationService _classificationService;
        private readonly List<ITagSpan<IClassificationTag>> _results;
        private readonly ITextSnapshot _snapshot;
        private readonly CancellationToken _cancellationToken;

        public SyntaxTaggerWorker(ShaderLabClassificationService classificationService, List<ITagSpan<IClassificationTag>> results, ITextSnapshot snapshot, CancellationToken cancellationToken)
        {
            _classificationService = classificationService;
            _results = results;
            _snapshot = snapshot;
            _cancellationToken = cancellationToken;
        }

        public void ClassifySyntax(SyntaxTree syntaxTree)
        {
            ClassifyNode(syntaxTree.Root);
        }

        private void ClassifyNode(SyntaxNode node)
        {
            _cancellationToken.ThrowIfCancellationRequested();

            foreach (var syntaxNodeOrToken in node.ChildNodes)
                ClassifyNodeOrToken(syntaxNodeOrToken);
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

            var kind = GetClassificationForToken(token);
            if (kind != null)
                AddClassification(token, kind);

            foreach (var trivia in token.TrailingTrivia)
                ClassifyTrivia(trivia);
        }

        private void ClassifyTrivia(SyntaxNode trivia)
        {
            if (trivia.Kind == SyntaxKind.WhitespaceTrivia || trivia.Kind == SyntaxKind.EndOfLineTrivia)
                AddClassification((SyntaxTrivia) trivia, _classificationService.WhiteSpace);
            else if (trivia.Kind.IsComment())
                AddClassification((SyntaxTrivia)trivia, _classificationService.Comment);
            else
                ClassifyNode(trivia);
        }

        private void AddClassification(SyntaxNode node, IClassificationType classificationType)
        {
            if (classificationType == null)
                throw new ArgumentNullException(nameof(classificationType));

            if (node.Span.Length > 0)
                _results.Add(CreateClassificationTagSpan(node, classificationType));
        }

        private ITagSpan<IClassificationTag> CreateClassificationTagSpan(SyntaxNode node, IClassificationType classificationType)
        {
            var snapshotSpan = new SnapshotSpan(_snapshot, node.Span.Start, node.Span.Length);
            var classificationTag = new ClassificationTag(classificationType);
            return new TagSpan<ClassificationTag>(snapshotSpan, classificationTag);
        }

        private IClassificationType GetClassificationForToken(SyntaxToken token)
        {
            if (token.Kind == SyntaxKind.IdentifierToken && token.ContextualKind == SyntaxKind.IdentifierToken)
                return _classificationService.Identifier;

            if (token.Kind.IsKeyword())
                return _classificationService.Keyword;

            if (token.Kind.IsPunctuation())
                return _classificationService.Punctuation;

            if (token.Kind.IsOperator())
                return _classificationService.Operator;

            if (token.Kind.IsWhitespace())
                return _classificationService.WhiteSpace;

            if (token.Kind.IsComment())
                return _classificationService.Comment;

            if (token.Kind == SyntaxKind.StringLiteralToken || token.Kind == SyntaxKind.BracketedStringLiteralToken)
                return _classificationService.StringLiteral;

            if (token.Kind.IsNumericLiteral())
                return _classificationService.NumberLiteral;

            return null;
        }
    }
}