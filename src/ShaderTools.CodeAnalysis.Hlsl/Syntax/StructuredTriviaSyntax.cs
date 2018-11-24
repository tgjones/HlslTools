using System.Collections.Generic;
using ShaderTools.CodeAnalysis.Diagnostics;

namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    public abstract partial class StructuredTriviaSyntax : SyntaxNode
    {
        internal override bool IsStructuredTrivia => true;
    }
}