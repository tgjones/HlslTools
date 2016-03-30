using System.Collections.Generic;
using HlslTools.Symbols;

namespace HlslTools.Binding
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
            var baseClass = _classSymbol;
            while (baseClass != null)
            {
                yield return baseClass.Binder;
                baseClass = baseClass.BaseClass;
            }
        }
    }
}