using ShaderTools.CodeAnalysis.Symbols;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Hlsl.Symbols
{
    public sealed class IndexerSymbol : InvocableSymbol
    {
        internal IndexerSymbol(string name, string documentation, TypeSymbol parent, TypeSymbol indexType, TypeSymbol valueType, bool readOnly = true)
            : base(SymbolKind.Indexer, name, documentation, parent, valueType)
        {
            IndexType = indexType;
            ValueType = valueType;
            ReadOnly = readOnly;
        }

        public TypeSymbol IndexType { get; }
        public TypeSymbol ValueType { get; }
        public bool ReadOnly { get; }

        public override SourceRange? Location => null;

        private bool Equals(IndexerSymbol other)
        {
            return base.Equals(other) && IndexType.Equals(other.IndexType) && ValueType.Equals(other.ValueType) && ReadOnly == other.ReadOnly;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is IndexerSymbol && Equals((IndexerSymbol) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ IndexType.GetHashCode();
                hashCode = (hashCode * 397) ^ ValueType.GetHashCode();
                hashCode = (hashCode * 397) ^ ReadOnly.GetHashCode();
                return hashCode;
            }
        }
    }
}