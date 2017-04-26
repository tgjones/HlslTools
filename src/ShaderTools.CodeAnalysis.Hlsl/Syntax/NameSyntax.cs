using System.Collections.Generic;
using ShaderTools.CodeAnalysis.Diagnostics;

namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    public abstract class NameSyntax : TypeSyntax
    {
        protected NameSyntax(SyntaxKind kind)
            : base(kind)
        {
            
        }

        protected NameSyntax(SyntaxKind kind, IEnumerable<Diagnostic> diagnostics)
            : base(kind, diagnostics)
        {
            
        }

        public abstract IdentifierNameSyntax GetUnqualifiedName();
    }
}