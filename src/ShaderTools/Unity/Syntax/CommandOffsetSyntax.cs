namespace ShaderTools.Unity.Syntax
{
    public sealed class CommandOffsetSyntax : CommandSyntax
    {
        public readonly SyntaxToken OffsetKeyword;
        public readonly CommandValueSyntax Factor;
        public readonly SyntaxToken CommaToken;
        public readonly CommandValueSyntax Units;

        public CommandOffsetSyntax(SyntaxToken offsetKeyword, CommandValueSyntax factor, SyntaxToken commaToken, CommandValueSyntax units)
            : base(SyntaxKind.CommandOffset)
        {
            RegisterChildNode(out OffsetKeyword, offsetKeyword);
            RegisterChildNode(out Factor, factor);
            RegisterChildNode(out CommaToken, commaToken);
            RegisterChildNode(out Units, units);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitCommandOffset(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitCommandOffset(this);
        }
    }
}