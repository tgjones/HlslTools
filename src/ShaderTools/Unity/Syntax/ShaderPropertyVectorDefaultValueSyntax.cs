namespace ShaderTools.Unity.Syntax
{
    public sealed class ShaderPropertyVectorDefaultValueSyntax : ShaderPropertyDefaultValueSyntax
    {
        public readonly BaseVectorSyntax Vector;

        public ShaderPropertyVectorDefaultValueSyntax(BaseVectorSyntax vector)
            : base (SyntaxKind.ShaderPropertyVectorDefaultValue)
        {
            RegisterChildNode(out Vector, vector);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitShaderPropertyVectorDefaultValue(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitShaderPropertyVectorDefaultValue(this);
        }
    }
}