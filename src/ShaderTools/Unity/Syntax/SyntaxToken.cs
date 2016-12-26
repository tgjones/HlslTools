using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using ShaderTools.Core.Diagnostics;
using ShaderTools.Core.Text;

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
            bool isMissing, TextSpan span, string text, object value,
            IEnumerable<SyntaxNode> leadingTrivia, IEnumerable<SyntaxNode> trailingTrivia, 
            IEnumerable<Diagnostic> diagnostics)
            : base(kind, diagnostics)
        {
            ContextualKind = contextualKind;

            IsMissing = isMissing;

            Text = text;
            Span = span;
            Value = value;

            LeadingTrivia = leadingTrivia.ToImmutableArray();
            foreach (var triviaNode in LeadingTrivia)
                triviaNode.Parent = this;

            TrailingTrivia = trailingTrivia.ToImmutableArray();
            foreach (var triviaNode in TrailingTrivia)
                triviaNode.Parent = this;

            FullSpan = ComputeFullSpan(span, LeadingTrivia, TrailingTrivia);

            ContainsDiagnostics = Diagnostics.Any()
                || LeadingTrivia.Any(x => x.ContainsDiagnostics)
                || TrailingTrivia.Any(x => x.ContainsDiagnostics);
        }

        internal SyntaxToken(SyntaxKind kind, bool isMissing, TextSpan span)
            : this(kind, SyntaxKind.BadToken, isMissing, span, string.Empty, null, 
                  Enumerable.Empty<SyntaxNode>(),
                  Enumerable.Empty<SyntaxNode>(),
                  Enumerable.Empty<Diagnostic>())
        {
            
        }

        private static TextSpan ComputeFullSpan(TextSpan span, ImmutableArray<SyntaxNode> leadingTrivia, ImmutableArray<SyntaxNode> trailingTrivia)
        {
            var result = span;
            foreach (var childNode in leadingTrivia)
                result = TextSpan.Union(result, childNode.FullSpan);
            foreach (var childNode in trailingTrivia)
                result = TextSpan.Union(result, childNode.FullSpan);
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

        public override SyntaxNode SetDiagnostics(ImmutableArray<Diagnostic> diagnostics)
        {
            return new SyntaxToken(Kind, ContextualKind, IsMissing, Span, Text, Value, LeadingTrivia, TrailingTrivia, diagnostics);
        }

        public SyntaxToken WithLeadingTrivia(IEnumerable<SyntaxNode> trivia)
        {
            return new SyntaxToken(Kind, ContextualKind, IsMissing, Span, Text, Value, trivia, TrailingTrivia, Diagnostics);
        }

        public SyntaxToken WithTrailingTrivia(IEnumerable<SyntaxNode> trivia)
        {
            return new SyntaxToken(Kind, ContextualKind, IsMissing, Span, Text, Value, LeadingTrivia, trivia, Diagnostics);
        }

        public SyntaxToken WithKind(SyntaxKind kind)
        {
            return new SyntaxToken(kind, ContextualKind, IsMissing, Span, Text, Value, LeadingTrivia, TrailingTrivia, Diagnostics);
        }

        public SyntaxToken WithContextualKind(SyntaxKind kind)
        {
            return new SyntaxToken(Kind, kind, IsMissing, Span, Text, Value, LeadingTrivia, TrailingTrivia, Diagnostics);
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