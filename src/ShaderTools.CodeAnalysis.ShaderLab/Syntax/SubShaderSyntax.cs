using System.Collections.Generic;

namespace ShaderTools.CodeAnalysis.ShaderLab.Syntax
{
    public sealed class SubShaderSyntax : SyntaxNode
    {
        public readonly SyntaxToken SubShaderKeyword;
        public readonly SyntaxToken OpenBraceToken;
        public readonly List<SyntaxNode> Statements;
        public readonly SyntaxToken CloseBraceToken;

        public SubShaderSyntax(SyntaxToken subShaderKeyword, SyntaxToken openBraceToken, List<SyntaxNode> statements, SyntaxToken closeBraceToken)
            : base(SyntaxKind.SubShader)
        {
            RegisterChildNode(out SubShaderKeyword, subShaderKeyword);
            RegisterChildNode(out OpenBraceToken, openBraceToken);
            RegisterChildNodes(out Statements, statements);
            RegisterChildNode(out CloseBraceToken, closeBraceToken);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitSubShader(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitSubShader(this);
        }
    }
}