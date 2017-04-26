using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Symbols;

namespace ShaderTools.CodeAnalysis.Hlsl.Symbols
{
    public sealed class InterfaceSymbol : TypeSymbol
    {
        internal InterfaceSymbol(InterfaceTypeSyntax syntax, Symbol parent)
            : base(SymbolKind.Interface, syntax.Name.Text, string.Empty, parent)
        {
        }
    }
}