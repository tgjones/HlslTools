using System.Collections.Generic;

namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    partial class AttributeDeclarationSyntax
    {
        public override IEnumerable<AttributeSyntax> GetAttributes()
        {
            yield return Attribute;
        }
    }
}
