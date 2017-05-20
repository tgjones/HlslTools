using System.Collections.Generic;
using ShaderTools.CodeAnalysis.Diagnostics;

namespace ShaderTools.CodeAnalysis.ShaderLab.Syntax
{
    public abstract class StructuredTriviaSyntax : SyntaxNode
    {
        internal override bool IsStructuredTrivia => true;

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