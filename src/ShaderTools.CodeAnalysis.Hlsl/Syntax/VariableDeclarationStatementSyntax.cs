using System.Collections.Generic;
using System.Collections.Immutable;
using ShaderTools.CodeAnalysis.Diagnostics;
using ShaderTools.CodeAnalysis.Syntax;

namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    public sealed partial class VariableDeclarationStatementSyntax : StatementSyntax
    {
        public VariableDeclarationStatementSyntax(VariableDeclarationSyntax variableDeclaration, SyntaxToken semicolonToken)
            : this(new List<AttributeSyntax>(), variableDeclaration, semicolonToken)
        {
        }

        public VariableDeclarationStatementSyntax(VariableDeclarationSyntax variableDeclaration, SyntaxToken semicolonToken, ImmutableArray<Diagnostic> diagnostics)
            : this(new List<AttributeSyntax>(), variableDeclaration, semicolonToken, diagnostics)
        {
        }
    }
}