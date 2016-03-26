using System;
using HlslTools.Binding;
using HlslTools.Symbols;

namespace HlslTools.Compilation
{
    public sealed class Conversion
    {
        // Make sure the number of values in ConversionTypes doesn't overflow a ulong.
        internal const int NumConversionBits = 6;

        private Conversion(bool exists, bool isIdentity, bool isExplicit, ConversionTypes implicitConversionType)
        {
            Exists = exists;
            IsIdentity = isIdentity;
            IsExplicit = isExplicit;
            ImplicitConversionType = implicitConversionType;
        }

        internal static readonly Conversion None = new Conversion(false, false, false, ConversionTypes.None);
        internal static readonly Conversion Identity = new Conversion(true, true, false, ConversionTypes.None);
        internal static readonly Conversion Explicit = new Conversion(true, false, true, ConversionTypes.None);

        public bool Exists { get; }

        public bool IsIdentity { get; }
        public ConversionTypes ImplicitConversionType { get; }

        public bool IsImplicit => !IsIdentity && !IsExplicit;

        public bool IsExplicit { get; }

        internal static Conversion Classify(TypeSymbol sourceType, TypeSymbol targetType, ParameterDirection direction)
        {
            if (sourceType.Equals(targetType))
                return Identity;

            // First, make sure we have a conversion from argument to parameter.
            switch (direction)
            {
                case ParameterDirection.In:
                    if (!sourceType.HasImplicitConversionTo(targetType))
                        return sourceType.HasExplicitConversionTo(targetType) ? Explicit : None;
                    break;
                case ParameterDirection.Out:
                    if (!targetType.HasImplicitConversionTo(sourceType))
                        return None;
                    break;
                case ParameterDirection.Inout:
                    if (!sourceType.HasImplicitConversionTo(targetType) || !targetType.HasImplicitConversionTo(sourceType))
                        return None;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var conversionType = ConversionTypes.None;

            var numericTargetType = targetType as IntrinsicNumericTypeSymbol;
            var numericSourceType = sourceType as IntrinsicNumericTypeSymbol;
            if (numericTargetType != null && numericSourceType != null)
            {
                var parameterScalarType = numericTargetType.ScalarType;
                var argumentScalarType = numericSourceType.ScalarType;

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

                var parameterDimension0 = numericTargetType.GetDimensionSize(0);
                var parameterDimension1 = numericTargetType.GetDimensionSize(1);
                var argumentDimension0 = numericSourceType.GetDimensionSize(0);
                var argumentDimension1 = numericSourceType.GetDimensionSize(1);

                if ((argumentDimension0 == parameterDimension0 && argumentDimension1 == parameterDimension1)
                    || argumentDimension1 == parameterDimension0 && argumentDimension0 == parameterDimension1)
                {
                    if ((sourceType.Kind == SymbolKind.IntrinsicMatrixType && targetType.Kind == SymbolKind.IntrinsicVectorType)
                        || (sourceType.Kind == SymbolKind.IntrinsicVectorType && targetType.Kind == SymbolKind.IntrinsicScalarType))
                    {
                        // float1x1 => float1
                        // float1   => float
                        conversionType |= ConversionTypes.SameSizeTruncation;
                    }
                    if (sourceType.Kind == SymbolKind.IntrinsicMatrixType && targetType.Kind == SymbolKind.IntrinsicScalarType)
                    {
                        // float1x1 => float
                        conversionType |= ConversionTypes.SameSizeTruncation;
                    }
                    else if ((sourceType.Kind == SymbolKind.IntrinsicScalarType && targetType.Kind == SymbolKind.IntrinsicVectorType)
                        || (sourceType.Kind == SymbolKind.IntrinsicVectorType && targetType.Kind == SymbolKind.IntrinsicMatrixType))
                    {
                        // float  => float1
                        // float1 => float1x1
                        conversionType |= ConversionTypes.ScalarPromotion;
                    }
                    else if (sourceType.Kind == SymbolKind.IntrinsicScalarType && targetType.Kind == SymbolKind.IntrinsicMatrixType)
                    {
                        // float => float1x1
                        conversionType |= ConversionTypes.ScalarPromotion;
                    }
                }
                else if (sourceType.Kind == SymbolKind.IntrinsicVectorType && targetType.Kind == SymbolKind.IntrinsicMatrixType
                    && ((argumentDimension0 == parameterDimension1 && argumentDimension1 == parameterDimension0)
                        || (argumentDimension1 == parameterDimension0 && argumentDimension0 == parameterDimension1)))
                {
                    // float1   => float1x3
                    conversionType |= ConversionTypes.ScalarPromotion;
                }
                else if (sourceType.Kind == SymbolKind.IntrinsicMatrixType && targetType.Kind == SymbolKind.IntrinsicVectorType
                    && ((argumentDimension0 == parameterDimension1 && argumentDimension1 == parameterDimension0)
                        || (argumentDimension1 == parameterDimension0 && argumentDimension0 == parameterDimension1)))
                {
                    // float1x3 => float1
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
                    if (sourceType.Kind == SymbolKind.IntrinsicMatrixType && targetType.Kind == SymbolKind.IntrinsicMatrixType)
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

            return new Conversion(true, false, false, conversionType);
        }
    }
}