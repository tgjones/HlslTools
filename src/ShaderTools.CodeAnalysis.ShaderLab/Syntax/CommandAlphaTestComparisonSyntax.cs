namespace ShaderTools.CodeAnalysis.ShaderLab.Syntax
{
    public sealed class CommandAlphaTestComparisonSyntax : CommandSyntax
    {
        public readonly SyntaxToken AlphaTestKeyword;
        public readonly SyntaxToken Comparison;
        public readonly CommandValueSyntax AlphaValue;

        public CommandAlphaTestComparisonSyntax(SyntaxToken alphaTestKeyword, SyntaxToken comparison, CommandValueSyntax alphaValue)
            : base(SyntaxKind.CommandAlphaTestComparison)
        {
            RegisterChildNode(out AlphaTestKeyword, alphaTestKeyword);
            RegisterChildNode(out Comparison, comparison);
            RegisterChildNode(out AlphaValue, alphaValue);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitCommandAlphaTestComparison(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitCommandAlphaTestComparison(this);
        }
    }

    
}