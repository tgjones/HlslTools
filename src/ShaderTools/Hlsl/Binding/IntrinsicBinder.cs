using System.Collections.Generic;
using System.Linq;
using ShaderTools.Hlsl.Symbols;

namespace ShaderTools.Hlsl.Binding
{
    internal sealed class IntrinsicBinder : Binder
    {
        private static readonly Dictionary<string, List<Symbol>> LocalSymbolDictionary;

        static IntrinsicBinder()
        {
            LocalSymbolDictionary = IntrinsicFunctions.AllFunctions
                .Union(IntrinsicFunctions.AllXboxFunctions)
                .Cast<Symbol>()
                .Union(IntrinsicSemantics.AllSemantics)
                .Union(IntrinsicTypes.AllTypes)
                .Union(IntrinsicNumericConstructors.AllFunctions)
                .GroupBy(x => x.Name)
                .ToDictionary(x => x.Key, x => x.ToList());
        }

        public IntrinsicBinder(SharedBinderState sharedBinderState)
            : base(sharedBinderState, null)
        {
            
        }

        protected internal override Dictionary<string, List<Symbol>> LocalSymbols => LocalSymbolDictionary;
    }
}