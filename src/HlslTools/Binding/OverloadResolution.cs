using System;
using System.Collections.Generic;
using System.Linq;
using HlslTools.Binding.Signatures;
using HlslTools.Compilation;
using HlslTools.Symbols;

namespace HlslTools.Binding
{
    internal static class OverloadResolution
    {
        public static OverloadResolutionResult<T> Perform<T>(IEnumerable<T> signatures, params TypeSymbol[] argumentTypes)
            where T : Signature
        {
            return Perform(signatures, (IReadOnlyList<TypeSymbol>) argumentTypes);
        }

        public static OverloadResolutionResult<T> Perform<T>(IEnumerable<T> signatures, IReadOnlyList<TypeSymbol> argumentTypes)
            where T : Signature
        {
            // >= 64 arguments will overflow the bitwise scoring system we use to compare arguments. So just give up...
            if (argumentTypes.Count >= (1 << NumConversionBits))
                return new OverloadResolutionResult<T>(null, null, Enumerable.Empty<OverloadResolutionCandidate<T>>());

            var candidates = new List<OverloadResolutionCandidate<T>>();
            foreach (var signature in signatures)
            {
                Conversion[] conversions;
                ulong score;
                if (!TryRankArguments(signature, argumentTypes, out conversions, out score))
                    continue;

                candidates.Add(new OverloadResolutionCandidate<T>(signature, conversions, score));
            }

            candidates.Sort((l, r) => l.Score.CompareTo(r.Score));

            var selected = candidates.FirstOrDefault();

            var best = (selected != null && (candidates.Count < 2 || candidates[1].Score > selected.Score))
                ? selected
                : null;

            return new OverloadResolutionResult<T>(best, selected, candidates);
        }

        private static bool TryRankArguments<T>(T signature, IReadOnlyList<TypeSymbol> argumentTypes, out Conversion[] conversions, out ulong score)
            where T : Signature
        {
            conversions = new Conversion[signature.ParameterCount];
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
            
            for (var i = 0; i < signature.ParameterCount; i++)
            {
                var parameterType = signature.GetParameterType(i);
                var argumentType = argumentTypes[i];

                if (parameterType.Equals(argumentType))
                {
                    conversions[i] = Conversion.Identity;
                    continue; // i.e. score == 0 for this argument
                }

                var conversionType = ConversionTypes.None;
                conversions[i] = Conversion.ImplicitWidening;

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
                        conversionType |= ConversionTypes.FloatTruncation;
                    }
                    else if (argumentScalarType == ScalarType.Float && parameterScalarType == ScalarType.Double)
                    {
                        conversionType |= ConversionTypes.FloatPromotion;
                    }
                    else if (argumentScalarType == ScalarType.Half && parameterScalarType == ScalarType.Float)
                    {
                        conversionType |= ConversionTypes.FloatPromotion;
                    }
                    else if (argumentScalarType == ScalarType.Half && parameterScalarType == ScalarType.Double)
                    {
                        conversionType |= ConversionTypes.FloatPromotion;
                    }
                    else if (argumentScalarType == ScalarType.Double && parameterScalarType == ScalarType.Half)
                    {
                        conversionType |= ConversionTypes.FloatTruncation;
                    }
                    else if (argumentScalarType == ScalarType.Double && parameterScalarType == ScalarType.Float)
                    {
                        conversionType |= ConversionTypes.FloatTruncation;
                    }
                    else if (argumentScalarType.IsFloat() && !parameterScalarType.IsFloat())
                    {
                        conversionType |= ConversionTypes.FloatToIntConversion;
                    }
                    else if (!argumentScalarType.IsFloat() && parameterScalarType.IsFloat())
                    {
                        conversionType |= ConversionTypes.IntToFloatConversion;
                    }
                    else if (argumentScalarType == ScalarType.Bool && parameterScalarType != ScalarType.Bool)
                    {
                        conversionType |= ConversionTypes.IntToFloatConversion;
                    }
                    else if (argumentScalarType == ScalarType.Int && parameterScalarType == ScalarType.Uint)
                    {
                        conversionType |= ConversionTypes.SignedToUnsigned;
                    }
                    else if (argumentScalarType == ScalarType.Uint && parameterScalarType == ScalarType.Int)
                    {
                        conversionType |= ConversionTypes.UnsignedToSigned;
                    }
                    else if (argumentScalarType != parameterScalarType)
                    {
                        // TODO: Need this, or can throw exception if we get here?
                        conversionType |= ConversionTypes.SignedToUnsigned;
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
                            conversionType |= ConversionTypes.SameSizeTruncation;
                        }
                        if (argumentType.Kind == SymbolKind.IntrinsicMatrixType && parameterType.Kind == SymbolKind.IntrinsicScalarType)
                        {
                            // float1x1 => float
                            conversionType |= ConversionTypes.SameSizeTruncation;
                        }
                        else if ((argumentType.Kind == SymbolKind.IntrinsicScalarType && parameterType.Kind == SymbolKind.IntrinsicVectorType)
                            || (argumentType.Kind == SymbolKind.IntrinsicVectorType && parameterType.Kind == SymbolKind.IntrinsicMatrixType))
                        {
                            // float  => float1
                            // float1 => float1x1
                            conversionType |= ConversionTypes.ScalarPromotion;
                        }
                        else if (argumentType.Kind == SymbolKind.IntrinsicScalarType && parameterType.Kind == SymbolKind.IntrinsicMatrixType)
                        {
                            // float => float1x1
                            conversionType |= ConversionTypes.ScalarPromotion;
                        }
                    }
                    else if (argumentType.Kind == SymbolKind.IntrinsicVectorType && parameterType.Kind == SymbolKind.IntrinsicMatrixType
                        && ((argumentDimension0 == parameterDimension1 && argumentDimension1 == parameterDimension0)
                            || (argumentDimension1 == parameterDimension0 && argumentDimension0 == parameterDimension1)))
                    {
                        // float1   => float1x3
                        conversionType |= ConversionTypes.ScalarPromotion;
                    }
                    else if (argumentType.Kind == SymbolKind.IntrinsicMatrixType && parameterType.Kind == SymbolKind.IntrinsicVectorType
                        && ((argumentDimension0 == parameterDimension1 && argumentDimension1 == parameterDimension0)
                            || (argumentDimension1 == parameterDimension0 && argumentDimension0 == parameterDimension1)))
                    {
                        // float1x3 => float1
                        conversions[i] = Conversion.ImplicitNarrowing;
                        conversionType |= ConversionTypes.DimensionTruncation;
                    }
                    else if (argumentDimension0 == 1 && argumentDimension1 == 1)
                    {
                        // float => float2x4
                        // float => float2
                        conversionType |= ConversionTypes.ScalarPromotion;
                    }
                    else if ((argumentDimension0 >= parameterDimension0 && argumentDimension1 >= parameterDimension1)
                        || (argumentDimension1 >= parameterDimension0 && argumentDimension0 >= parameterDimension1))
                    {
                        conversions[i] = Conversion.ImplicitNarrowing;
                        if (argumentType.Kind == SymbolKind.IntrinsicMatrixType && parameterType.Kind == SymbolKind.IntrinsicMatrixType)
                        {
                            // float4x4 => float3x3
                            if (argumentDimension0 > parameterDimension0 && argumentDimension1 > parameterDimension1)
                            {
                                conversionType |= ConversionTypes.RankTruncation2;
                            }
                            else if (argumentDimension0 > parameterDimension0 || argumentDimension1 > parameterDimension1)
                            {
                                conversionType |= ConversionTypes.RankTruncation;
                            }
                        }
                        else
                        {
                            // float4   => float3
                            // float1   => float
                            conversionType |= ConversionTypes.RankTruncation2;
                        }
                    }
                }

                score += (ulong) conversionType;
            }

            return true;
        }

        // Make sure the number of values in ConversionTypes doesn't overflow a ulong.
        private const int NumConversionBits = 6;

        [Flags]
        private enum ConversionTypes : ulong
        {
            None = 0,

            // Worse to better.

            SignedToUnsigned     = (ulong) 1 << (NumConversionBits * 0),
            UnsignedToSigned     = (ulong) 1 << (NumConversionBits * 1),
            FloatPromotion       = (ulong) 1 << (NumConversionBits * 2),
            IntToFloatConversion = (ulong) 1 << (NumConversionBits * 3),
            FloatTruncation      = (ulong) 1 << (NumConversionBits * 4),
            FloatToIntConversion = (ulong) 1 << (NumConversionBits * 5),
            ScalarPromotion      = (ulong) 1 << (NumConversionBits * 6),
            SameSizeTruncation   = (ulong) 1 << (NumConversionBits * 7),
            RankTruncation2      = RankTruncation + 1,
            RankTruncation       = (ulong) 1 << (NumConversionBits * 8),
            DimensionTruncation  = (ulong) 1 << (NumConversionBits * 9)
        }
    }
}