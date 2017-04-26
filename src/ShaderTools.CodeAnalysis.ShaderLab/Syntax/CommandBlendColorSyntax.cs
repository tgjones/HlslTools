namespace ShaderTools.CodeAnalysis.ShaderLab.Syntax
{
    public sealed class CommandBlendColorSyntax : CommandSyntax
    {
        public readonly SyntaxToken BlendKeyword;
        public readonly CommandValueSyntax SrcFactor;
        public readonly CommandValueSyntax DstFactor;

        public CommandBlendColorSyntax(SyntaxToken blendKeyword, CommandValueSyntax srcFactor, CommandValueSyntax dstFactor)
            : base(SyntaxKind.CommandBlendColor)
        {
            RegisterChildNode(out BlendKeyword, blendKeyword);
            RegisterChildNode(out SrcFactor, srcFactor);
            RegisterChildNode(out DstFactor, dstFactor);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitCommandBlendColor(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitCommandBlendColor(this);
        }
    }
}