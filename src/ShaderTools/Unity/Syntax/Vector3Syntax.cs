namespace ShaderTools.Unity.Syntax
{
    public sealed class Vector3Syntax : BaseVectorSyntax
    {
        public readonly SyntaxToken OpenParenToken;
        public readonly ExpressionSyntax X;
        public readonly SyntaxToken FirstCommaToken;
        public readonly ExpressionSyntax Y;
        public readonly SyntaxToken SecondCommaToken;
        public readonly ExpressionSyntax Z;
        public readonly SyntaxToken CloseParenToken;

        public Vector3Syntax(SyntaxToken openParenToken, ExpressionSyntax x, SyntaxToken firstCommaToken, ExpressionSyntax y, SyntaxToken secondCommaToken, ExpressionSyntax z, SyntaxToken closeParenToken)
            : base (SyntaxKind.Vector3)
        {
            RegisterChildNode(out OpenParenToken, openParenToken);
            RegisterChildNode(out X, x);
            RegisterChildNode(out FirstCommaToken, firstCommaToken);
            RegisterChildNode(out Y, y);
            RegisterChildNode(out SecondCommaToken, secondCommaToken);
            RegisterChildNode(out Z, z);
            RegisterChildNode(out CloseParenToken, closeParenToken);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitVector3(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitVector3(this);
        }
    }
}