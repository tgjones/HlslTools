using System.Collections.Generic;
using HlslTools.Diagnostics;

namespace HlslTools.Syntax
{
    public abstract class StructuredTriviaSyntax : SyntaxNode
    {
        protected StructuredTriviaSyntax(SyntaxKind kind, IEnumerable<Diagnostic> diagnostics)
            : base(kind, diagnostics)
        {
        }

        protected StructuredTriviaSyntax(SyntaxKind kind)
            : base(kind)
        {
        }
    }
}