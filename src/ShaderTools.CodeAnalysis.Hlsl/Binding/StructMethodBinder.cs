using System.Collections.Generic;
using ShaderTools.CodeAnalysis.Hlsl.Symbols;

namespace ShaderTools.CodeAnalysis.Hlsl.Binding
{
    internal sealed class StructMethodBinder : FunctionBinder
    {
        private readonly StructSymbol _structSymbol;

        public StructMethodBinder(SharedBinderState sharedBinderState, Binder parent, StructSymbol classSymbol, FunctionSymbol functionSymbol)
            : base(sharedBinderState, parent, functionSymbol)
        {
            _structSymbol = classSymbol;
        }

        protected override IEnumerable<Binder> GetAdditionalParentBinders()
        {
            var baseClass = (StructSymbol) _structSymbol;
            while (baseClass != null)
            {
                yield return baseClass.Binder;
                baseClass = baseClass.BaseType;
            }
        }
    }
}