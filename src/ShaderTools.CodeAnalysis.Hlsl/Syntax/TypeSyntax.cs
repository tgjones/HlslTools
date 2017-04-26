using System.Collections.Generic;
using ShaderTools.CodeAnalysis.Diagnostics;

namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
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