namespace HlslTools.Syntax
{
    public sealed class SamplerStateInitializerSyntax : InitializerSyntax
    {
        public readonly SyntaxToken EqualsToken;
        public readonly SyntaxToken SamplerStateKeyword;
        public readonly StateInitializerSyntax StateInitializer;

        public SamplerStateInitializerSyntax(SyntaxToken equalsToken, SyntaxToken samplerStateKeyword, StateInitializerSyntax stateInitializer)
            : base(SyntaxKind.EqualsValueClause)
        {
            RegisterChildNode(out EqualsToken, equalsToken);
            RegisterChildNode(out SamplerStateKeyword, samplerStateKeyword);
            RegisterChildNode(out StateInitializer, stateInitializer);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitSamplerStateInitializer(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitSamplerStateInitializer(this);
        }
    }
}