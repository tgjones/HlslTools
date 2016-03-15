using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace HlslTools.Symbols
{
    public class MethodSymbol : Symbol, IInvocableSymbol, IMemberSymbol
    {
        private readonly Func<MethodSymbol, IEnumerable<ParameterSymbol>> _lazyParameters;
        private ImmutableArray<ParameterSymbol> _parameters;

        public MethodSymbol(string name, string documentation, TypeSymbol parent, TypeSymbol returnType, Func<MethodSymbol, IEnumerable<ParameterSymbol>> lazyParameters)
            : base(SymbolKind.Method, name, documentation, parent)
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

        TypeSymbol IMemberSymbol.AssociatedType => ReturnType;
    }
}