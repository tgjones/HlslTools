using System.Collections.Immutable;

namespace ShaderTools.CodeAnalysis.Hlsl.Binding.BoundNodes
{
    internal sealed class BoundTypedefStatement : BoundStatement
    {
        public BoundTypedefStatement(ImmutableArray<BoundTypeAlias> declarations)
            : base(BoundNodeKind.Typedef)
        {
            Declarations = declarations;
        }

        public ImmutableArray<BoundTypeAlias> Declarations { get; }
    }
}