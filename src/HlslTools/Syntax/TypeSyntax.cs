using System.Collections.Generic;
using HlslTools.Diagnostics;

namespace HlslTools.Syntax
{
    public abstract class TypeSyntax : ExpressionSyntax
    {
        protected TypeSyntax(SyntaxKind kind, IEnumerable<Diagnostic> diagnostics)
            : base(kind, diagnostics)
        {
        }

        protected TypeSyntax(SyntaxKind kind)
            : base(kind)
        {
        }
    }
}