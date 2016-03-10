namespace HlslTools.Symbols
{
    public class FieldSymbol : VariableSymbol, IMemberSymbol
    {
        internal FieldSymbol(string name, string documentation, TypeSymbol parent, TypeSymbol valueType)
            : base(SymbolKind.Field, name, documentation, parent, valueType)
        {
            
        }

        TypeSymbol IMemberSymbol.AssociatedType => ValueType;
    }
}