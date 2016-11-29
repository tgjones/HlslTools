using ShaderTools.Hlsl.Symbols;

namespace ShaderTools.Hlsl.Binding.BoundNodes
{
    internal sealed class BoundCompileExpression : BoundExpression
    {
        public BoundCompileExpression()
            : base(BoundNodeKind.CompileExpression)
        {
        }

        public override TypeSymbol Type { get; } = TypeFacts.Unknown;
    }
}