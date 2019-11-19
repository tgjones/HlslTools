using System.Collections.Generic;

namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    public sealed partial class TypeDeclarationStatementSyntax : StatementSyntax
    {
        public TypeDeclarationStatementSyntax(List<SyntaxToken> modifiers, TypeDefinitionSyntax type, SyntaxToken semicolonToken)
            : this(new List<AttributeDeclarationSyntaxBase>(), modifiers, type, semicolonToken)
        {
        }
    }
}