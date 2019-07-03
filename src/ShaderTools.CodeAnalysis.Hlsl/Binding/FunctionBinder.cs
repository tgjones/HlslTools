using ShaderTools.CodeAnalysis.Hlsl.Symbols;

namespace ShaderTools.CodeAnalysis.Hlsl.Binding
{
    internal class FunctionBinder : Binder
    {
        internal override Symbol ContainingMember { get; }

        public FunctionBinder(SharedBinderState sharedBinderState, Binder parent, FunctionSymbol functionSymbol)
            : base(sharedBinderState, parent)
        {
            ContainingMember = functionSymbol;
        }
    }
}
