using System.Collections.Generic;
using ShaderTools.CodeAnalysis.Diagnostics;

namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    public abstract class ExpressionSyntax : SyntaxNode
    {
        protected ExpressionSyntax(SyntaxKind kind, IEnumerable<Diagnostic> diagnostics)
            : base(kind, diagnostics)
        {

        }

        protected ExpressionSyntax(SyntaxKind kind)
            : base(kind)
        {
            
        }
    }
}