namespace ShaderTools.CodeAnalysis.ShaderLab.Syntax
{
    public sealed class CommandZTestSyntax : CommandSyntax
    {
        public readonly SyntaxToken ZTestKeyword;
        public readonly CommandValueSyntax Value;

        public CommandZTestSyntax(SyntaxToken zTestKeyword, CommandValueSyntax value)
            : base(SyntaxKind.CommandZTest)
        {
            RegisterChildNode(out ZTestKeyword, zTestKeyword);
            RegisterChildNode(out Value, value);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitCommandZTest(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitCommandZTest(this);
        }
    }
}