using ShaderTools.CodeAnalysis.Symbols;

namespace ShaderTools.CodeAnalysis.Hlsl.Symbols
{
    public abstract class IntrinsicTypeSymbol : TypeSymbol
    {
        internal IntrinsicTypeSymbol(SymbolKind kind, string name, string documentation)
            : base(kind, name, documentation, null)
        {
            
        }
    }
}