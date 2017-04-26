namespace ShaderTools.CodeAnalysis.ShaderLab.Syntax
{
    public sealed class CommandColorMaskSyntax : CommandSyntax
    {
        public readonly SyntaxToken ColorMaskKeyword;
        public readonly CommandValueSyntax Mask;

        public CommandColorMaskSyntax(SyntaxToken colorMaskKeyword, CommandValueSyntax mask)
            : base(SyntaxKind.CommandColorMask)
        {
            RegisterChildNode(out ColorMaskKeyword, colorMaskKeyword);
            RegisterChildNode(out Mask, mask);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitCommandColorMask(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitCommandColorMask(this);
        }
    }
}