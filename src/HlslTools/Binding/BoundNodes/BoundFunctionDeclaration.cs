using HlslTools.Symbols;

namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundFunctionDeclaration : BoundNode
    {
        public FunctionDeclarationSymbol FunctionSymbol { get; }

        public BoundFunctionDeclaration(FunctionDeclarationSymbol functionSymbol)
            : base(BoundNodeKind.FunctionDeclaration)
        {
            FunctionSymbol = functionSymbol;
        }
    }
}