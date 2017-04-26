using ShaderTools.CodeAnalysis.Hlsl.Symbols;

namespace ShaderTools.CodeAnalysis.Hlsl.Binding.BoundNodes
{
    internal abstract class BoundType : BoundExpression
    {
        public TypeSymbol TypeSymbol { get; }

        public override TypeSymbol Type { get; }

        protected BoundType(BoundNodeKind kind, TypeSymbol typeSymbol)
            : base(kind)
        {
            TypeSymbol = typeSymbol;
            Type = typeSymbol;
        }
    }
}