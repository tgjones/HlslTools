using HlslTools.Symbols;

namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundSemantic : BoundVariableQualifier
    {
        public SemanticSymbol SemanticSymbol { get; }

        public BoundSemantic(SemanticSymbol semanticSymbol)
            : base(BoundNodeKind.Semantic)
        {
            SemanticSymbol = semanticSymbol;
        }
    }
}