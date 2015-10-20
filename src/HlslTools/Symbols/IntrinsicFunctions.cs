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

            // From https://msdn.microsoft.com/en-us/library/windows/desktop/ff471350(v=vs.85).aspx

            allFunctions.Add(Create0("abort", "Submits an error message to the information queue and terminates the current draw or dispatch call being executed.", IntrinsicTypes.Void));

            allFunctions.AddRange(Create1("abs", "Returns the absolute value of the specified value.", IntrinsicTypes.AllNumericTypes));

            allFunctions.AddRange(Create1("acos", "Returns the arccosine of the specified value.", IntrinsicTypes.AllFloatTypes,
                "The specified value. Each component should be a floating-point value within the range of -1 to 1."));

            allFunctions.AddRange(Create1("all", "Determines if all components of the specified value are non-zero.", IntrinsicTypes.AllNumericTypes,
                overrideReturnType: IntrinsicTypes.Bool));

            allFunctions.Add(Create0("AllMemoryBarrier", "Blocks execution of all threads in a group until all memory accesses have been completed.", IntrinsicTypes.Void));

            allFunctions.Add(Create0("AllMemoryBarrierWithGroupSync", "Blocks execution of all threads in a group until all memory accesses have been completed and all threads in the group have reached this call.", IntrinsicTypes.Void));

            allFunctions.AddRange(Create1("any", "Determines if any components of the specified value are non-zero.", IntrinsicTypes.AllNumericTypes, overrideReturnType: IntrinsicTypes.Bool));

            allFunctions.AddRange(Create1("normalize", "Normalizes the specified floating-point vector according to x / length(x).", IntrinsicTypes.AllFloatVectorTypes,
                "The specified floating-point vector."));

            allFunctions.Add(new FunctionDeclarationSymbol("asdouble", "Reinterprets a cast value (two 32-bit values) into a double.", IntrinsicTypes.Double, f => new[]
            {
                new ParameterSymbol("lowbits", "The low 32-bit pattern of the input value.", f, IntrinsicTypes.Uint),
                new ParameterSymbol("highbits", "The high 32-bit pattern of the input value.", f, IntrinsicTypes.Uint)
            }));
            allFunctions.Add(new FunctionDeclarationSymbol("asdouble", "Reinterprets a cast value (four 32-bit values) into two doubles.", IntrinsicTypes.Double2, f => new[]
            {
                new ParameterSymbol("lowbits", "The low 32-bit pattern of the the input values.", f, IntrinsicTypes.Uint2),
                new ParameterSymbol("highbits", "The high 32-bit pattern of the input values.", f, IntrinsicTypes.Uint2)
            }));

            allFunctions.AddRange(Create1("atan", "Returns the arctangent of the specified value.", IntrinsicTypes.AllFloatTypes));

            allFunctions.AddRange(Create2("atan2", "Returns the arctangent of two values (x,y).", IntrinsicTypes.AllFloatTypes,
                "y", "The y value.",
                "x", "The x value."));

            AllFunctions = allFunctions;
        }

        private static FunctionDeclarationSymbol Create0(string name, string documentation, TypeSymbol returnType)
        {
            return new FunctionDeclarationSymbol(
                name, documentation, returnType,
                f => new ParameterSymbol[0]);
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

        private static IEnumerable<FunctionDeclarationSymbol> Create2(string name, string documentation, TypeSymbol[] types, 
            string parameterName1, string parameterDocumentation1, 
            string parameterName2, string parameterDocumentation2,
            TypeSymbol overrideReturnType = null)
        {
            return types.Select(type => new FunctionDeclarationSymbol(
                name, documentation, overrideReturnType ?? type,
                f => new[]
                {
                    new ParameterSymbol(parameterName1, parameterDocumentation1, f, type),
                    new ParameterSymbol(parameterName2, parameterDocumentation2, f, type)
                }));
        }
    }
}