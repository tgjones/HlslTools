using System.Collections.Generic;
using ShaderTools.Hlsl.Symbols;

namespace ShaderTools.Hlsl.Binding
{
    internal sealed class ClassMethodBinder : Binder
    {
        private readonly ClassSymbol _classSymbol;

        public ClassMethodBinder(SharedBinderState sharedBinderState, Binder parent, ClassSymbol classSymbol)
            : base(sharedBinderState, parent)
        {
            _classSymbol = classSymbol;
        }

        protected override IEnumerable<Binder> GetAdditionalParentBinders()
        {
            var baseClass = (ClassOrStructSymbol) _classSymbol;
            while (baseClass != null)
            {
                yield return baseClass.Binder;
                baseClass = baseClass.BaseType;
            }
        }
    }
}