namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    public class PackOffsetLocation : VariableDeclaratorQualifierSyntax
    {
        public readonly SyntaxToken ColonToken;
        public readonly SyntaxToken PackOffsetKeyword;
        public readonly SyntaxToken OpenParenToken;
        public readonly SyntaxToken Register;
        public readonly PackOffsetComponentPart ComponentPart;
        public readonly SyntaxToken CloseParenToken;

        public PackOffsetLocation(SyntaxToken colonToken, SyntaxToken packOffsetKeyword, SyntaxToken openParenToken, SyntaxToken register, PackOffsetComponentPart componentPart, SyntaxToken closeParenToken)
            : base(SyntaxKind.PackOffsetLocation)
        {
            RegisterChildNode(out ColonToken, colonToken);
            RegisterChildNode(out PackOffsetKeyword, packOffsetKeyword);
            RegisterChildNode(out OpenParenToken, openParenToken);
            RegisterChildNode(out Register, register);
            RegisterChildNode(out ComponentPart, componentPart);
            RegisterChildNode(out CloseParenToken, closeParenToken);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitPackOffsetLocation(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitPackOffsetLocation(this);
        }
    }
}