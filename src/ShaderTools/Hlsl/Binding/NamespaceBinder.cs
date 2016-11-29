using ShaderTools.Hlsl.Symbols;

namespace ShaderTools.Hlsl.Binding
{
    internal sealed class NamespaceBinder : Binder
    {
        public NamespaceSymbol NamespaceSymbol { get; }

        public NamespaceBinder(SharedBinderState sharedBinderState, Binder parent, NamespaceSymbol namespaceSymbol)
            : base(sharedBinderState, parent)
        {
            NamespaceSymbol = namespaceSymbol;
        }
    }
}