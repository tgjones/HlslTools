using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using ShaderTools.Core.Diagnostics;
using ShaderTools.Core.Syntax;
using ShaderTools.Core.Parser;

namespace ShaderTools.Unity.Syntax
{
    public sealed class SyntaxToken : SyntaxNode
    {
        public SyntaxKind ContextualKind { get; }
        public object Value { get; }

        public string Text { get; }
        public string ValueText => Value as string ?? Text;

        public override bool IsMissing { get; }

        internal SyntaxToken(SyntaxKind kind, SyntaxKind contextualKind, 
            bool isMissing, SourceRange sourceRange, string text, object value,
            IEnumerable<SyntaxNode> leadingTrivia, IEnumerable<SyntaxNode> trailingTrivia, 
            IEnumerable<Diagnostic> diagnostics)
            : base(kind, diagnostics)
        {
            ContextualKind = contextualKind;

            IsMissing = isMissing;

            Text = text;
            SourceRange = sourceRange;
            Value = value;

            LeadingTrivia = leadingTrivia.ToImmutableArray();
            foreach (var triviaNode in LeadingTrivia)
                ((SyntaxNodeBase) triviaNode).Parent = this;

            TrailingTrivia = trailingTrivia.ToImmutableArray();
            foreach (var triviaNode in TrailingTrivia)
                ((SyntaxNodeBase) triviaNode).Parent = this;

            FullSourceRange = ComputeFullSpan(sourceRange, LeadingTrivia, TrailingTrivia);

            ContainsDiagnostics = Diagnostics.Any()
                || LeadingTrivia.Any(x => x.ContainsDiagnostics)
                || TrailingTrivia.Any(x => x.ContainsDiagnostics);
        }

        internal SyntaxToken(SyntaxKind kind, bool isMissing, SourceRange sourceRange)
            : this(kind, SyntaxKind.BadToken, isMissing, sourceRange, string.Empty, null, 
                  Enumerable.Empty<SyntaxNode>(),
                  Enumerable.Empty<SyntaxNode>(),
                  Enumerable.Empty<Diagnostic>())
        {
            
        }

        internal SyntaxToken(PretokenizedSyntaxToken pretokenizedToken, SourceLocation position)
            : base((SyntaxKind) pretokenizedToken.RawKind, ConvertDiagnostics(pretokenizedToken.Diagnostics, position))
        {
            ContextualKind = (SyntaxKind) pretokenizedToken.RawContextualKind;

            Text = pretokenizedToken.Text;
            Value = pretokenizedToken.Value;

            SourceRange = new SourceRange(position + pretokenizedToken.LeadingTriviaWidth, pretokenizedToken.Width);
            FullSourceRange = new SourceRange(position, pretokenizedToken.FullWidth);

            var triviaPosition = position;
            var convertedLeadingTrivia = new List<SyntaxNode>();
            foreach (var trivia in pretokenizedToken.LeadingTrivia)
            {
                convertedLeadingTrivia.Add(new SyntaxTrivia(
                    (SyntaxKind) trivia.RawKind,
                    trivia.Text,
                    new SourceRange(triviaPosition, trivia.FullWidth),
                    ConvertDiagnostics(trivia.Diagnostics, triviaPosition)));
                triviaPosition += trivia.FullWidth;
            }
            LeadingTrivia = convertedLeadingTrivia.ToImmutableArray();

            triviaPosition = SourceRange.End;
            var convertedTrailingTrivia = new List<SyntaxNode>();
            foreach (var trivia in pretokenizedToken.TrailingTrivia)
            {
                convertedTrailingTrivia.Add(new SyntaxTrivia(
                    (SyntaxKind) trivia.RawKind,
                    trivia.Text,
                    new SourceRange(triviaPosition, trivia.FullWidth),
                    ConvertDiagnostics(trivia.Diagnostics, triviaPosition)));
                triviaPosition += trivia.FullWidth;
            }
            TrailingTrivia = convertedTrailingTrivia.ToImmutableArray();

            ContainsDiagnostics = Diagnostics.Any()
                || LeadingTrivia.Any(x => x.ContainsDiagnostics)
                || TrailingTrivia.Any(x => x.ContainsDiagnostics);
        }

        private static ImmutableArray<Diagnostic> ConvertDiagnostics(ImmutableArray<PretokenizedDiagnostic> diagnostics, SourceLocation startPosition)
        {
            var result = new List<Diagnostic>();

            foreach (var diagnostic in diagnostics)
            {
                result.Add(new Diagnostic(
                    diagnostic.Descriptor,
                    new SourceRange(startPosition + diagnostic.Offset, diagnostic.Width),
                    diagnostic.Message));
            }

            return result.ToImmutableArray();
        }

        private static SourceRange ComputeFullSpan(SourceRange span, ImmutableArray<SyntaxNode> leadingTrivia, ImmutableArray<SyntaxNode> trailingTrivia)
        {
            var result = span;
            foreach (var childNode in leadingTrivia)
                result = SourceRange.Union(result, childNode.FullSourceRange);
            foreach (var childNode in trailingTrivia)
                result = SourceRange.Union(result, childNode.FullSourceRange);
            return result;
        }

        public override bool IsToken => true;
        public override bool ContainsDiagnostics { get; }
        public override IEnumerable<Diagnostic> GetDiagnostics() => Diagnostics
            .Union(LeadingTrivia.SelectMany(x => x.GetDiagnostics()))
            .Union(TrailingTrivia.SelectMany(x => x.GetDiagnostics()));

        public override void Accept(SyntaxVisitor visitor) => visitor.VisitSyntaxToken(this);
        public override T Accept<T>(SyntaxVisitor<T> visitor) => visitor.VisitSyntaxToken(this);
        public override string ToString() => Text;

        public ImmutableArray<SyntaxNode> LeadingTrivia { get; }
        public ImmutableArray<SyntaxNode> TrailingTrivia { get; }

        public override SyntaxNodeBase SetDiagnostics(ImmutableArray<Diagnostic> diagnostics)
        {
            return new SyntaxToken(Kind, ContextualKind, IsMissing, SourceRange, Text, Value, LeadingTrivia, TrailingTrivia, diagnostics);
        }

        public SyntaxToken WithLeadingTrivia(IEnumerable<SyntaxNode> trivia)
        {
            return new SyntaxToken(Kind, ContextualKind, IsMissing, SourceRange, Text, Value, trivia, TrailingTrivia, Diagnostics);
        }

        public SyntaxToken WithTrailingTrivia(IEnumerable<SyntaxNode> trivia)
        {
            return new SyntaxToken(Kind, ContextualKind, IsMissing, SourceRange, Text, Value, LeadingTrivia, trivia, Diagnostics);
        }

        public SyntaxToken WithKind(SyntaxKind kind)
        {
            return new SyntaxToken(kind, ContextualKind, IsMissing, SourceRange, Text, Value, LeadingTrivia, TrailingTrivia, Diagnostics);
        }

        public SyntaxToken WithContextualKind(SyntaxKind kind)
        {
            return new SyntaxToken(Kind, kind, IsMissing, SourceRange, Text, Value, LeadingTrivia, TrailingTrivia, Diagnostics);
        }

        protected internal override void WriteTo(StringBuilder sb, bool leading, bool trailing)
        {
            if (leading)
                foreach (var trivia in LeadingTrivia)
                    trivia.WriteTo(sb, true, true);

            sb.Append(Text);

            if (trailing)
                foreach (var trivia in TrailingTrivia)
                    trivia.WriteTo(sb, true, true);
        }
    }
}