using ShaderTools.CodeAnalysis.Hlsl.Symbols;

namespace ShaderTools.CodeAnalysis.Hlsl.Binding.BoundNodes
{
    internal sealed class BoundObjectType : BoundType
    {
        public IntrinsicObjectTypeSymbol ObjectSymbol { get; }

        public BoundObjectType(IntrinsicObjectTypeSymbol objectSymbol)
            : base(BoundNodeKind.IntrinsicObjectType, objectSymbol)
        {
            ObjectSymbol = objectSymbol;
        }
    }
}