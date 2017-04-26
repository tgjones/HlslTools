using ShaderTools.CodeAnalysis.Hlsl.Symbols;

namespace ShaderTools.CodeAnalysis.Hlsl.Binding.BoundNodes
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