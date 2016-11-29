using ShaderTools.Hlsl.Symbols;

namespace ShaderTools.Hlsl.Binding.BoundNodes
{
    internal sealed class BoundUnknownType : BoundType
    {
        public BoundUnknownType()
            : base(BoundNodeKind.UnknownType, TypeFacts.Unknown)
        {
        }
    }
}