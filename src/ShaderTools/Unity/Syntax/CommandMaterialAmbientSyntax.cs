namespace ShaderTools.Unity.Syntax
{
    public sealed class CommandMaterialAmbientSyntax : CommandSyntax
    {
        public readonly SyntaxToken AmbientKeyword;
        public readonly CommandValueSyntax Value;

        public CommandMaterialAmbientSyntax(SyntaxToken ambientKeyword, CommandValueSyntax value)
            : base(SyntaxKind.CommandMaterialAmbient)
        {
            RegisterChildNode(out AmbientKeyword, ambientKeyword);
            RegisterChildNode(out Value, value);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitCommandMaterialAmbient(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitCommandMaterialAmbient(this);
        }
    }
}