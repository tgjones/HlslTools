namespace ShaderTools.CodeAnalysis.ShaderLab.Syntax
{
    public sealed class Vector4Syntax : BaseVectorSyntax
    {
        public readonly SyntaxToken OpenParenToken;
        public readonly ExpressionSyntax X;
        public readonly SyntaxToken FirstCommaToken;
        public readonly ExpressionSyntax Y;
        public readonly SyntaxToken SecondCommaToken;
        public readonly ExpressionSyntax Z;
        public readonly SyntaxToken ThirdCommaToken;
        public readonly ExpressionSyntax W;
        public readonly SyntaxToken CloseParenToken;

        public Vector4Syntax(SyntaxToken openParenToken, ExpressionSyntax x, SyntaxToken firstCommaToken, ExpressionSyntax y, SyntaxToken secondCommaToken, ExpressionSyntax z, SyntaxToken thirdCommaToken, ExpressionSyntax w, SyntaxToken closeParenToken)
            : base(SyntaxKind.Vector4)
        {
            RegisterChildNode(out OpenParenToken, openParenToken);
            RegisterChildNode(out X, x);
            RegisterChildNode(out FirstCommaToken, firstCommaToken);
            RegisterChildNode(out Y, y);
            RegisterChildNode(out SecondCommaToken, secondCommaToken);
            RegisterChildNode(out Z, z);
            RegisterChildNode(out ThirdCommaToken, thirdCommaToken);
            RegisterChildNode(out W, w);
            RegisterChildNode(out CloseParenToken, closeParenToken);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitVector4(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitVector4(this);
        }
    }
}