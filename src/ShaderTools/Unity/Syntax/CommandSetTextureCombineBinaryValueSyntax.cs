namespace ShaderTools.Unity.Syntax
{
    public sealed class CommandSetTextureCombineBinaryValueSyntax : BaseCommandSetTextureCombineValueSyntax
    {
        public readonly CommandSetTextureCombineSourceSyntax Source1;
        public readonly SyntaxToken OperatorToken;
        public readonly CommandSetTextureCombineSourceSyntax Source2;

        public CommandSetTextureCombineBinaryValueSyntax(CommandSetTextureCombineSourceSyntax source1, SyntaxToken operatorToken, CommandSetTextureCombineSourceSyntax source2)
            : base(SyntaxKind.CommandSetTextureCombineBinaryValue)
        {
            RegisterChildNode(out Source1, source1);
            RegisterChildNode(out OperatorToken, operatorToken);
            RegisterChildNode(out Source2, source2);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitCommandSetTextureCombineBinaryValue(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitCommandSetTextureCombineBinaryValue(this);
        }
    }
}