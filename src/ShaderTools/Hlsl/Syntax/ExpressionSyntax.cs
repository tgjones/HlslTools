using System.Collections.Generic;
using ShaderTools.Hlsl.Diagnostics;

namespace ShaderTools.Hlsl.Syntax
{
    public abstract class ExpressionSyntax : SyntaxNode
    {
        public TypeSyntax ExpressionType { get; internal set; }

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