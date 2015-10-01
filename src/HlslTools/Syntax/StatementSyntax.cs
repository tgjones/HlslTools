using System.Collections.Generic;
using System.Collections.Immutable;
using HlslTools.Diagnostics;

namespace HlslTools.Syntax
{
    public abstract class StatementSyntax : SyntaxNode
    {
        public readonly List<AttributeSyntax> Attributes;

        protected StatementSyntax(SyntaxKind kind, List<AttributeSyntax> attributes)
            : base(kind)
        {
            RegisterChildNodes(out Attributes, attributes);
        }

        protected StatementSyntax(SyntaxKind kind, List<AttributeSyntax> attributes, ImmutableArray<Diagnostic> diagnostics)
            : base(kind, diagnostics)
        {
            RegisterChildNodes(out Attributes, attributes);
        }
    }
}