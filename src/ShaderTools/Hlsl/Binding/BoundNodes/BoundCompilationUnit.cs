using System.Collections.Immutable;

namespace ShaderTools.Hlsl.Binding.BoundNodes
{
    internal sealed class BoundCompilationUnit : BoundNode
    {
        public ImmutableArray<BoundNode> Declarations { get; }

        public BoundCompilationUnit(ImmutableArray<BoundNode> declarations)
            : base(BoundNodeKind.CompilationUnit)
        {
            Declarations = declarations;
        }
    }
}