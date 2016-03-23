using System.Collections.Immutable;
using HlslTools.Symbols;

namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundFunctionDeclaration : BoundFunction
    {
        public BoundFunctionDeclaration(FunctionSymbol functionSymbol, ImmutableArray<BoundVariableDeclaration> parameters)
            : base(BoundNodeKind.FunctionDeclaration, functionSymbol, parameters)
        {
        }
    }
}