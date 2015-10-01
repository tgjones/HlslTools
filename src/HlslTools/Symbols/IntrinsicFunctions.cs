using System.Collections.Generic;
using System.Linq;

namespace HlslTools.Symbols
{
    public static class IntrinsicFunctions
    {
        public static readonly IEnumerable<FunctionDeclarationSymbol> AllFunctions;

        static IntrinsicFunctions()
        {
            var allFunctions = new List<FunctionDeclarationSymbol>();

            allFunctions.AddRange(Create1("abs", "Returns the absolute value of the specified value.", IntrinsicTypes.AllNumericTypes));

            AllFunctions = allFunctions;
        }

        private static IEnumerable<FunctionDeclarationSymbol> Create1(string name, string documentation, TypeSymbol[] types)
        {
            return types.Select(type => new FunctionDeclarationSymbol(
                name, documentation, type,
                f => new []
                {
                    new ParameterSymbol("value", "The specified value.", f, type)
                }));
        }
    }
}