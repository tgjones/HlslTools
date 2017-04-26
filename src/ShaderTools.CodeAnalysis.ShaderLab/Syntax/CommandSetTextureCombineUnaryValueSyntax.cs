namespace ShaderTools.CodeAnalysis.ShaderLab.Syntax
{
    public sealed class CommandSetTextureCombineUnaryValueSyntax : BaseCommandSetTextureCombineValueSyntax
    {
        public readonly CommandSetTextureCombineSourceSyntax Source;

        public CommandSetTextureCombineUnaryValueSyntax(CommandSetTextureCombineSourceSyntax source)
            : base(SyntaxKind.CommandSetTextureCombineUnaryValue)
        {
            RegisterChildNode(out Source, source);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitCommandSetTextureCombineUnaryValue(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitCommandSetTextureCombineUnaryValue(this);
        }
    }
}