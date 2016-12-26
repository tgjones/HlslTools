using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using ShaderTools.Core.Diagnostics;
using ShaderTools.Core.Text;

namespace ShaderTools.Unity.Syntax
{
    public sealed class SyntaxTrivia : SyntaxNode
    {
        public override bool ContainsDiagnostics => Diagnostics.Any();
        public override IEnumerable<Diagnostic> GetDiagnostics() => Diagnostics;

        public string Text { get; }

        internal SyntaxTrivia(SyntaxKind kind, string text, TextSpan span, ImmutableArray<Diagnostic> diagnostics)
            : base(kind, diagnostics)
        {
            Text = text;
            Span = span;
            FullSpan = span;
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitSyntaxTrivia(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitSyntaxTrivia(this);
        }

        protected internal override void WriteTo(StringBuilder sb, bool leading, bool trailing)
        {
            sb.Append(Text);
        }
    }
}