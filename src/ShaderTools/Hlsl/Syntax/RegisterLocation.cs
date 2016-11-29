namespace ShaderTools.Hlsl.Syntax
{
    public class RegisterLocation : VariableDeclaratorQualifierSyntax
    {
        public readonly SyntaxToken Colon;
        public readonly SyntaxToken RegisterKeyword;
        public readonly SyntaxToken OpenParenToken;
        public readonly SyntaxToken Register;
        public readonly LogicalRegisterSpace LogicalRegisterSpace;
        public readonly SyntaxToken CloseParenToken;

        public RegisterLocation(SyntaxToken colon, SyntaxToken registerKeyword, SyntaxToken openParenToken, SyntaxToken register, LogicalRegisterSpace logicalRegisterSpace, SyntaxToken closeParenToken)
            : base(SyntaxKind.RegisterLocation)
        {
            RegisterChildNode(out Colon, colon);
            RegisterChildNode(out RegisterKeyword, registerKeyword);
            RegisterChildNode(out OpenParenToken, openParenToken);
            RegisterChildNode(out Register, register);
            RegisterChildNode(out LogicalRegisterSpace, logicalRegisterSpace);
            RegisterChildNode(out CloseParenToken, closeParenToken);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitRegisterLocation(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitRegisterLocation(this);
        }
    }
}