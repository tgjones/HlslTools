using System;

namespace HlslTools.Symbols
{
    public class ParameterSymbol : VariableSymbol
    {
        public ParameterDirection Direction { get; }

        internal ParameterSymbol(string name, string documentation, Symbol parent, TypeSymbol valueType, ParameterDirection direction = ParameterDirection.In)
            : base(SymbolKind.Parameter, name, documentation, parent, valueType)
        {
            if (valueType == null)
                throw new ArgumentNullException(nameof(valueType));

            Direction = direction;
        }

        protected bool Equals(ParameterSymbol other)
        {
            return base.Equals(other) && Direction == other.Direction;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ParameterSymbol) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ (int) Direction;
            }
        }

        public virtual bool HasDefaultValue => false;
    }
}