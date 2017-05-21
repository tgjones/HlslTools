using ShaderTools.CodeAnalysis.Symbols;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Hlsl.Symbols
{
    public abstract class IntrinsicTypeSymbol : TypeSymbol
    {
        public override SourceRange? Location => null;

        internal IntrinsicTypeSymbol(SymbolKind kind, string name, string documentation)
            : base(kind, name, documentation, null)
        {
            
        }
    }
}