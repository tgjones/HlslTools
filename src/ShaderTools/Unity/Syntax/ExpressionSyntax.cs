using System.Collections.Generic;
using ShaderTools.Unity.Diagnostics;

namespace ShaderTools.Unity.Syntax
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