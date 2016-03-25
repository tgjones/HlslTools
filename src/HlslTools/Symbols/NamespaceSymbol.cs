using HlslTools.Syntax;

namespace HlslTools.Symbols
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