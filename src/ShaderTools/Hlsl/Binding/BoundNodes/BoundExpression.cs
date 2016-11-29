using ShaderTools.Hlsl.Symbols;

namespace ShaderTools.Hlsl.Binding.BoundNodes
{
    internal abstract class BoundExpression : BoundNode
    {
        public abstract TypeSymbol Type { get; }

        protected BoundExpression(BoundNodeKind kind)
            : base(kind)
        {
        }
    }
}