namespace ShaderTools.CodeAnalysis.ShaderLab.Syntax
{
    public sealed class CommandFogRangeSyntax : CommandSyntax
    {
        public readonly SyntaxToken RangeKeyword;
        public readonly CommandValueSyntax NearValue;
        public readonly SyntaxToken CommaToken;
        public readonly CommandValueSyntax FarValue;

        public CommandFogRangeSyntax(SyntaxToken rangeKeyword, CommandValueSyntax nearValue, SyntaxToken commaToken, CommandValueSyntax farValue)
            : base(SyntaxKind.CommandFogRange)
        {
            RegisterChildNode(out RangeKeyword, rangeKeyword);
            RegisterChildNode(out NearValue, nearValue);
            RegisterChildNode(out CommaToken, commaToken);
            RegisterChildNode(out FarValue, farValue);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitCommandFogRange(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitCommandFogRange(this);
        }
    }
}