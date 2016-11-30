namespace ShaderTools.Unity.Syntax
{
    public sealed class CommandMaterialSpecularSyntax : CommandSyntax
    {
        public readonly SyntaxToken SpecularKeyword;
        public readonly CommandValueSyntax Value;

        public CommandMaterialSpecularSyntax(SyntaxToken specularKeyword, CommandValueSyntax value)
            : base(SyntaxKind.CommandMaterialSpecular)
        {
            RegisterChildNode(out SpecularKeyword, specularKeyword);
            RegisterChildNode(out Value, value);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitCommandMaterialSpecular(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitCommandMaterialSpecular(this);
        }
    }
}