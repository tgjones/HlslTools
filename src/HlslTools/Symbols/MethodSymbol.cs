using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace HlslTools.Symbols
{
    public abstract class MethodSymbol : MemberSymbol, IInvocableSymbol
    {
        private readonly Func<MethodSymbol, IEnumerable<ParameterSymbol>> _lazyParameters;
        private ImmutableArray<ParameterSymbol> _parameters;

        protected MethodSymbol(SymbolKind kind, string name, string documentation, TypeSymbol parent, TypeSymbol returnType, Func<MethodSymbol, IEnumerable<ParameterSymbol>> lazyParameters)
            : base(kind, name, documentation, parent, returnType)
        {
            _lazyParameters = lazyParameters;
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
    }
}