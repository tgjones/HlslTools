using System.Collections.Immutable;

namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundMultipleVariableDeclarations : BoundStatement
    {
        public BoundMultipleVariableDeclarations(ImmutableArray<BoundVariableDeclaration> variableDeclarations)
            : base(BoundNodeKind.MultipleVariableDeclarations)
        {
            VariableDeclarations = variableDeclarations;
        }

        public ImmutableArray<BoundVariableDeclaration> VariableDeclarations { get; }
    }
}