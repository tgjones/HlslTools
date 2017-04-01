using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using ShaderTools.Hlsl.Syntax;

namespace ShaderTools.Editor.VisualStudio.Hlsl.Tagging.Classification
{
    internal sealed class SyntaxTaggerWorker
    {
        private readonly HlslClassificationService _classificationService;
        private readonly List<ITagSpan<IClassificationTag>> _results;
        private readonly ITextSnapshot _snapshot;
        private readonly CancellationToken _cancellationToken;

        public SyntaxTaggerWorker(HlslClassificationService classificationService, List<ITagSpan<IClassificationTag>> results, ITextSnapshot snapshot, CancellationToken cancellationToken)
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
                AddClassification((SyntaxTrivia) trivia, _classificationService.WhiteSpace);
            else if (trivia.Kind.IsComment())
                AddClassification((SyntaxTrivia)trivia, _classificationService.Comment);
            else if (trivia.Kind == SyntaxKind.DisabledTextTrivia)
                AddClassification((SyntaxTrivia)trivia, _classificationService.ExcludedCode);
            else
                ClassifyNode(trivia);
        }

        private void AddClassification(LocatedNode node, IClassificationType classificationType)
        {
            if (classificationType == null)
                throw new ArgumentNullException(nameof(classificationType));

            if (node.SourceRange.Length > 0 && node.Span.IsInRootFile)
                _results.Add(CreateClassificationTagSpan(node, classificationType));
        }

        private ITagSpan<IClassificationTag> CreateClassificationTagSpan(LocatedNode node, IClassificationType classificationType)
        {
            var snapshotSpan = new SnapshotSpan(_snapshot, node.Span.Start, node.Span.Length);
            var classificationTag = new ClassificationTag(classificationType);
            return new TagSpan<ClassificationTag>(snapshotSpan, classificationTag);
        }

        private IClassificationType GetClassificationForToken(SyntaxToken token)
        {
            if (token.Kind == SyntaxKind.IdentifierToken && token.ContextualKind == SyntaxKind.IdentifierToken)
                return _classificationService.Identifier;

            if (IsInPreprocessorDirective(token) && (token.Kind == SyntaxKind.HashToken || token.ContextualKind.IsPreprocessorKeyword()))
                return _classificationService.PreprocessorKeyword;

            if (token.Kind.IsKeyword() || token.ContextualKind.IsKeyword())
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

        private static bool IsInPreprocessorDirective(SyntaxToken token)
        {
            return token.Parent is DirectiveTriviaSyntax;
        }
    }
}