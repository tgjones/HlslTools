namespace ShaderTools.Hlsl.Syntax
{
    public class GenericMatrixTypeSyntax : BaseMatrixTypeSyntax
    {
        public readonly SyntaxToken MatrixKeyword;
        public readonly SyntaxToken LessThanToken;
        public readonly ScalarTypeSyntax ScalarType;
        public readonly SyntaxToken FirstCommaToken;
        public readonly SyntaxToken RowsToken;
        public readonly SyntaxToken SecondCommaToken;
        public readonly SyntaxToken ColsToken;
        public readonly SyntaxToken GreaterThanToken;

        public GenericMatrixTypeSyntax(SyntaxToken matrixKeyword, SyntaxToken lessThanToken, ScalarTypeSyntax scalarType, SyntaxToken firstCommaToken, SyntaxToken rowsToken, SyntaxToken secondCommaToken, SyntaxToken colsToken, SyntaxToken greaterThanToken)
            : base(SyntaxKind.PredefinedGenericMatrixType)
        {
            RegisterChildNode(out MatrixKeyword, matrixKeyword);
            RegisterChildNode(out LessThanToken, lessThanToken);
            RegisterChildNode(out ScalarType, scalarType);
            RegisterChildNode(out FirstCommaToken, firstCommaToken);
            RegisterChildNode(out RowsToken, rowsToken);
            RegisterChildNode(out SecondCommaToken, secondCommaToken);
            RegisterChildNode(out ColsToken, colsToken);
            RegisterChildNode(out GreaterThanToken, greaterThanToken);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitGenericMatrixType(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitGenericMatrixType(this);
        }
    }
}