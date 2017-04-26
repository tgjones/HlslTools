using System.Collections.Generic;

namespace ShaderTools.CodeAnalysis.ShaderLab.Syntax
{
    public sealed class CategorySyntax : SyntaxNode
    {
        public readonly SyntaxToken CategoryKeyword;
        public readonly SyntaxToken OpenBraceToken;
        public readonly List<SyntaxNode> Statements;
        public readonly SyntaxToken CloseBraceToken;

        public CategorySyntax(SyntaxToken categoryKeyword, SyntaxToken openBraceToken, List<SyntaxNode> statements, SyntaxToken closeBraceToken)
            : base(SyntaxKind.Category)
        {
            RegisterChildNode(out CategoryKeyword, categoryKeyword);
            RegisterChildNode(out OpenBraceToken, openBraceToken);
            RegisterChildNodes(out Statements, statements);
            RegisterChildNode(out CloseBraceToken, closeBraceToken);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitCategory(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitCategory(this);
        }
    }
}