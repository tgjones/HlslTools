using System.Collections.Generic;
using ShaderTools.Hlsl.Diagnostics;

namespace ShaderTools.Hlsl.Syntax
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