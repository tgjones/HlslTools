namespace ShaderTools.CodeAnalysis.ShaderLab.Syntax
{
    public sealed class CommandSeparateSpecularSyntax : CommandSyntax
    {
        public readonly SyntaxToken SeparateSpecularKeyword;
        public readonly CommandValueSyntax Value;

        public CommandSeparateSpecularSyntax(SyntaxToken separateSpecularKeyword, CommandValueSyntax value)
            : base(SyntaxKind.CommandSeparateSpecular)
        {
            RegisterChildNode(out SeparateSpecularKeyword, separateSpecularKeyword);
            RegisterChildNode(out Value, value);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitCommandSeparateSpecular(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitCommandSeparateSpecular(this);
        }
    }
}