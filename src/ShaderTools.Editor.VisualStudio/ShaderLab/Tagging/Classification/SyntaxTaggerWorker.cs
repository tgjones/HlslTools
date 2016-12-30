using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using ShaderTools.Core.Text;
using ShaderTools.Unity.Syntax;

namespace ShaderTools.Editor.VisualStudio.ShaderLab.Tagging.Classification
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
            ClassifyNode(syntaxTree, syntaxTree.Root);
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
                AddClassification(syntaxTree, (SyntaxTrivia) trivia, _classificationService.WhiteSpace);
            else if (trivia.Kind.IsComment())
                AddClassification(syntaxTree, (SyntaxTrivia)trivia, _classificationService.Comment);
            else
                ClassifyNode(syntaxTree, trivia);
        }

        private void AddClassification(SyntaxTree syntaxTree, SyntaxNode node, IClassificationType classificationType)
        {
            if (classificationType == null)
                throw new ArgumentNullException(nameof(classificationType));

            if (node.SourceRange.Length > 0)
            {
                var textSpan = syntaxTree.GetSourceTextSpan(node.SourceRange);
                _results.Add(CreateClassificationTagSpan(textSpan, node, classificationType));
            }
        }

        private ITagSpan<IClassificationTag> CreateClassificationTagSpan(TextSpan span, SyntaxNode node, IClassificationType classificationType)
        {
            var snapshotSpan = new SnapshotSpan(_snapshot, span.Start, span.Length);
            var classificationTag = new ClassificationTag(classificationType);
            return new TagSpan<ClassificationTag>(snapshotSpan, classificationTag);
        }

        private IClassificationType GetClassificationForToken(SyntaxToken token)
        {
            if (token.Parent != null 
                && token.Parent.Kind == SyntaxKind.ShaderProperty 
                && token == ((ShaderPropertySyntax) token.Parent).NameToken)
                return _classificationService.ShaderProperty;

            if (token.Kind == SyntaxKind.IdentifierToken
                && token.Parent != null
                && token.Parent.Kind == SyntaxKind.ShaderPropertyAttribute
                && token == ((ShaderPropertyAttributeSyntax) token.Parent).Name)
                return _classificationService.Attribute;

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