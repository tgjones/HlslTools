namespace ShaderTools.CodeAnalysis.ShaderLab.Syntax
{

    public sealed class CommandSetTextureCombineMultiplyAlphaValueSyntax : BaseCommandSetTextureCombineValueSyntax
    {
        public readonly CommandSetTextureCombineSourceSyntax Source1;
        public readonly SyntaxToken AsteriskToken;
        public readonly CommandSetTextureCombineSourceSyntax Source2;
        public readonly SyntaxToken PlusToken;
        public readonly CommandSetTextureCombineSourceSyntax Source3;

        public CommandSetTextureCombineMultiplyAlphaValueSyntax(CommandSetTextureCombineSourceSyntax source1, SyntaxToken asteriskToken, CommandSetTextureCombineSourceSyntax source2, SyntaxToken plusToken, CommandSetTextureCombineSourceSyntax source3)
            : base(SyntaxKind.CommandSetTextureCombineMultiplyAlphaValue)
        {
            RegisterChildNode(out Source1, source1);
            RegisterChildNode(out AsteriskToken, asteriskToken);
            RegisterChildNode(out Source2, source2);
            RegisterChildNode(out PlusToken, plusToken);
            RegisterChildNode(out Source3, source3);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitCommandSetTextureCombineMultiplyAlphaValue(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitCommandSetTextureCombineMultiplyAlphaValue(this);
        }
    }
}