using HlslTools.Syntax;

namespace HlslTools.Symbols
{
    public class VariableSymbol : Symbol
    {
        internal VariableSymbol(VariableDeclaratorSyntax syntax, Symbol parent, TypeSymbol valueType)
            : base(SymbolKind.Variable, syntax.Identifier.Text, string.Empty, parent)
        {
            ValueType = valueType;
        }

        internal VariableSymbol(SymbolKind kind, string name, string documentation, Symbol parent, TypeSymbol valueType)
            : base(kind, name, documentation, parent)
        {
            ValueType = valueType;
        }

        public TypeSymbol ValueType { get; }

        protected bool Equals(VariableSymbol other)
        {
            return base.Equals(other) && ValueType.Equals(other.ValueType);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((VariableSymbol) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ ValueType.GetHashCode();
            }
        }
    }
}