namespace ShaderTools.CodeAnalysis.ShaderLab.Syntax
{
    public sealed class CommandAlphaToMaskSyntax : CommandSyntax
    {
        public readonly SyntaxToken AlphaToMaskKeyword;
        public readonly CommandValueSyntax Value;

        public CommandAlphaToMaskSyntax(SyntaxToken alphaToMaskKeyword, CommandValueSyntax value)
            : base(SyntaxKind.CommandAlphaToMask)
        {
            RegisterChildNode(out AlphaToMaskKeyword, alphaToMaskKeyword);
            RegisterChildNode(out Value, value);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitCommandAlphaToMask(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitCommandAlphaToMask(this);
        }
    }
}