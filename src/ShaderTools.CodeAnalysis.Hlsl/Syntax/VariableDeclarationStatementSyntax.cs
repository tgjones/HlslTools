using System.Collections.Generic;

namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    public sealed partial class VariableDeclarationStatementSyntax : StatementSyntax
    {
        public VariableDeclarationStatementSyntax(VariableDeclarationSyntax variableDeclaration, SyntaxToken semicolonToken)
            : this(new List<AttributeDeclarationSyntaxBase>(), variableDeclaration, semicolonToken)
        {
        }
    }
}