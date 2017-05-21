using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Symbols;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Hlsl.Symbols
{
    public sealed class InterfaceSymbol : TypeSymbol
    {
        public override SourceRange? Location { get; }

        internal InterfaceSymbol(InterfaceTypeSyntax syntax, Symbol parent)
            : base(SymbolKind.Interface, syntax.Name.Text, string.Empty, parent)
        {
            Location = syntax.Name.SourceRange;
        }
    }
}