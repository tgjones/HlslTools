using ShaderTools.CodeAnalysis.Hlsl.Symbols;

namespace ShaderTools.CodeAnalysis.Hlsl.Binding.BoundNodes
{
    internal sealed class BoundUnknownType : BoundType
    {
        public BoundUnknownType()
            : base(BoundNodeKind.UnknownType, TypeFacts.Unknown)
        {
        }
    }
}