using ShaderTools.Core.Syntax;

namespace ShaderTools.Hlsl.Syntax
{
    public class ArrayInitializerExpressionSyntax : ExpressionSyntax
    {
        public readonly SyntaxToken OpenBraceToken;
        public readonly SeparatedSyntaxList<ExpressionSyntax> Elements;
        public readonly SyntaxToken CloseBraceToken;

        public ArrayInitializerExpressionSyntax(SyntaxToken openBraceToken, SeparatedSyntaxList<ExpressionSyntax> elements, SyntaxToken closeBraceToken)
            : base(SyntaxKind.ArrayInitializerExpression)
        {
            RegisterChildNode(out OpenBraceToken, openBraceToken);
            RegisterChildNodes(out Elements, elements);
            RegisterChildNode(out CloseBraceToken, closeBraceToken);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitArrayInitializerExpression(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitArrayInitializerExpression(this);
        }
    }
}