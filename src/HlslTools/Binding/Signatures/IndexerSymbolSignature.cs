using System;
using HlslTools.Symbols;

namespace HlslTools.Binding.Signatures
{
    internal sealed class IndexerSymbolSignature : Signature
    {
        public IndexerSymbolSignature(IndexerSymbol symbol)
        {
            Symbol = symbol;
        }

        public override TypeSymbol ReturnType => Symbol.ValueType;

        public override TypeSymbol GetParameterType(int index)
        {
            return IntrinsicTypes.Uint; // TODO: Is this always true?
        }

        public override int ParameterCount { get; } = 1;
        public override bool HasVariadicParameter { get; } = false;

        public IndexerSymbol Symbol { get; }

        public override string ToString()
        {
            return Symbol.ToString();
        }
    }
}