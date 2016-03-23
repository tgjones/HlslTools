using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace HlslTools.Symbols
{
    public abstract class InvocableSymbol : Symbol
    {
        private readonly List<ParameterSymbol> _parameters;
        private ImmutableArray<ParameterSymbol> _parametersArray = ImmutableArray<ParameterSymbol>.Empty;

        public ImmutableArray<ParameterSymbol> Parameters
        {
            get
            {
                if (_parametersArray == ImmutableArray<ParameterSymbol>.Empty)
                    _parametersArray = _parameters.ToImmutableArray();
                return _parametersArray;
            }
        }

        public TypeSymbol ReturnType { get; }

        protected InvocableSymbol(SymbolKind kind, string name, string documentation, Symbol parent, TypeSymbol returnType, Func<InvocableSymbol, IEnumerable<ParameterSymbol>> lazyParameters = null)
            : base(kind, name, documentation, parent)
        {
            _parameters = new List<ParameterSymbol>();

            if (lazyParameters != null)
                foreach (var parameter in lazyParameters(this))
                    AddParameter(parameter);

            ReturnType = returnType;
        }

        internal void AddParameter(ParameterSymbol parameter)
        {
            _parameters.Add(parameter);
            _parametersArray = ImmutableArray<ParameterSymbol>.Empty;
        }
    }
}