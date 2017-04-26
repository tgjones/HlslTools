using System.Collections.Generic;
using System.Collections.Immutable;
using ShaderTools.CodeAnalysis.Diagnostics;
using ShaderTools.CodeAnalysis.Syntax;

namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    public class VariableDeclarationStatementSyntax : StatementSyntax
    {
        public readonly VariableDeclarationSyntax Declaration;
        public readonly SyntaxToken SemicolonToken;

        public VariableDeclarationStatementSyntax(VariableDeclarationSyntax variableDeclaration, SyntaxToken semicolonToken)
            : base(SyntaxKind.VariableDeclarationStatement, new List<AttributeSyntax>())
        {
            RegisterChildNode(out Declaration, variableDeclaration);
            RegisterChildNode(out SemicolonToken, semicolonToken);
        }

        public VariableDeclarationStatementSyntax(VariableDeclarationSyntax variableDeclaration, SyntaxToken semicolonToken, ImmutableArray<Diagnostic> diagnostics)
            : base(SyntaxKind.VariableDeclarationStatement, new List<AttributeSyntax>(), diagnostics)
        {
            RegisterChildNode(out Declaration, variableDeclaration);
            RegisterChildNode(out SemicolonToken, semicolonToken);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitVariableDeclarationStatement(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitVariableDeclarationStatement(this);
        }

        public override SyntaxNodeBase SetDiagnostics(ImmutableArray<Diagnostic> diagnostics)
        {
            return new VariableDeclarationStatementSyntax(Declaration, SemicolonToken, diagnostics);
        }
    }
}