using System.Collections.Generic;
using System.Collections.Immutable;
using ShaderTools.Core.Diagnostics;

namespace ShaderTools.Hlsl.Syntax
{
    public sealed class IdentifierNameSyntax : NameSyntax
    {
        public readonly SyntaxToken Name;

        public IdentifierNameSyntax(SyntaxToken name)
            : base(SyntaxKind.IdentifierName)
        {
            RegisterChildNode(out Name, name);
        }

        public IdentifierNameSyntax(SyntaxToken name, IEnumerable<Diagnostic> diagnostics)
            : base(SyntaxKind.IdentifierName, diagnostics)
        {
            RegisterChildNode(out Name, name);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitIdentifierName(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitIdentifierName(this);
        }

        public override SyntaxNode SetDiagnostics(ImmutableArray<Diagnostic> diagnostics)
        {
            return new IdentifierNameSyntax(Name, diagnostics);
        }

        public override IdentifierNameSyntax GetUnqualifiedName()
        {
            return this;
        }
    }
}