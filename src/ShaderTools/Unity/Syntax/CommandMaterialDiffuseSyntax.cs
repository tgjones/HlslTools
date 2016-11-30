namespace ShaderTools.Unity.Syntax
{
    public sealed class CommandMaterialDiffuseSyntax : CommandSyntax
    {
        public readonly SyntaxToken DiffuseKeyword;
        public readonly CommandValueSyntax Value;

        public CommandMaterialDiffuseSyntax(SyntaxToken diffuseKeyword, CommandValueSyntax value)
            : base(SyntaxKind.CommandMaterialDiffuse)
        {
            RegisterChildNode(out DiffuseKeyword, diffuseKeyword);
            RegisterChildNode(out Value, value);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitCommandMaterialDiffuse(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitCommandMaterialDiffuse(this);
        }
    }
}