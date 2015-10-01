using System.Collections.Immutable;
using System.Linq;
using HlslTools.Binding.BoundNodes;

namespace HlslTools.Symbols
{
    internal sealed class SymbolImplementation
    {
        public SymbolImplementation(ImmutableArray<BoundStatement> statements, SymbolScope scope)
        {
            Scope = scope;
            Statements = statements;

            DeclaresVariables = statements.Any(x => x is BoundVariableDeclaration);
        }

        public bool DeclaresVariables { get; }
        public SymbolScope Scope { get; }
        public ImmutableArray<BoundStatement> Statements { get; }
    }
}