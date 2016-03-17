using System;
using System.Linq;
using HlslTools.Symbols;

namespace HlslTools.Binding.Signatures
{
    internal sealed class FunctionSymbolSignature : Signature
    {
        public FunctionSymbolSignature(FunctionSymbol symbol)
        {
            Symbol = symbol;
            ParameterCount = Symbol.Parameters.Count(x => x.ValueType != TypeFacts.Variadic);
            HasVariadicParameter = Symbol.Parameters.Any(x => x.ValueType == TypeFacts.Variadic);
        }

        public override TypeSymbol ReturnType => Symbol.ReturnType;

        public override TypeSymbol GetParameterType(int index)
        {
            return Symbol.Parameters[index].ValueType;
        }

        public override int ParameterCount { get; }

        public override bool HasVariadicParameter { get; }

        public FunctionSymbol Symbol { get; }

        public override string ToString()
        {
            return Symbol.ToString();
        }
    }
}