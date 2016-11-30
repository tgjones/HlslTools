namespace ShaderTools.Unity.Syntax
{
    public sealed class UsePassSyntax : BasePassSyntax
    {
        public readonly SyntaxToken UsePassKeyword;
        public readonly SyntaxToken PassName;

        public UsePassSyntax(SyntaxToken usePassKeyword, SyntaxToken passName)
            : base(SyntaxKind.UsePass)
        {
            RegisterChildNode(out UsePassKeyword, usePassKeyword);
            RegisterChildNode(out PassName, passName);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitUsePass(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitUsePass(this);
        }
    }
}