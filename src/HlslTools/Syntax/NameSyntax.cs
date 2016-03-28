using System.Collections.Generic;
using HlslTools.Diagnostics;

namespace HlslTools.Syntax
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