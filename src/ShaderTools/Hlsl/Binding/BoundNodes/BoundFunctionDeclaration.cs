using System.Collections.Immutable;
using ShaderTools.Hlsl.Symbols;

namespace ShaderTools.Hlsl.Binding.BoundNodes
{
    internal sealed class BoundFunctionDeclaration : BoundFunction
    {
        public BoundType ReturnType { get; }

        public BoundFunctionDeclaration(FunctionSymbol functionSymbol, BoundType returnType, ImmutableArray<BoundVariableDeclaration> parameters)
            : base(BoundNodeKind.FunctionDeclaration, functionSymbol, parameters)
        {
            ReturnType = returnType;
        }
    }
}