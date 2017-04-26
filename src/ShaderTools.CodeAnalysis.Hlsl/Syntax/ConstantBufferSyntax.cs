using System.Collections.Generic;

namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    public class ConstantBufferSyntax : SyntaxNode
    {
        public readonly SyntaxToken ConstantBufferKeyword;
        public readonly SyntaxToken Name;
        public readonly RegisterLocation Register;
        public readonly SyntaxToken OpenBraceToken;
        public readonly List<VariableDeclarationStatementSyntax> Declarations;
        public readonly SyntaxToken CloseBraceToken;
        public readonly SyntaxToken SemicolonToken;

        public ConstantBufferSyntax(SyntaxToken constantBufferKeyword, SyntaxToken name, RegisterLocation register, SyntaxToken openBraceToken, List<VariableDeclarationStatementSyntax> declarations, SyntaxToken closeBraceToken, SyntaxToken semicolonToken)
            : base(SyntaxKind.ConstantBufferDeclaration)
        {
            RegisterChildNode(out ConstantBufferKeyword, constantBufferKeyword);
            RegisterChildNode(out Name, name);
            RegisterChildNode(out Register, register);
            RegisterChildNode(out OpenBraceToken, openBraceToken);
            RegisterChildNodes(out Declarations, declarations);
            RegisterChildNode(out CloseBraceToken, closeBraceToken);
            RegisterChildNode(out SemicolonToken, semicolonToken);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitConstantBuffer(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitConstantBuffer(this);
        }
    }
}