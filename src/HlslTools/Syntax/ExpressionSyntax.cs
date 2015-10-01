using System.Collections.Generic;
using HlslTools.Diagnostics;

namespace HlslTools.Syntax
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