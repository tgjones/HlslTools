namespace HlslTools.Syntax
{
    public class GenericVectorTypeSyntax : BaseVectorTypeSyntax
    {
        public readonly SyntaxToken VectorKeyword;
        public readonly SyntaxToken LessThanToken;
        public readonly ScalarTypeSyntax ScalarType;
        public readonly SyntaxToken CommaToken;
        public readonly SyntaxToken SizeToken;
        public readonly SyntaxToken GreaterThanToken;

        public GenericVectorTypeSyntax(SyntaxToken vectorKeyword, SyntaxToken lessThanToken, ScalarTypeSyntax scalarType, SyntaxToken commaToken, SyntaxToken sizeToken, SyntaxToken greaterThanToken)
            : base(SyntaxKind.PredefinedGenericVectorType)
        {
            RegisterChildNode(out VectorKeyword, vectorKeyword);
            RegisterChildNode(out LessThanToken, lessThanToken);
            RegisterChildNode(out ScalarType, scalarType);
            RegisterChildNode(out CommaToken, commaToken);
            RegisterChildNode(out SizeToken, sizeToken);
            RegisterChildNode(out GreaterThanToken, greaterThanToken);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitGenericVectorType(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitGenericVectorType(this);
        }
    }
}