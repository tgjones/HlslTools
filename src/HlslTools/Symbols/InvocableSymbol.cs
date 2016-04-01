using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

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

        internal InvocableSymbol(SymbolKind kind, string name, string documentation, Symbol parent, TypeSymbol returnType, Func<InvocableSymbol, IEnumerable<ParameterSymbol>> lazyParameters = null)
            : base(kind, name, documentation, parent)
        {
            if (returnType == null)
                throw new ArgumentNullException(nameof(returnType));

            _parameters = new List<ParameterSymbol>();

            if (lazyParameters != null)
                foreach (var parameter in lazyParameters(this))
                    AddParameter(parameter);

            ReturnType = returnType;
        }

        internal void ClearParameters()
        {
            _parameters.Clear();
            _parametersArray = ImmutableArray<ParameterSymbol>.Empty;
        }

        internal void AddParameter(ParameterSymbol parameter)
        {
            _parameters.Add(parameter);
            _parametersArray = ImmutableArray<ParameterSymbol>.Empty;
        }

        protected bool Equals(InvocableSymbol other)
        {
            return base.Equals(other)
                   && _parameters.Count == other._parameters.Count
                   && _parameters.Zip(other._parameters, (x, y) => x.Equals(y)).All(x => x)
                   && ReturnType.Equals(other.ReturnType);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((InvocableSymbol) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ _parameters.GetHashCode();
                hashCode = (hashCode * 397) ^ ReturnType.GetHashCode();
                return hashCode;
            }
        }
    }
}