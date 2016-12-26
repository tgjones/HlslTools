using System.Collections.Generic;
using ShaderTools.Core.Diagnostics;

namespace ShaderTools.Hlsl.Syntax
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