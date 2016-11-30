namespace ShaderTools.Unity.Syntax
{
    public sealed class CommandSetTextureMatrixSyntax : CommandSyntax
    {
        public readonly SyntaxToken MatrixKeyword;
        public readonly CommandVariableValueSyntax Value;

        public CommandSetTextureMatrixSyntax(SyntaxToken matrixKeyword, CommandVariableValueSyntax value)
            : base(SyntaxKind.CommandSetTextureMatrix)
        {
            RegisterChildNode(out MatrixKeyword, matrixKeyword);
            RegisterChildNode(out Value, value);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitCommandSetTextureMatrix(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitCommandSetTextureMatrix(this);
        }
    }
}