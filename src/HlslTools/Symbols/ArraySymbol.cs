using System.Collections.Generic;

namespace HlslTools.Symbols
{
    public sealed class ArraySymbol : TypeSymbol
    {
        internal ArraySymbol(TypeSymbol valueType)
            : base(SymbolKind.Array, $"{valueType.FullName}[]", "Array of " + valueType.Name, null)
        {
            foreach (var member in CreateArrayMembers(this, valueType))
                AddMember(member);
        }

        private static IEnumerable<Symbol> CreateArrayMembers(TypeSymbol parent, TypeSymbol valueType)
        {
            yield return new IndexerSymbol("[]", string.Empty, parent, valueType);
        }
    }
}