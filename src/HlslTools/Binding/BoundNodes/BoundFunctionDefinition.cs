using HlslTools.Symbols;

namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundFunctionDefinition : BoundNode
    {
        public FunctionDefinitionSymbol FunctionSymbol { get; }
        public BoundBlock BoundBody { get; }

        public BoundFunctionDefinition(FunctionDefinitionSymbol functionSymbol, BoundBlock boundBody)
            : base(BoundNodeKind.FunctionDefinition)
        {
            FunctionSymbol = functionSymbol;
            BoundBody = boundBody;
        }
    }
}