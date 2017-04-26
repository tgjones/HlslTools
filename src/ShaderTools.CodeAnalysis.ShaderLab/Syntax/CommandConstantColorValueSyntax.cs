namespace ShaderTools.CodeAnalysis.ShaderLab.Syntax
{
    public sealed class CommandConstantColorValueSyntax : CommandValueSyntax
    {
        public readonly BaseVectorSyntax Vector;

        public CommandConstantColorValueSyntax(BaseVectorSyntax vector)
            : base(SyntaxKind.CommandConstantColorValue)
        {
            RegisterChildNode(out Vector, vector);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitCommandConstantColorValue(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitCommandConstantColorValue(this);
        }
    }
}