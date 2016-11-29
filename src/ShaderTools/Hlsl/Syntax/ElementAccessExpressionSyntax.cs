namespace ShaderTools.Hlsl.Syntax
{
    /// <summary>
    /// array[index]
    /// </summary>
    public class ElementAccessExpressionSyntax : ExpressionSyntax
    {
        public readonly ExpressionSyntax Expression;
        public readonly SyntaxToken OpenBracketToken;
        public readonly ExpressionSyntax Index;
        public readonly SyntaxToken CloseBracketToken;

        public ElementAccessExpressionSyntax(ExpressionSyntax expression, SyntaxToken openBracketToken, ExpressionSyntax index, SyntaxToken closeBracketToken)
            : base(SyntaxKind.ElementAccessExpression)
        {
            RegisterChildNode(out Expression, expression);
            RegisterChildNode(out OpenBracketToken, openBracketToken);
            RegisterChildNode(out Index, index);
            RegisterChildNode(out CloseBracketToken, closeBracketToken);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitElementAccessExpression(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitElementAccessExpression(this);
        }
    }
}