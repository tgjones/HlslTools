using System.Collections.Generic;
using ShaderTools.CodeAnalysis.Diagnostics;

namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    public abstract partial class NameSyntax : TypeSyntax
    {
        public abstract IdentifierNameSyntax GetUnqualifiedName();
    }
}