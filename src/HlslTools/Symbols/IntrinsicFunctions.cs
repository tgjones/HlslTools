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
            allFunctions.AddRange(Create1("acos", "Returns the arccosine of the specified value.", IntrinsicTypes.AllNumericTypes, "The specified value. Each component should be a floating-point value within the range of -1 to 1."));
            allFunctions.AddRange(Create1("all", "Determines if all components of the specified value are non-zero.", IntrinsicTypes.AllNumericTypes, overrideReturnType: IntrinsicTypes.Bool));
            allFunctions.AddRange(Create1("any", "Determines if any components of the specified value are non-zero.", IntrinsicTypes.AllNumericTypes, overrideReturnType: IntrinsicTypes.Bool));
            allFunctions.AddRange(Create1("normalize", "Normalizes the specified floating-point vector according to x / length(x).", IntrinsicTypes.AllFloatVectorTypes, "The specified floating-point vector."));

            AllFunctions = allFunctions;
        }

        private static IEnumerable<FunctionDeclarationSymbol> Create1(string name, string documentation, TypeSymbol[] types, string parameterDocumentation = null, TypeSymbol overrideReturnType = null)
        {
            return types.Select(type => new FunctionDeclarationSymbol(
                name, documentation, overrideReturnType ?? type,
                f => new []
                {
                    new ParameterSymbol("value", parameterDocumentation ?? "The specified value.", f, type)
                }));
        }
    }
}