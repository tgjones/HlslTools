using HlslTools.Symbols;

namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundNamespace : BoundNode
    {
        public NamespaceSymbol NamespaceSymbol { get; }

        public BoundNamespace(NamespaceSymbol namespaceSymbol)
            : base(BoundNodeKind.Namespace)
        {
            NamespaceSymbol = namespaceSymbol;
        }
    }
}