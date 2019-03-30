using System.Collections.Generic;
using System.Collections.Immutable;
using ShaderTools.CodeAnalysis.Diagnostics;
using ShaderTools.CodeAnalysis.Syntax;

namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    public sealed partial class IdentifierNameSyntax : NameSyntax
    {
        public override IdentifierNameSyntax GetUnqualifiedName()
        {
            return this;
        }
    }
}