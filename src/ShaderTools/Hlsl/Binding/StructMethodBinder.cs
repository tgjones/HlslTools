using System.Collections.Generic;
using ShaderTools.Hlsl.Symbols;

namespace ShaderTools.Hlsl.Binding
{
    internal sealed class StructMethodBinder : Binder
    {
        private readonly StructSymbol _structSymbol;

        public StructMethodBinder(SharedBinderState sharedBinderState, Binder parent, StructSymbol classSymbol)
            : base(sharedBinderState, parent)
        {
            _structSymbol = classSymbol;
        }

        protected override IEnumerable<Binder> GetAdditionalParentBinders()
        {
            var baseClass = (ClassOrStructSymbol) _structSymbol;
            while (baseClass != null)
            {
                yield return baseClass.Binder;
                baseClass = baseClass.BaseType;
            }
        }
    }
}