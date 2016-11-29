using System.Collections.Generic;
using ShaderTools.Hlsl.Diagnostics;

namespace ShaderTools.Hlsl.Syntax
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