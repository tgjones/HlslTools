namespace ShaderTools.Unity.Syntax
{
    public sealed class CommandColorMaterialSyntax : CommandSyntax
    {
        public readonly SyntaxToken ColorMaterialKeyword;
        public readonly SyntaxToken Value;

        public CommandColorMaterialSyntax(SyntaxToken colorMaterialKeyword, SyntaxToken value)
            : base(SyntaxKind.CommandColorMaterial)
        {
            RegisterChildNode(out ColorMaterialKeyword, colorMaterialKeyword);
            RegisterChildNode(out Value, value);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitCommandColorMaterial(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitCommandColorMaterial(this);
        }
    }
}