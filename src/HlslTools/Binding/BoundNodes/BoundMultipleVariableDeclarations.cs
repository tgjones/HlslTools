using System.Collections.Immutable;
using HlslTools.Syntax;

namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundMultipleVariableDeclarations : BoundStatement
    {
        public BoundMultipleVariableDeclarations(VariableDeclarationSyntax syntax, ImmutableArray<BoundVariableDeclaration> variableDeclarations)
            : base(BoundNodeKind.MultipleVariableDeclarations, syntax)
        {
            VariableDeclarations = variableDeclarations;
        }

        public ImmutableArray<BoundVariableDeclaration> VariableDeclarations { get; }
    }
}