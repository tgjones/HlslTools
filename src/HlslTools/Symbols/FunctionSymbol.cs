using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace HlslTools.Symbols
{
    public class FunctionSymbol : Symbol, IInvocableSymbol
    {
        private readonly Func<FunctionSymbol, IEnumerable<ParameterSymbol>> _lazyParameters;
        private ImmutableArray<ParameterSymbol> _parameters;

        public FunctionSymbol(string name, string documentation, TypeSymbol returnType, Func<FunctionSymbol, IEnumerable<ParameterSymbol>> lazyParameters)
            : base(SymbolKind.Function, name, documentation, returnType)
        {
            _lazyParameters = lazyParameters;
            ReturnType = returnType;
        }

        public ImmutableArray<ParameterSymbol> Parameters
        {
            get
            {
                if (_parameters.IsDefault)
                    _parameters = _lazyParameters(this).ToImmutableArray();
                return _parameters;
            }
        }

        public TypeSymbol ReturnType { get; }
    }
}