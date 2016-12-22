using ShaderTools.Core.Symbols;
using ShaderTools.Hlsl.Syntax;

namespace ShaderTools.Hlsl.Symbols
{
    public sealed class InterfaceSymbol : TypeSymbol
    {
        internal InterfaceSymbol(InterfaceTypeSyntax syntax, Symbol parent)
            : base(SymbolKind.Interface, syntax.Name.Text, string.Empty, parent)
        {
        }
    }
}