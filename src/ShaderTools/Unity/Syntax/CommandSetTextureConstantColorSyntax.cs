namespace ShaderTools.Unity.Syntax
{
    public sealed class CommandSetTextureConstantColorSyntax : CommandSyntax
    {
        public readonly SyntaxToken ConstantColorKeyword;
        public readonly CommandValueSyntax Value;

        public CommandSetTextureConstantColorSyntax(SyntaxToken constantColorKeyword, CommandValueSyntax value)
            : base(SyntaxKind.CommandSetTextureConstantColor)
        {
            RegisterChildNode(out ConstantColorKeyword, constantColorKeyword);
            RegisterChildNode(out Value, value);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitCommandSetTextureConstantColor(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitCommandSetTextureConstantColor(this);
        }
    }
}