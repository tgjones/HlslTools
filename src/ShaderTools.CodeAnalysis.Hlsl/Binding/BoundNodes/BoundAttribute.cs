using ShaderTools.CodeAnalysis.Hlsl.Symbols;

namespace ShaderTools.CodeAnalysis.Hlsl.Binding.BoundNodes
{
    internal sealed class BoundAttribute : BoundNode
    {
        public AttributeSymbol AttributeSymbol { get; }

        public BoundAttribute(AttributeSymbol attributeSymbol)
            : base(BoundNodeKind.Attribute)
        {
            AttributeSymbol = attributeSymbol;
        }
    }
}