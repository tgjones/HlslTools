using System.Collections.Generic;

namespace HlslTools.Symbols
{
    public sealed class ArraySymbol : TypeSymbol
    {
        public int? Dimension { get; }

        internal ArraySymbol(TypeSymbol valueType, int? dimension)
            : base(SymbolKind.Array, $"{valueType.FullName}[]", "Array of " + valueType.Name, null)
        {
            foreach (var member in CreateArrayMembers(this, valueType))
                AddMember(member);

            Dimension = dimension;
        }

        private static IEnumerable<Symbol> CreateArrayMembers(TypeSymbol parent, TypeSymbol valueType)
        {
            yield return new IndexerSymbol("[]", string.Empty, parent, valueType);
        }
    }
}