using System.Collections.Immutable;
using ShaderTools.Hlsl.Symbols;

namespace ShaderTools.Hlsl.Binding.BoundNodes
{
    internal sealed class BoundFunctionDefinition : BoundFunction
    {
        public BoundType ReturnType { get; }
        public BoundBlock Body { get; }

        public BoundFunctionDefinition(FunctionSymbol functionSymbol, BoundType returnType, ImmutableArray<BoundVariableDeclaration> parameters, BoundBlock body)
            : base(BoundNodeKind.FunctionDefinition, functionSymbol, parameters)
        {
            ReturnType = returnType;
            Body = body;
        }
    }
}