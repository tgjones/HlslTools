namespace ShaderTools.Hlsl.Binding.BoundNodes
{
    internal sealed class BoundEqualsValue : BoundInitializer
    {
        public BoundExpression Value { get; }

        public BoundEqualsValue(BoundExpression value)
            : base(BoundNodeKind.EqualsValue)
        {
            Value = value;
        }
    }
}