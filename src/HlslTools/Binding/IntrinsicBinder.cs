using System.Collections.Generic;
using HlslTools.Symbols;

namespace HlslTools.Binding
{
    internal sealed class IntrinsicBinder : Binder
    {
        public IntrinsicBinder(SharedBinderState sharedBinderState)
            : base(sharedBinderState, null)
        {
            
        }

        protected override IEnumerable<Symbol> LocalSymbols => IntrinsicFunctions.AllFunctions;
    }
}