namespace ShaderTools.Unity.Syntax
{
    public sealed class CommandDependencySyntax : CommandSyntax
    {
        public readonly SyntaxToken DependencyKeyword;
        public readonly SyntaxToken NameToken;
        public readonly SyntaxToken EqualsToken;
        public readonly SyntaxToken DependentShaderToken;

        public CommandDependencySyntax(SyntaxToken dependencyKeyword, SyntaxToken nameToken, SyntaxToken equalsToken, SyntaxToken dependentShaderToken)
            : base(SyntaxKind.CommandDependency)
        {
            RegisterChildNode(out DependencyKeyword, dependencyKeyword);
            RegisterChildNode(out NameToken, nameToken);
            RegisterChildNode(out EqualsToken, equalsToken);
            RegisterChildNode(out DependentShaderToken, dependentShaderToken);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitCommandDependency(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitCommandDependency(this);
        }
    }
}