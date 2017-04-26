namespace ShaderTools.CodeAnalysis.ShaderLab.Syntax
{
    public sealed class CommandSetTextureCombineLerpValueSyntax : BaseCommandSetTextureCombineValueSyntax
    {
        public readonly CommandSetTextureCombineSourceSyntax Source1;
        public readonly SyntaxToken LerpKeyword;
        public readonly SyntaxToken OpenParenToken;
        public readonly CommandSetTextureCombineSourceSyntax Source2;
        public readonly SyntaxToken CloseParenToken;
        public readonly CommandSetTextureCombineSourceSyntax Source3;

        public CommandSetTextureCombineLerpValueSyntax(CommandSetTextureCombineSourceSyntax source1, SyntaxToken lerpKeyword, SyntaxToken openParenToken, CommandSetTextureCombineSourceSyntax source2, SyntaxToken closeParenToken, CommandSetTextureCombineSourceSyntax source3)
            : base(SyntaxKind.CommandSetTextureCombineLerpValue)
        {
            RegisterChildNode(out Source1, source1);
            RegisterChildNode(out LerpKeyword, lerpKeyword);
            RegisterChildNode(out OpenParenToken, openParenToken);
            RegisterChildNode(out Source2, source2);
            RegisterChildNode(out CloseParenToken, closeParenToken);
            RegisterChildNode(out Source3, source3);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitCommandSetTextureCombineLerpValue(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitCommandSetTextureCombineLerpValue(this);
        }
    }
}