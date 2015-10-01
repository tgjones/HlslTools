namespace HlslTools.Symbols
{
    public class LocalVariableSymbol : LocalSymbol
    {
        public LocalVariableSymbol(string name, string documentation, Symbol parent, TypeSymbol valueType)
            : this(SymbolKind.LocalVariable, name, documentation, parent, valueType)
        {
            
        }

        protected LocalVariableSymbol(SymbolKind kind, string name, string documentation, Symbol parent, TypeSymbol valueType)
            : base(kind, name, documentation, parent, valueType)
        {

        }
    }
}