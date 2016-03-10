using System.Collections.Generic;

namespace HlslTools.Symbols
{
    public sealed class ArraySymbol : TypeSymbol
    {
        internal ArraySymbol(TypeSymbol valueType)
            : base(SymbolKind.Array, $"{valueType.FullName}[]", "Array of " + valueType.Name, null, t => CreateArrayMembers(t, valueType))
        {
        }

        private static IEnumerable<Symbol> CreateArrayMembers(TypeSymbol parent, TypeSymbol valueType)
        {
            yield return new IndexerSymbol("[]", string.Empty, parent, valueType);
        }
    }
}