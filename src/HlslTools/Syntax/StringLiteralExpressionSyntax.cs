using System.Collections.Generic;

namespace HlslTools.Syntax
{
    public class StringLiteralExpressionSyntax : ExpressionSyntax
    {
        // String literals can have multiple strings separated by whitespace.
        public readonly List<SyntaxToken> Tokens;

        public StringLiteralExpressionSyntax(List<SyntaxToken> tokens)
            : base(SyntaxKind.StringLiteralExpression)
        {
            RegisterChildNodes(out Tokens, tokens);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitStringLiteralExpression(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitStringLiteralExpression(this);
        }
    }
}