using ShaderTools.CodeAnalysis.Symbols;

namespace ShaderTools.CodeAnalysis.Hlsl.Symbols
{
    public class FieldSymbol : VariableSymbol
    {
        internal FieldSymbol(string name, string documentation, TypeSymbol parent, TypeSymbol valueType)
            : base(SymbolKind.Field, name, documentation, parent, valueType)
        {
            
        }
    }
}