namespace ShaderTools.CodeAnalysis.ShaderLab.Syntax
{
    public sealed class CommandAlphaTestOffSyntax : CommandSyntax
    {
        public readonly SyntaxToken AlphaTestKeyword;
        public readonly SyntaxToken OffToken;

        public CommandAlphaTestOffSyntax(SyntaxToken alphaTestKeyword, SyntaxToken offToken)
            : base(SyntaxKind.CommandAlphaTestOff)
        {
            RegisterChildNode(out AlphaTestKeyword, alphaTestKeyword);
            RegisterChildNode(out OffToken, offToken);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitCommandAlphaTestOff(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitCommandAlphaTestOff(this);
        }
    }
}