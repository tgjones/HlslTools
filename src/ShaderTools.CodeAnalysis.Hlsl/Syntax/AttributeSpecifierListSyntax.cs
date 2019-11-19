using System.Collections.Generic;

namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    partial class AttributeSpecifierListSyntax
    {
        public override IEnumerable<AttributeSyntax> GetAttributes()
        {
            return Attributes;
        }
    }
}
