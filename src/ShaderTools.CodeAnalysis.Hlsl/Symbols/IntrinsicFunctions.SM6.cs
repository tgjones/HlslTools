using System;
using System.Collections.Generic;

namespace ShaderTools.CodeAnalysis.Hlsl.Symbols
{
    partial class IntrinsicFunctions
    {
        public static readonly IEnumerable<FunctionSymbol> AllSM6Functions = CreateSM6Functions();

        private static IEnumerable<FunctionSymbol> CreateSM6Functions()
        {
            var allFunctions = new List<FunctionSymbol>();

            allFunctions.Add(Create0(
                "WaveGetLaneCount",
                "Returns the number of lanes in a wave on this architecture.",
                IntrinsicTypes.Uint));

            allFunctions.Add(Create0(
                "WaveGetLaneIndex",
                "Returns the index of the current lane within the current wave.",
                IntrinsicTypes.Uint));

            allFunctions.Add(Create0(
                "WaveIsFirstLane",
                "Returns true only for the active lane in the current wave with the smallest index.",
                IntrinsicTypes.Bool));

            allFunctions.AddRange(Create1(
                "WaveActiveAnyTrue",
                "Returns true if the expression is true in any of the active lanes in the current wave.",
                new[] { IntrinsicTypes.Bool },
                "expr",
                "The boolean expression to evaluate."));

            allFunctions.AddRange(Create1(
                "WaveActiveAllTrue",
                "Returns true if the expression is true in all active lanes in the current wave.",
                new[] { IntrinsicTypes.Bool },
                "expr",
                "The boolean expression to evaluate."));

            allFunctions.AddRange(Create1(
                "WaveActiveBallot",
                "Returns a 4-bit unsigned integer bitmask of the evaluation of the Boolean expression for all active lanes in the specified wave.",
                new[] { IntrinsicTypes.Bool },
                "expr",
                "The boolean expression to evaluate.",
                new[] { IntrinsicTypes.Uint4 }));

            allFunctions.AddRange(Create2(
                "WaveReadLaneAt",
                "Returns the value of the expression for the given lane index within the specified wave.",
                IntrinsicTypes.AllNumericTypes,
                "expr",
                "The expression to evaluate.",
                "laneIndex",
                "The input lane index must be uniform across the wave.",
                overrideParameterType2: IntrinsicTypes.Uint));

            allFunctions.AddRange(Create1(
                "WaveReadLaneFirst",
                "Returns the value of the expression for the active lane of the current wave with the smallest index.",
                IntrinsicTypes.AllNumericTypes,
                "expr",
                "The expression to evaluate."));

            allFunctions.AddRange(Create1(
                "WaveActiveAllEqual",
                "Returns true if the expression is the same for every active lane in the current wave (and thus uniform across it).",
                IntrinsicTypes.AllNumericTypes,
                "expr",
                "The expression to evaluate.",
                new[] { IntrinsicTypes.Bool }));

            allFunctions.AddRange(Create1(
                "WaveActiveBitAnd",
                "Returns the bitwise AND of all the values of the expression across all active lanes in the current wave and replicates it back to all active lanes.",
                IntrinsicTypes.AllIntTypes,
                "expr",
                "The expression to evaluate."));

            allFunctions.AddRange(Create1(
                "WaveActiveBitOr",
                "Returns the bitwise OR of all the values of the expression across all active lanes in the current wave and replicates it back to all active lanes.",
                IntrinsicTypes.AllIntTypes,
                "expr",
                "The expression to evaluate."));

            allFunctions.AddRange(Create1(
                "WaveActiveBitXor",
                "Returns the bitwise XOR of all the values of the expression across all active lanes in the current wave and replicates it back to all active lanes.",
                IntrinsicTypes.AllIntTypes,
                "expr",
                "The expression to evaluate."));

            allFunctions.AddRange(Create1(
                "WaveActiveCountBits",
                "Counts the number of boolean variables which evaluate to true across all active lanes in the current wave, and replicates the result to all lanes in the wave.",
                new[] { IntrinsicTypes.Bool },
                "bBit",
                "The boolean variables to evaluate. Providing an explicit true Boolean value returns the number of active lanes.",
                new[] { IntrinsicTypes.Uint }));

            allFunctions.AddRange(Create1(
                "WaveActiveMax",
                "Returns the maximum value of the expression across all active lanes in the current wave and replicates it back to all active lanes.",
                IntrinsicTypes.AllNumericTypes,
                "expr",
                "The expression to evaluate."));

            allFunctions.AddRange(Create1(
                "WaveActiveMin",
                "Returns the minimum value of the expression across all active lanes in the current wave and replicates it back to all active lanes.",
                IntrinsicTypes.AllNumericTypes,
                "expr",
                "The expression to evaluate."));

            allFunctions.AddRange(Create1(
                "WaveActiveProduct",
                "Multiplies the values of the expression together across all active lanes in the current wave and replicates it back to all active lanes.",
                IntrinsicTypes.AllNumericTypes,
                "expr",
                "The expression to evaluate."));

            allFunctions.AddRange(Create1(
                "WaveActiveSum",
                "Sums up the value of the expression across all active lanes in the current wave and replicates it to all lanes in the current wave.",
                IntrinsicTypes.AllNumericTypes,
                "expr",
                "The expression to evaluate."));

            allFunctions.AddRange(Create1(
                "WavePrefixCountBits",
                "Returns the sum of all the specified boolean variables set to true across all active lanes with indices smaller than the current lane.",
                new[] { IntrinsicTypes.Bool },
                "bBit",
                "The specified boolean variables.",
                new[] { IntrinsicTypes.Uint }));

            allFunctions.AddRange(Create1(
                "WavePrefixSum",
                "Returns the sum of all of the values in the active lanes with smaller indices than this one.",
                IntrinsicTypes.AllNumericTypes,
                "value",
                "The value to sum up."));

            allFunctions.AddRange(Create1(
                "WavePrefixProduct",
                "Returns the product of all of the values in the active lanes in this wave with indices less than this lane.",
                IntrinsicTypes.AllNumericTypes,
                "value",
                "The value to multiply."));

            allFunctions.AddRange(Create2(
                "QuadReadLaneAt",
                "Returns the specified source value from the lane identified by the lane ID within the current quad.",
                IntrinsicTypes.AllNumericTypes,
                "sourceValue",
                "The requested type.",
                "quadLaneID",
                "The lane ID; this will be a value from 0 to 3.",
                overrideParameterType2: IntrinsicTypes.Uint));

            allFunctions.AddRange(Create1(
                "QuadReadAcrossDiagonal",
                "Returns the specified local value which is read from the diagonally opposite lane in this quad.",
                IntrinsicTypes.AllNumericTypes,
                "localValue",
                "The requested type."));

            allFunctions.AddRange(Create1(
                "QuadReadAcrossX",
                "Returns the specified local value read from the other lane in this quad in the X direction.",
                IntrinsicTypes.AllNumericTypes,
                "localValue",
                "The requested type."));

            allFunctions.AddRange(Create1(
                "QuadReadAcrossY",
                "Returns the specified local value read from the other lane in this quad in the Y direction.",
                IntrinsicTypes.AllNumericTypes,
                "localValue",
                "The requested type."));

            return allFunctions;
        }
    }
}
