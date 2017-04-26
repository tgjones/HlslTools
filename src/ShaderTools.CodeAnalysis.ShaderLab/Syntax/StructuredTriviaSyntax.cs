using System.Collections.Generic;
using ShaderTools.CodeAnalysis.Diagnostics;

namespace ShaderTools.CodeAnalysis.ShaderLab.Syntax
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