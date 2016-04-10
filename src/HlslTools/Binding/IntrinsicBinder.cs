using System.Collections.Generic;
using System.Linq;
using HlslTools.Symbols;

namespace HlslTools.Binding
{
    internal sealed class IntrinsicBinder : Binder
    {
        public IntrinsicBinder(SharedBinderState sharedBinderState)
            : base(sharedBinderState, null)
        {
            
        }

        protected internal override IEnumerable<Symbol> LocalSymbols
        {
            get
            {
                return IntrinsicFunctions.AllFunctions
                    .Cast<Symbol>()
                    .Union(IntrinsicSemantics.AllSemantics)
                    .Union(IntrinsicTypes.AllTypes)
                    .Union(IntrinsicNumericConstructors.AllFunctions);
            }
        }
    }
}