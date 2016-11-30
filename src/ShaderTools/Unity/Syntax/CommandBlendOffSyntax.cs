namespace ShaderTools.Unity.Syntax
{
    public sealed class CommandBlendOffSyntax : CommandSyntax
    {
        public readonly SyntaxToken BlendKeyword;
        public readonly SyntaxToken OffIdentifier;

        public CommandBlendOffSyntax(SyntaxToken blendKeyword, SyntaxToken offIdentifier)
            : base(SyntaxKind.CommandBlendOff)
        {
            RegisterChildNode(out BlendKeyword, blendKeyword);
            RegisterChildNode(out OffIdentifier, offIdentifier);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitCommandBlendOff(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitCommandBlendOff(this);
        }
    }
}