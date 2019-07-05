using System.Collections.Generic;

namespace ShaderTools.CodeAnalysis.ShaderLab.Syntax
{
    public sealed class PassSyntax : BasePassSyntax
    {
        public readonly SyntaxToken PassKeyword;
        public readonly SyntaxToken OpenBraceToken;
        public readonly List<SyntaxNode> Statements;
        public readonly ShaderProgramSyntax ShaderProgram;
        public readonly SyntaxToken CloseBraceToken;

        public PassSyntax(SyntaxToken passKeyword, SyntaxToken openBraceToken, List<SyntaxNode> statements, ShaderProgramSyntax shaderProgram, SyntaxToken closeBraceToken)
            : base(SyntaxKind.Pass)
        {
            RegisterChildNode(out PassKeyword, passKeyword);
            RegisterChildNode(out OpenBraceToken, openBraceToken);
            RegisterChildNodes(out Statements, statements);
            RegisterChildNode(out ShaderProgram, shaderProgram);
            RegisterChildNode(out CloseBraceToken, closeBraceToken);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitPass(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitPass(this);
        }
    }
}