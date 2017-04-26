namespace ShaderTools.CodeAnalysis.ShaderLab.Syntax
{
    public sealed class CommandMaterialShininessSyntax : CommandSyntax
    {
        public readonly SyntaxToken ShininessKeyword;
        public readonly CommandValueSyntax Value;

        public CommandMaterialShininessSyntax(SyntaxToken shininessKeyword, CommandValueSyntax value)
            : base(SyntaxKind.CommandMaterialShininess)
        {
            RegisterChildNode(out ShininessKeyword, shininessKeyword);
            RegisterChildNode(out Value, value);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitCommandMaterialShininess(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitCommandMaterialShininess(this);
        }
    }
}