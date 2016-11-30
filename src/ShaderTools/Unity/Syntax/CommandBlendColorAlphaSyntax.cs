namespace ShaderTools.Unity.Syntax
{
    public sealed class CommandBlendColorAlphaSyntax : CommandSyntax
    {
        public readonly SyntaxToken BlendKeyword;
        public readonly CommandValueSyntax SrcFactor;
        public readonly CommandValueSyntax DstFactor;
        public readonly SyntaxToken CommaToken;
        public readonly CommandValueSyntax SrcFactorA;
        public readonly CommandValueSyntax DstFactorA;

        public CommandBlendColorAlphaSyntax(SyntaxToken blendKeyword, CommandValueSyntax srcFactor, CommandValueSyntax dstFactor, SyntaxToken commaToken, CommandValueSyntax srcFactorA, CommandValueSyntax dstFactorA)
            : base(SyntaxKind.CommandBlendColorAlpha)
        {
            RegisterChildNode(out BlendKeyword, blendKeyword);
            RegisterChildNode(out SrcFactor, srcFactor);
            RegisterChildNode(out DstFactor, dstFactor);
            RegisterChildNode(out CommaToken, commaToken);
            RegisterChildNode(out SrcFactorA, srcFactorA);
            RegisterChildNode(out DstFactorA, dstFactorA);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitCommandBlendColorAlpha(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitCommandBlendColorAlpha(this);
        }
    }
}