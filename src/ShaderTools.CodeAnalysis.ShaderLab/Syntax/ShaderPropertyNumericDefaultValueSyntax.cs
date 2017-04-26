namespace ShaderTools.CodeAnalysis.ShaderLab.Syntax
{
    public sealed class ShaderPropertyNumericDefaultValueSyntax : ShaderPropertyDefaultValueSyntax
    {
        public readonly ExpressionSyntax Number;

        public ShaderPropertyNumericDefaultValueSyntax(ExpressionSyntax number)
            : base(SyntaxKind.ShaderPropertyNumericDefaultValue)
        {
            RegisterChildNode(out Number, number);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitShaderPropertyNumericDefaultValue(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitShaderPropertyNumericDefaultValue(this);
        }
    }
}