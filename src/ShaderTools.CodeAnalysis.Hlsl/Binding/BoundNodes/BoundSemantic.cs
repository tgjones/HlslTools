using ShaderTools.CodeAnalysis.Hlsl.Symbols;

namespace ShaderTools.CodeAnalysis.Hlsl.Binding.BoundNodes
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