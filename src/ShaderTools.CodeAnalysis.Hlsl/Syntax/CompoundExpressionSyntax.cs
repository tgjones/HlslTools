namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    public class CompoundExpressionSyntax : ExpressionSyntax
    {
        public readonly ExpressionSyntax Left;
        public readonly SyntaxToken CommaToken;
        public readonly ExpressionSyntax Right;

        public CompoundExpressionSyntax(SyntaxKind kind, ExpressionSyntax left, SyntaxToken commaToken, ExpressionSyntax right)
            : base(kind)
        {
            RegisterChildNode(out Left, left);
            RegisterChildNode(out CommaToken, commaToken);
            RegisterChildNode(out Right, right);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitCompoundExpression(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitCompoundExpression(this);
        }
    }
}