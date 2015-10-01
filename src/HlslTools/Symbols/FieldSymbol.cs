namespace HlslTools.Symbols
{
    public class FieldSymbol : MemberSymbol
    {
        internal FieldSymbol(string name, string documentation, TypeSymbol parent, TypeSymbol valueType)
            : base(SymbolKind.Field, name, documentation, parent, valueType)
        {
            
        }
    }
}