using ShaderTools.CodeAnalysis.Hlsl.Symbols;

namespace ShaderTools.CodeAnalysis.Hlsl.Binding.BoundNodes
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