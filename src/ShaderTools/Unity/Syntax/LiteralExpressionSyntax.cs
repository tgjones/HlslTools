namespace ShaderTools.Unity.Syntax
{
    /// <summary>
    /// Float, integer, boolean, etc. literal constant.
    /// </summary>
    public class LiteralExpressionSyntax : ExpressionSyntax
    {
        public readonly SyntaxToken Token;

        public LiteralExpressionSyntax(SyntaxKind kind, SyntaxToken token)
            : base(kind)
        {
            RegisterChildNode(out Token, token);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitLiteralExpression(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitLiteralExpression(this);
        }
    }
}