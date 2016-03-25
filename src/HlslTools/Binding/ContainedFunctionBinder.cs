using System.Collections.Generic;

namespace HlslTools.Binding
{
    internal class ContainedFunctionBinder : Binder
    {
        private readonly Binder _containerBinder;

        public ContainedFunctionBinder(SharedBinderState sharedBinderState, Binder parent, Binder containerBinder)
            : base(sharedBinderState, parent)
        {
            _containerBinder = containerBinder;
        }

        protected override IEnumerable<Binder> GetAdditionalParentBinders()
        {
            return _containerBinder.GetBinderChain();
        }
    }
}