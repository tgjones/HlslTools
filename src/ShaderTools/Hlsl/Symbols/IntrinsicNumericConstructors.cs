using System.Collections.Generic;
using System.Linq;

namespace ShaderTools.Hlsl.Symbols
{
    internal static class IntrinsicNumericConstructors
    {
        public static readonly IEnumerable<FunctionSymbol> AllFunctions;

        static IntrinsicNumericConstructors()
        {
            var allFunctions = new List<FunctionSymbol>();

            foreach (var scalarType in IntrinsicTypes.AllScalarTypes)
            {
                var vector1Type = IntrinsicTypes.GetVectorType(scalarType.ScalarType, 1);
                var vector2Type = IntrinsicTypes.GetVectorType(scalarType.ScalarType, 2);
                var vector3Type = IntrinsicTypes.GetVectorType(scalarType.ScalarType, 3);
                var vector4Type = IntrinsicTypes.GetVectorType(scalarType.ScalarType, 4);

                allFunctions.Add(new FunctionSymbol(
                    scalarType.Name,
                    $"Constructor function for {scalarType.Name}.",
                    null,
                    scalarType,
                    f => new[]
                    {
                        new ParameterSymbol("x", "Value for the x component.", f, scalarType)
                    },
                    isNumericConstructor: true));

                allFunctions.Add(new FunctionSymbol(
                    vector1Type.Name,
                    $"Constructor function for {vector1Type.Name}.",
                    null,
                    vector1Type,
                    f => new[]
                    {
                        new ParameterSymbol("x", "Value for the x component.", f, scalarType)
                    },
                    isNumericConstructor: true));

                allFunctions.Add(new FunctionSymbol(
                    vector2Type.Name,
                    $"Constructor function for {vector2Type.Name}.",
                    null,
                    vector2Type,
                    f => new[]
                    {
                        new ParameterSymbol("x", "Value for the x component.", f, scalarType),
                        new ParameterSymbol("y", "Value for the y component.", f, scalarType)
                    },
                    isNumericConstructor: true));

                allFunctions.Add(new FunctionSymbol(
                    vector2Type.Name,
                    $"Constructor function for {vector2Type.Name}.",
                    null,
                    vector2Type,
                    f => new[]
                    {
                        new ParameterSymbol("xy", "Value for the x and y components.", f, vector2Type)
                    },
                    isNumericConstructor: true));

                allFunctions.Add(new FunctionSymbol(
                    vector3Type.Name,
                    $"Constructor function for {vector3Type.Name}.",
                    null,
                    vector3Type,
                    f => new[]
                    {
                        new ParameterSymbol("x", "Value for the x component.", f, scalarType),
                        new ParameterSymbol("y", "Value for the y component.", f, scalarType),
                        new ParameterSymbol("z", "Value for the z component.", f, scalarType)
                    },
                    isNumericConstructor: true));

                allFunctions.Add(new FunctionSymbol(
                    vector3Type.Name,
                    $"Constructor function for {vector3Type.Name}.",
                    null,
                    vector3Type,
                    f => new[]
                    {
                        new ParameterSymbol("xy", "Value for the x and y components.", f, vector2Type),
                        new ParameterSymbol("z", "Value for the z component.", f, scalarType)
                    },
                    isNumericConstructor: true));

                allFunctions.Add(new FunctionSymbol(
                    vector3Type.Name,
                    $"Constructor function for {vector3Type.Name}.",
                    null,
                    vector3Type,
                    f => new[]
                    {
                        new ParameterSymbol("x", "Value for the x component.", f, scalarType),
                        new ParameterSymbol("yz", "Value for the y and z components.", f, vector2Type)
                    },
                    isNumericConstructor: true));

                allFunctions.Add(new FunctionSymbol(
                    vector3Type.Name,
                    $"Constructor function for {vector3Type.Name}.",
                    null,
                    vector3Type,
                    f => new[]
                    {
                        new ParameterSymbol("xyz", "Value for the x, y, and z components.", f, vector3Type),
                    },
                    isNumericConstructor: true));

                allFunctions.Add(new FunctionSymbol(
                    vector4Type.Name,
                    $"Constructor function for {vector4Type.Name}.",
                    null,
                    vector4Type,
                    f => new[]
                    {
                        new ParameterSymbol("x", "Value for the x component.", f, scalarType),
                        new ParameterSymbol("y", "Value for the y component.", f, scalarType),
                        new ParameterSymbol("z", "Value for the z component.", f, scalarType),
                        new ParameterSymbol("w", "Value for the w component.", f, scalarType)
                    },
                    isNumericConstructor: true));

                allFunctions.Add(new FunctionSymbol(
                    vector4Type.Name,
                    $"Constructor function for {vector4Type.Name}.",
                    null,
                    vector4Type,
                    f => new[]
                    {
                        new ParameterSymbol("xy", "Value for the x and y components.", f, vector2Type),
                        new ParameterSymbol("z", "Value for the z component.", f, scalarType),
                        new ParameterSymbol("w", "Value for the w component.", f, scalarType)
                    },
                    isNumericConstructor: true));

                allFunctions.Add(new FunctionSymbol(
                    vector4Type.Name,
                    $"Constructor function for {vector4Type.Name}.",
                    null,
                    vector4Type,
                    f => new[]
                    {
                        new ParameterSymbol("xy", "Value for the x and y components.", f, vector2Type),
                        new ParameterSymbol("zw", "Value for the z and w components.", f, vector2Type)
                    },
                    isNumericConstructor: true));

                allFunctions.Add(new FunctionSymbol(
                    vector4Type.Name,
                    $"Constructor function for {vector4Type.Name}.",
                    null,
                    vector4Type,
                    f => new[]
                    {
                        new ParameterSymbol("x", "Value for the x component.", f, scalarType),
                        new ParameterSymbol("yz", "Value for the y and z components.", f, vector2Type),
                        new ParameterSymbol("w", "Value for the w component.", f, scalarType)
                    },
                    isNumericConstructor: true));

                allFunctions.Add(new FunctionSymbol(
                    vector4Type.Name,
                    $"Constructor function for {vector4Type.Name}.",
                    null,
                    vector4Type,
                    f => new[]
                    {
                        new ParameterSymbol("x", "Value for the x component.", f, scalarType),
                        new ParameterSymbol("y", "Value for the y component.", f, scalarType),
                        new ParameterSymbol("zw", "Value for the z and w components.", f, vector2Type)
                    },
                    isNumericConstructor: true));

                allFunctions.Add(new FunctionSymbol(
                    vector4Type.Name,
                    $"Constructor function for {vector4Type.Name}.",
                    null,
                    vector4Type,
                    f => new[]
                    {
                        new ParameterSymbol("x", "Value for the x component.", f, scalarType),
                        new ParameterSymbol("yzw", "Value for the y, z, and w components.", f, vector3Type),
                    },
                    isNumericConstructor: true));

                allFunctions.Add(new FunctionSymbol(
                    vector4Type.Name,
                    $"Constructor function for {vector4Type.Name}.",
                    null,
                    vector4Type,
                    f => new[]
                    {
                        new ParameterSymbol("xyz", "Value for the x, y, and z component.", f, vector3Type),
                        new ParameterSymbol("w", "Value for the w component.", f, scalarType),
                    },
                    isNumericConstructor: true));

                allFunctions.Add(new FunctionSymbol(
                    vector4Type.Name,
                    $"Constructor function for {vector4Type.Name}.",
                    null,
                    vector4Type,
                    f => new[]
                    {
                        new ParameterSymbol("xyzw", "Value for the x, y, z, and w components.", f, vector4Type),
                    },
                    isNumericConstructor: true));

                foreach (var matrixType in IntrinsicTypes.AllMatrixTypes.Where(x => x.ScalarType == scalarType.ScalarType))
                {
                    allFunctions.Add(new FunctionSymbol(
                        matrixType.Name,
                        $"Constructor function for {matrixType.Name}.",
                        null,
                        matrixType,
                        f =>
                        {
                            var result = new List<ParameterSymbol>();
                            for (var rows = 1; rows <= matrixType.Rows; rows++)
                                for (var cols = 1; cols <= matrixType.Cols; cols++)
                                    result.Add(new ParameterSymbol($"m{rows}{cols}", $"Value for the component in row {rows} and column {cols}.", f, scalarType));
                            return result.ToArray();
                        },
                        isNumericConstructor: true));

                    allFunctions.Add(new FunctionSymbol(
                        matrixType.Name,
                        $"Constructor function for {matrixType.Name}.",
                        null,
                        matrixType,
                        f =>
                        {
                            var result = new List<ParameterSymbol>();
                            for (var rows = 0; rows < matrixType.Rows; rows++)
                                result.Add(new ParameterSymbol($"row{rows}", $"Value for the components in row {rows}.", f, IntrinsicTypes.GetVectorType(scalarType.ScalarType, matrixType.Cols)));
                            return result.ToArray();
                        },
                        isNumericConstructor: true));
                }
            }

            AllFunctions = allFunctions;
        }
    }
}