using HlslTools.Symbols;

namespace HlslTools.Binding.Signatures
{
    internal sealed class FunctionSymbolSignature : Signature
    {
        public FunctionSymbolSignature(FunctionSymbol symbol)
        {
            Symbol = symbol;
        }

        public override TypeSymbol ReturnType => Symbol.ReturnType;

        public override TypeSymbol GetParameterType(int index)
        {
            return Symbol.Parameters[index].ValueType;
        }

        public override int ParameterCount => Symbol.Parameters.Length;

        public FunctionSymbol Symbol { get; }

        public override string ToString()
        {
            return Symbol.ToString();
        }
    }
}