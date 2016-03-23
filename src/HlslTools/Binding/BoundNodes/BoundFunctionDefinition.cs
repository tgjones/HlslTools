using System.Collections.Immutable;
using HlslTools.Symbols;

namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundFunctionDefinition : BoundFunction
    {
        public BoundBlock Body { get; }

        public BoundFunctionDefinition(FunctionSymbol functionSymbol, ImmutableArray<BoundVariableDeclaration> parameters, BoundBlock body)
            : base(BoundNodeKind.FunctionDefinition, functionSymbol, parameters)
        {
            Body = body;
        }
    }
}