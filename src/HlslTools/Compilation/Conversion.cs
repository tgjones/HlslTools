using System;
using HlslTools.Binding;
using HlslTools.Symbols;

namespace HlslTools.Compilation
{
    public sealed class Conversion
    {
        // Make sure the number of values in ConversionTypes doesn't overflow a ulong.
        internal const int NumConversionBits = 5;

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
                conversionType |= ClassifyTypeConversion(numericSourceType, numericTargetType);
                conversionType |= ClassifyDimensionConversion(numericSourceType, numericTargetType);
            }

            // TODO: Non-numeric implicit conversions.

            return new Conversion(true, false, false, conversionType);
        }

        private static ConversionTypes ClassifyTypeConversion(IntrinsicNumericTypeSymbol numericSourceType, IntrinsicNumericTypeSymbol numericTargetType)
        {
            var parameterScalarType = numericTargetType.ScalarType;
            var argumentScalarType = numericSourceType.ScalarType;

            if (argumentScalarType.IsFloat() && !parameterScalarType.IsFloat())
                return ConversionTypes.FloatToIntConversion;

            if (!argumentScalarType.IsFloat() && parameterScalarType.IsFloat())
                return ConversionTypes.IntToFloatConversion;

            switch (argumentScalarType)
            {
                case ScalarType.Float:
                    switch (parameterScalarType)
                    {
                        case ScalarType.Half:
                            return ConversionTypes.FloatTruncation;
                        case ScalarType.Double:
                            return ConversionTypes.FloatPromotion;
                    }
                    break;
                case ScalarType.Half:
                    switch (parameterScalarType)
                    {
                        case ScalarType.Float:
                        case ScalarType.Double:
                            return ConversionTypes.FloatPromotion;
                    }
                    break;
                case ScalarType.Double:
                    switch (parameterScalarType)
                    {
                        case ScalarType.Half:
                        case ScalarType.Float:
                            return ConversionTypes.FloatTruncation;
                    }
                    break;
                case ScalarType.Bool:
                    if (parameterScalarType != ScalarType.Bool)
                        return ConversionTypes.IntToFloatConversion;
                    break;
                case ScalarType.Int:
                    if (parameterScalarType == ScalarType.Uint)
                        return ConversionTypes.SignedToUnsigned;
                    break;
                case ScalarType.Uint:
                    if (parameterScalarType == ScalarType.Int)
                        return ConversionTypes.UnsignedToSigned;
                    break;
            }

            if (argumentScalarType != parameterScalarType)
                return ConversionTypes.SignedToUnsigned; // TODO: Check this. Int/Uint => Bool?

            return ConversionTypes.None;
        }

        private static ConversionTypes ClassifyDimensionConversion(IntrinsicNumericTypeSymbol sourceType, IntrinsicNumericTypeSymbol targetType)
        {
            var targetDim0 = targetType.GetDimensionSize(0);
            var targetDim1 = targetType.GetDimensionSize(1);
            var sourceDim0 = sourceType.GetDimensionSize(0);
            var sourceDim1 = sourceType.GetDimensionSize(1);

            if ((sourceDim0 == targetDim0 && sourceDim1 == targetDim1) || (sourceDim1 == targetDim0 && sourceDim0 == targetDim1))
            {
                switch (sourceType.Kind)
                {
                    case SymbolKind.IntrinsicMatrixType:
                        switch (targetType.Kind)
                        {
                            case SymbolKind.IntrinsicVectorType: // float1x1 => float1
                            case SymbolKind.IntrinsicScalarType: // float1x1 => float
                                return ConversionTypes.SameSizeTruncation;
                        }
                        break;
                    case SymbolKind.IntrinsicVectorType:
                        switch (targetType.Kind)
                        {
                            case SymbolKind.IntrinsicMatrixType: // float1 => float1x1
                                return ConversionTypes.SameSizePromotion;
                            case SymbolKind.IntrinsicScalarType: // float1 => float
                                return ConversionTypes.SameSizeTruncation;
                        }
                        break;
                    case SymbolKind.IntrinsicScalarType:
                        switch (targetType.Kind)
                        {
                            case SymbolKind.IntrinsicMatrixType: // float => float1x1
                            case SymbolKind.IntrinsicVectorType: // float => float1
                                return ConversionTypes.SameSizePromotion;
                        }
                        break;
                }
            }
            else if ((sourceDim0 == targetDim1 && sourceDim1 == targetDim0) || (sourceDim1 == targetDim0 && sourceDim0 == targetDim1))
            {
                switch (sourceType.Kind)
                {
                    case SymbolKind.IntrinsicMatrixType:
                        switch (targetType.Kind)
                        {
                            case SymbolKind.IntrinsicVectorType: // float1x3 => float1
                                return ConversionTypes.DimensionTruncation;
                        }
                        break;
                    case SymbolKind.IntrinsicVectorType:
                        switch (targetType.Kind)
                        {
                            case SymbolKind.IntrinsicMatrixType: // float1 => float1x3
                                return ConversionTypes.ScalarPromotion;
                        }
                        break;
                }
            }
            else if (sourceDim0 == 1 && sourceDim1 == 1)
            {
                // float => float2x4
                // float => float2
                return ConversionTypes.ScalarPromotion;
            }
            else if ((sourceDim0 >= targetDim0 && sourceDim1 >= targetDim1) || (sourceDim1 >= targetDim0 && sourceDim0 >= targetDim1))
            {
                if (sourceType.Kind == SymbolKind.IntrinsicMatrixType && targetType.Kind == SymbolKind.IntrinsicMatrixType)
                {
                    if (sourceDim0 > targetDim0 && sourceDim1 > targetDim1) // float4x4 => float3x3
                        return ConversionTypes.RankTruncation2;
                    if (sourceDim0 > targetDim0 || sourceDim1 > targetDim1) // float4x4 => float4x3
                        return ConversionTypes.RankTruncation;
                }
                else
                {
                    // float4   => float3
                    // float1   => float
                    return ConversionTypes.RankTruncation2;
                }
            }

            return ConversionTypes.None;
        }
    }
}