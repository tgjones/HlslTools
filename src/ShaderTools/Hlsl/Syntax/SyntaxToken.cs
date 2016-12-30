using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using ShaderTools.Core.Diagnostics;
using ShaderTools.Core.Syntax;
using ShaderTools.Core.Text;
using ShaderTools.Hlsl.Parser;

namespace ShaderTools.Hlsl.Syntax
{
    public sealed class SyntaxToken : LocatedNode
    {
        public SyntaxKind ContextualKind { get; }
        public object Value { get; }

        public string ValueText => Value as string ?? Text;

        public override bool IsMissing { get; }

        internal SyntaxToken(SyntaxKind kind, SyntaxKind contextualKind, 
            bool isMissing, SourceRange sourceRange, TextSpan span, string text, object value,
            IEnumerable<SyntaxNode> leadingTrivia, IEnumerable<SyntaxNode> trailingTrivia, 
            IEnumerable<Diagnostic> diagnostics,
            MacroReference macroReference, bool isFirstTokenInMacroExpansion)
            : base(kind, text, span, diagnostics)
        {
            ContextualKind = contextualKind;

            IsMissing = isMissing;

            SourceRange = sourceRange;
            Value = value;

            LeadingTrivia = leadingTrivia.ToImmutableArray();
            foreach (var triviaNode in LeadingTrivia)
                triviaNode.Parent = this;

            TrailingTrivia = trailingTrivia.ToImmutableArray();
            foreach (var triviaNode in TrailingTrivia)
                triviaNode.Parent = this;

            FullSourceRange = ComputeFullSpan(sourceRange, LeadingTrivia, TrailingTrivia);

            ContainsDiagnostics = Diagnostics.Any()
                || LeadingTrivia.Any(x => x.ContainsDiagnostics)
                || TrailingTrivia.Any(x => x.ContainsDiagnostics);

            ContainsDirectives = LeadingTrivia.OfType<DirectiveTriviaSyntax>().Any()
                || TrailingTrivia.OfType<DirectiveTriviaSyntax>().Any();

            MacroReference = macroReference;
            IsFirstTokenInMacroExpansion = isFirstTokenInMacroExpansion;
        }

        internal SyntaxToken(SyntaxKind kind, bool isMissing, SourceRange sourceRange, TextSpan span)
            : this(kind, SyntaxKind.BadToken, isMissing, sourceRange, span, string.Empty, null, 
                  Enumerable.Empty<SyntaxNode>(),
                  Enumerable.Empty<SyntaxNode>(),
                  Enumerable.Empty<Diagnostic>(),
                  null, false)
        {
            
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

        public override bool ContainsDirectives { get; }

        public override IEnumerable<DirectiveTriviaSyntax> GetDirectives()
        {
            return LeadingTrivia.OfType<DirectiveTriviaSyntax>().Union(TrailingTrivia.OfType<DirectiveTriviaSyntax>());
        }

        public override void Accept(SyntaxVisitor visitor) => visitor.VisitSyntaxToken(this);
        public override T Accept<T>(SyntaxVisitor<T> visitor) => visitor.VisitSyntaxToken(this);
        public override string ToString() => Text;

        public ImmutableArray<SyntaxNode> LeadingTrivia { get; }
        public ImmutableArray<SyntaxNode> TrailingTrivia { get; }

        public MacroReference MacroReference { get; }
        public bool IsFirstTokenInMacroExpansion { get; }

        public override SyntaxNode SetDiagnostics(ImmutableArray<Diagnostic> diagnostics)
        {
            return new SyntaxToken(Kind, ContextualKind, IsMissing, SourceRange, Span, Text, Value, LeadingTrivia, TrailingTrivia, diagnostics, MacroReference, IsFirstTokenInMacroExpansion);
        }

        public SyntaxToken WithLeadingTrivia(IEnumerable<SyntaxNode> trivia)
        {
            return new SyntaxToken(Kind, ContextualKind, IsMissing, SourceRange, Span, Text, Value, trivia, TrailingTrivia, Diagnostics, MacroReference, IsFirstTokenInMacroExpansion);
        }

        public SyntaxToken WithTrailingTrivia(IEnumerable<SyntaxNode> trivia)
        {
            return new SyntaxToken(Kind, ContextualKind, IsMissing, SourceRange, Span, Text, Value, LeadingTrivia, trivia, Diagnostics, MacroReference, IsFirstTokenInMacroExpansion);
        }

        public SyntaxToken WithOriginalMacroReference(MacroReference macroReference, bool isFirstTokenInMacroExpansion)
        {
            return new SyntaxToken(Kind, ContextualKind, IsMissing, SourceRange, Span, Text, Value, LeadingTrivia, TrailingTrivia, Diagnostics, macroReference, isFirstTokenInMacroExpansion);
        }

        public SyntaxToken WithKind(SyntaxKind kind)
        {
            return new SyntaxToken(kind, ContextualKind, IsMissing, SourceRange, Span, Text, Value, LeadingTrivia, TrailingTrivia, Diagnostics, MacroReference, IsFirstTokenInMacroExpansion);
        }

        public SyntaxToken WithContextualKind(SyntaxKind kind)
        {
            return new SyntaxToken(Kind, kind, IsMissing, SourceRange, Span, Text, Value, LeadingTrivia, TrailingTrivia, Diagnostics, MacroReference, IsFirstTokenInMacroExpansion);
        }

        public SyntaxToken WithSpan(SourceRange sourceRange, TextSpan span)
        {
            return new SyntaxToken(Kind, ContextualKind, IsMissing, sourceRange, span, Text, Value, LeadingTrivia, TrailingTrivia, Diagnostics, MacroReference, IsFirstTokenInMacroExpansion);
        }

        internal override DirectiveStack ApplyDirectives(DirectiveStack stack)
        {
            if (ContainsDirectives)
            {
                foreach (var trivia in LeadingTrivia.Where(x => x.ContainsDirectives))
                    stack = trivia.ApplyDirectives(stack);

                foreach (var trivia in TrailingTrivia.Where(x => x.ContainsDirectives))
                    stack = trivia.ApplyDirectives(stack);
            }

            return stack;
        }

        protected internal override void WriteTo(StringBuilder sb, bool leading, bool trailing, bool includeNonRootFile, bool ignoreMacroReferences)
        {
            if (!ignoreMacroReferences && MacroReference != null && !IsFirstTokenInMacroExpansion)
                return;

            if ((ignoreMacroReferences || MacroReference == null) && leading)
                foreach (var trivia in LeadingTrivia)
                    trivia.WriteTo(sb, true, true, includeNonRootFile, ignoreMacroReferences);

            if (!ignoreMacroReferences && MacroReference != null)
            {
                MacroReference.WriteTo(sb, leading, trailing, includeNonRootFile, ignoreMacroReferences);
            }
            else if (Span.IsInRootFile || includeNonRootFile)
            {
                sb.Append(Text);
            }

            if (MacroReference == null && trailing)
                foreach (var trivia in TrailingTrivia)
                    trivia.WriteTo(sb, true, true, includeNonRootFile, ignoreMacroReferences);
        }
    }
}