using System;
using System.Collections.Generic;
using System.Linq;
using HlslTools.Binding.Signatures;
using HlslTools.Symbols;

namespace HlslTools.Binding
{
    internal static class OverloadResolution
    {
        public static OverloadResolutionResult<T> Perform<T>(IEnumerable<T> signatures, IReadOnlyList<TypeSymbol> argumentTypes)
            where T : Signature
        {
            var candidates = new List<OverloadResolutionCandidate<T>>();
            foreach (var signature in signatures)
            {
                ulong score;
                if (!TryRankArguments(signature, argumentTypes, out score))
                    continue;

                candidates.Add(new OverloadResolutionCandidate<T>(signature, score));
            }

            candidates.Sort((l, r) => l.Score.CompareTo(r.Score));

            var selected = candidates.FirstOrDefault();

            var best = (selected != null && (candidates.Count < 2 || candidates[1].Score > selected.Score))
                ? selected
                : null;

            return new OverloadResolutionResult<T>(best, selected, candidates);
        }

        private static bool TryRankArguments<T>(T signature, IReadOnlyList<TypeSymbol> argumentTypes, out ulong score)
            where T : Signature
        {
            score = 0;

            var argumentCount = argumentTypes.Count;

            if (signature.HasVariadicParameter)
            {
                if (argumentCount < signature.ParameterCount)
                    return false;
            }
            else
            {
                if (signature.ParameterCount != argumentCount)
                    return false;
            }

            // ReSharper disable InconsistentNaming
            ulong intConversions = 0;
            ulong i2fConversions = 0;
            ulong f2iConversions = 0;
            ulong f2hConversions = 0;
            ulong f2dConversions = 0;
            ulong scalarPromotions = 0;
            ulong sameSizeTruncations = 0;
            ulong truncations = 0;
            // ReSharper restore InconsistentNaming

            for (var i = 0; i < signature.ParameterCount; i++)
            {
                var parameterType = signature.GetParameterType(i);
                var argumentType = argumentTypes[i];

                if (parameterType.Equals(argumentType))
                    continue; // i.e. score == 0 for this argument

                // First, make sure we have an implicit conversion from argument to parameter.
                switch (signature.GetParameterDirection(i))
                {
                    case ParameterDirection.In:
                        if (!argumentType.HasImplicitConversionTo(parameterType))
                            return false;
                        break;
                    case ParameterDirection.Out:
                        if (!parameterType.HasImplicitConversionTo(argumentType))
                            return false;
                        break;
                    case ParameterDirection.Inout:
                        if (!argumentType.HasImplicitConversionTo(parameterType) || !parameterType.HasImplicitConversionTo(argumentType))
                            return false;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                var numericParameterType = parameterType as IntrinsicNumericTypeSymbol;
                var numericArgumentType = argumentType as IntrinsicNumericTypeSymbol;
                if (numericParameterType != null && numericArgumentType != null)
                {
                    var parameterScalarType = numericParameterType.ScalarType;
                    var argumentScalarType = numericArgumentType.ScalarType;

                    if (argumentScalarType == ScalarType.Float && parameterScalarType == ScalarType.Half)
                    {
                        f2hConversions++;
                    }
                    else if (argumentScalarType == ScalarType.Float && parameterScalarType == ScalarType.Double)
                    {
                        f2dConversions++;
                    }
                    else if (argumentScalarType == ScalarType.Half && parameterScalarType == ScalarType.Float)
                    {
                        f2dConversions++;
                    }
                    else if (argumentScalarType == ScalarType.Half && parameterScalarType == ScalarType.Double)
                    {
                        f2dConversions++;
                    }
                    else if (argumentScalarType == ScalarType.Double && parameterScalarType == ScalarType.Half)
                    {
                        f2hConversions++;
                    }
                    else if (argumentScalarType == ScalarType.Double && parameterScalarType == ScalarType.Float)
                    {
                        f2hConversions++;
                    }
                    else if (argumentScalarType.IsFloat() && !parameterScalarType.IsFloat())
                    {
                        f2iConversions++;
                    }
                    else if (!argumentScalarType.IsFloat() && parameterScalarType.IsFloat())
                    {
                        i2fConversions++;
                    }
                    else if (argumentScalarType == ScalarType.Bool && parameterScalarType != ScalarType.Bool)
                    {
                        i2fConversions++;
                    }
                    else if (argumentScalarType != parameterScalarType)
                    {
                        intConversions++;
                    }

                    var parameterDimension0 = numericParameterType.GetDimensionSize(0);
                    var parameterDimension1 = numericParameterType.GetDimensionSize(1);
                    var argumentDimension0 = numericArgumentType.GetDimensionSize(0);
                    var argumentDimension1 = numericArgumentType.GetDimensionSize(1);

                    if ((argumentDimension0 == parameterDimension0 && argumentDimension1 == parameterDimension1)
                        || argumentDimension1 == parameterDimension0 && argumentDimension0 == parameterDimension1)
                    {
                        if ((argumentType.Kind == SymbolKind.IntrinsicMatrixType && parameterType.Kind == SymbolKind.IntrinsicVectorType)
                            || (argumentType.Kind == SymbolKind.IntrinsicVectorType && parameterType.Kind == SymbolKind.IntrinsicScalarType))
                        {
                            // float1x1 => float1
                            // float1   => float
                            sameSizeTruncations++;
                        }
                        if (argumentType.Kind == SymbolKind.IntrinsicMatrixType && parameterType.Kind == SymbolKind.IntrinsicScalarType)
                        {
                            // float1x1 => float
                            sameSizeTruncations++;
                        }
                        else if ((argumentType.Kind == SymbolKind.IntrinsicScalarType && parameterType.Kind == SymbolKind.IntrinsicVectorType)
                            || (argumentType.Kind == SymbolKind.IntrinsicVectorType && parameterType.Kind == SymbolKind.IntrinsicMatrixType))
                        {
                            // float  => float1
                            // float1 => float1x1
                            scalarPromotions++;
                        }
                        else if (argumentType.Kind == SymbolKind.IntrinsicScalarType && parameterType.Kind == SymbolKind.IntrinsicMatrixType)
                        {
                            // float => float1x1
                            scalarPromotions++;
                        }
                    }
                    else if (argumentType.Kind == SymbolKind.IntrinsicVectorType && parameterType.Kind == SymbolKind.IntrinsicMatrixType
                        && ((argumentDimension0 == parameterDimension1 && argumentDimension1 == parameterDimension0)
                        || (argumentDimension1 == parameterDimension0 && argumentDimension0 == parameterDimension1)))
                    {
                        // float1   => float1x3
                        scalarPromotions++;
                    }
                    else if (argumentType.Kind == SymbolKind.IntrinsicMatrixType && parameterType.Kind == SymbolKind.IntrinsicVectorType
                        && ((argumentDimension0 == parameterDimension1 && argumentDimension1 == parameterDimension0)
                        || (argumentDimension1 == parameterDimension0 && argumentDimension0 == parameterDimension1)))
                    {
                        // float1x3 => float1
                        truncations++;
                    }
                    else if (argumentDimension0 == 1 && argumentDimension1 == 1)
                    {
                        // float => float2x4
                        // float => float2
                        scalarPromotions++;
                    }
                    else if ((argumentDimension0 >= parameterDimension0 && argumentDimension1 >= parameterDimension1)
                        || (argumentDimension1 >= parameterDimension0 && argumentDimension0 >= parameterDimension1))
                    {
                        // float4x4 => float3x3
                        // float4   => float3
                        // float1   => float
                        ulong diff = (ulong) (argumentDimension0 + argumentDimension1 - parameterDimension0 - parameterDimension1);
                        if (argumentType.Kind == SymbolKind.IntrinsicMatrixType && parameterType.Kind == SymbolKind.IntrinsicScalarType)
                        {
                            truncations++;
                        }
                        else
                        {
                            truncations++;
                        }
                        
                    }
                }
            }

            const int numBits = 4;
            const ulong mask = (1 << 6) - 1;

            // Worse to better.
            score = (score << numBits) | (truncations & mask);
            score = (score << numBits) | (sameSizeTruncations & mask);
            score = (score << numBits) | (scalarPromotions & mask);
            score = (score << numBits) | (f2iConversions & mask);
            score = (score << numBits) | (f2hConversions & mask);
            score = (score << numBits) | (i2fConversions & mask);
            score = (score << numBits) | (f2dConversions & mask);
            score = (score << numBits) | (intConversions & mask);

            return true;
        }
    }
}