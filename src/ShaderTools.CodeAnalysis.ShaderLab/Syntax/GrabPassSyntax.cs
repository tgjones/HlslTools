using System.Collections.Generic;

namespace ShaderTools.CodeAnalysis.ShaderLab.Syntax
{
    public sealed class GrabPassSyntax : BasePassSyntax
    {
        public readonly SyntaxToken GrabPassKeyword;
        public readonly SyntaxToken OpenBraceToken;
        public readonly List<SyntaxNode> Statements;
        public readonly SyntaxToken CloseBraceToken;

        public GrabPassSyntax(SyntaxToken grabPassKeyword, SyntaxToken openBraceToken, List<SyntaxNode> statements, SyntaxToken closeBraceToken)
            : base(SyntaxKind.GrabPass)
        {
            RegisterChildNode(out GrabPassKeyword, grabPassKeyword);
            RegisterChildNode(out OpenBraceToken, openBraceToken);
            RegisterChildNodes(out Statements, statements);
            RegisterChildNode(out CloseBraceToken, closeBraceToken);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitGrabPass(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitGrabPass(this);
        }
    }
}