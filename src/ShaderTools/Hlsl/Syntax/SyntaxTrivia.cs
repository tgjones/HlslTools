using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using ShaderTools.Hlsl.Diagnostics;
using ShaderTools.Hlsl.Text;

namespace ShaderTools.Hlsl.Syntax
{
    public sealed class SyntaxTrivia : LocatedNode
    {
        public override bool ContainsDiagnostics => Diagnostics.Any();
        public override IEnumerable<Diagnostic> GetDiagnostics() => Diagnostics;

        internal SyntaxTrivia(SyntaxKind kind, string text, SourceRange sourceRange, TextSpan span, ImmutableArray<Diagnostic> diagnostics)
            : base(kind, text, span, diagnostics)
        {
            SourceRange = sourceRange;
            FullSourceRange = sourceRange;
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitSyntaxTrivia(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitSyntaxTrivia(this);
        }

        protected internal override void WriteTo(StringBuilder sb, bool leading, bool trailing, bool includeNonRootFile, bool ignoreMacroReferences)
        {
            if (Span.IsInRootFile || includeNonRootFile)
                sb.Append(Text);
        }
    }
}