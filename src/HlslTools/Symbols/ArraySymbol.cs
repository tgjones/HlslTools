using System.Collections.Generic;

namespace HlslTools.Symbols
{
    public class ArraySymbol : TypeSymbol
    {
        public ArraySymbol(TypeSymbol valueType)
            : base(SymbolKind.Array, $"{valueType.FullName}[]", "Array of " + valueType.Name, null, t => CreateArrayMembers(t, valueType))
        {
        }

        private static IEnumerable<MemberSymbol> CreateArrayMembers(TypeSymbol parent, TypeSymbol valueType)
        {
            yield return new IndexerSymbol("[]", string.Empty, parent, valueType);
        }
    }
}