namespace ShaderTools.Unity.Syntax
{
    public sealed class CommandZWriteSyntax : CommandSyntax
    {
        public readonly SyntaxToken ZWriteKeyword;
        public readonly CommandValueSyntax Value;

        public CommandZWriteSyntax(SyntaxToken zWriteKeyword, CommandValueSyntax value)
            : base(SyntaxKind.CommandZWrite)
        {
            RegisterChildNode(out ZWriteKeyword, zWriteKeyword);
            RegisterChildNode(out Value, value);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitCommandZWrite(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitCommandZWrite(this);
        }
    }
}