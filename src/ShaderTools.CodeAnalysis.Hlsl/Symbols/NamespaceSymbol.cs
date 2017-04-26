using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Symbols;

namespace ShaderTools.CodeAnalysis.Hlsl.Symbols
{
    public sealed class NamespaceSymbol : ContainerSymbol
    {
        public NamespaceSyntax Syntax { get; }

        internal NamespaceSymbol(NamespaceSyntax syntax, Symbol parent)
            : base(SymbolKind.Namespace, syntax.Name.Text, string.Empty, parent)
        {
            Syntax = syntax;
        }
    }
}