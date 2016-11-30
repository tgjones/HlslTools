namespace ShaderTools.Unity.Syntax
{
    public sealed class CommandMaterialEmissionSyntax : CommandSyntax
    {
        public readonly SyntaxToken EmissionKeyword;
        public readonly CommandValueSyntax Value;

        public CommandMaterialEmissionSyntax(SyntaxToken emissionKeyword, CommandValueSyntax value)
            : base(SyntaxKind.CommandMaterialEmission)
        {
            RegisterChildNode(out EmissionKeyword, emissionKeyword);
            RegisterChildNode(out Value, value);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitCommandMaterialEmission(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitCommandMaterialEmission(this);
        }
    }
}