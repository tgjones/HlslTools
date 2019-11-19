using System.Collections.Generic;

namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    partial class AttributeDeclarationSyntaxBase
    {
        public abstract IEnumerable<AttributeSyntax> GetAttributes();
    }
}
