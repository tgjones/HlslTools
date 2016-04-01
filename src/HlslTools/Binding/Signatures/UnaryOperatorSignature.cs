using HlslTools.Binding.BoundNodes;
using HlslTools.Symbols;

namespace HlslTools.Binding.Signatures
{
    internal sealed class UnaryOperatorSignature : Signature
    {
        private readonly TypeSymbol _parameterType;

        public UnaryOperatorSignature(UnaryOperatorKind kind, TypeSymbol returnType, TypeSymbol parameterType)
        {
            Kind = kind;
            ReturnType = returnType;
            _parameterType = parameterType;
        }

        public UnaryOperatorSignature(UnaryOperatorKind kind, TypeSymbol type)
            : this(kind, type, type)
        {
        }

        public override TypeSymbol ReturnType { get; }

        public override ParameterDirection GetParameterDirection(int index) => ParameterDirection.In;
        public override bool ParameterHasDefaultValue(int index) => false;

        public override TypeSymbol GetParameterType(int index) => _parameterType;

        public override int ParameterCount => 1;
        public override bool HasVariadicParameter { get; } = false;

        public UnaryOperatorKind Kind { get; }

        public override string ToString()
        {
            return $"{Kind}({_parameterType.ToDisplayName()}) AS {ReturnType.ToDisplayName()}";
        }
    }
}