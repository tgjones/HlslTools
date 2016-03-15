using HlslTools.Symbols;

namespace HlslTools.Binding.Signatures
{
    internal sealed class MethodSymbolSignature : Signature
    {
        public MethodSymbolSignature(MethodSymbol symbol)
        {
            Symbol = symbol;
        }

        public override TypeSymbol ReturnType => Symbol.ReturnType;

        public override TypeSymbol GetParameterType(int index)
        {
            return Symbol.Parameters[index].ValueType;
        }

        public override int ParameterCount => Symbol.Parameters.Length;

        public MethodSymbol Symbol { get; }

        public override string ToString()
        {
            return Symbol.ToString();
        }
    }
}