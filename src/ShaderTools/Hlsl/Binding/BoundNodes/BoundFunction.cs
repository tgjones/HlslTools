using System.Collections.Immutable;
using ShaderTools.Hlsl.Symbols;

namespace ShaderTools.Hlsl.Binding.BoundNodes
{
    internal abstract class BoundFunction : BoundNode
    {
        public FunctionSymbol FunctionSymbol { get; }
        public ImmutableArray<BoundVariableDeclaration> Parameters { get; }

        protected BoundFunction(BoundNodeKind kind, FunctionSymbol functionSymbol, ImmutableArray<BoundVariableDeclaration> parameters)
            : base(kind)
        {
            FunctionSymbol = functionSymbol;
            Parameters = parameters;
        }
    }
}