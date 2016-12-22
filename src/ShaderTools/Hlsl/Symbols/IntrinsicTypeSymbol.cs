using ShaderTools.Core.Symbols;

namespace ShaderTools.Hlsl.Symbols
{
    public abstract class IntrinsicTypeSymbol : TypeSymbol
    {
        internal IntrinsicTypeSymbol(SymbolKind kind, string name, string documentation)
            : base(kind, name, documentation, null)
        {
            
        }
    }
}