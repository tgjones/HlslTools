using System;
using ShaderTools.CodeAnalysis.Hlsl.Binding;
using ShaderTools.CodeAnalysis.Hlsl.Symbols;
using ShaderTools.CodeAnalysis.Symbols;

namespace ShaderTools.CodeAnalysis.Hlsl.Compilation
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
                        case ScalarType.Min10Float:
                        case ScalarType.Min16Float:
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
                        case ScalarType.Min16Float:
                            return ConversionTypes.FloatPromotion;
                        case ScalarType.Min10Float:
                            return ConversionTypes.FloatTruncation;
                    }
                    break;
                case ScalarType.Double:
                    switch (parameterScalarType)
                    {
                        case ScalarType.Half:
                        case ScalarType.Float:
                        case ScalarType.Min16Float:
                        case ScalarType.Min10Float:
                            return ConversionTypes.FloatTruncation;
                    }
                    break;
                case ScalarType.Min16Float:
                    switch (parameterScalarType)
                    {
                        case ScalarType.Float:
                        case ScalarType.Double:
                            return ConversionTypes.FloatPromotion;
                        case ScalarType.Half:
                        case ScalarType.Min10Float:
                            return ConversionTypes.FloatTruncation;
                    }
                    break;
                case ScalarType.Min10Float:
                    switch (parameterScalarType)
                    {
                        case ScalarType.Half:
                        case ScalarType.Float:
                        case ScalarType.Double:
                        case ScalarType.Min16Float:
                            return ConversionTypes.FloatPromotion;
                    }
                    break;

                case ScalarType.Bool:
                    if (parameterScalarType != ScalarType.Bool)
                        return ConversionTypes.IntToFloatConversion;
                    break;
                case ScalarType.Int:
                case ScalarType.Int64_t:
                case ScalarType.Min16Int:
                case ScalarType.Min12Int:
                    switch (parameterScalarType)
                    {
                        case ScalarType.Uint:
                        case ScalarType.Uint64_t:
                        case ScalarType.Min16Uint:
                            return ConversionTypes.SignedToUnsigned;
                    }
                    break;
                case ScalarType.Uint:
                case ScalarType.Uint64_t:
                case ScalarType.Min16Uint:
                    switch (parameterScalarType)
                    {
                        case ScalarType.Int:
						case ScalarType.Int64_t:
						case ScalarType.Min16Int:
                        case ScalarType.Min12Int:
                            return ConversionTypes.UnsignedToSigned;
                    }
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
                            case SymbolKind.IntrinsicVectorType: // float3x1 => float1
                                return ConversionTypes.DimensionTruncation;
                        }
                        break;
                    case SymbolKind.IntrinsicVectorType:
                        switch (targetType.Kind)
                        {
                            case SymbolKind.IntrinsicMatrixType: // float3 => float3x1
                                return ConversionTypes.ScalarPromotion;
                        }
                        break;
                }
            }
            else if (sourceDim0 == 1 && sourceDim1 == 1)
            {
                // float => float2x4
                // float => float2
                // float1 => float2
                return ConversionTypes.ScalarPromotion;
            }
            else if ((sourceDim0 >= targetDim0 && sourceDim1 >= targetDim1) || (sourceDim1 >= targetDim0 && sourceDim0 >= targetDim1))
            {
                switch (sourceType.Kind)
                {
                    case SymbolKind.IntrinsicMatrixType:
                        switch (targetType.Kind)
                        {
                            case SymbolKind.IntrinsicMatrixType:
                                if (sourceDim0 > targetDim0 && sourceDim1 > targetDim1) // float4x4 => float3x3
                                    return ConversionTypes.RankTruncation2;
                                if (sourceDim0 > targetDim0 || sourceDim1 > targetDim1) // float4x4 => float4x3
                                    return ConversionTypes.RankTruncation;
                                return ConversionTypes.None;
                            case SymbolKind.IntrinsicVectorType: // float4x4 => float3
                                return ConversionTypes.RankTruncation2;
                            case SymbolKind.IntrinsicScalarType: // float3x4 => float
                                return ConversionTypes.RankTruncation2;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    case SymbolKind.IntrinsicVectorType:
                        switch (targetType.Kind)
                        {
                            case SymbolKind.IntrinsicMatrixType: // float4 => float4x3
                                return ConversionTypes.RankTruncation;
                            case SymbolKind.IntrinsicVectorType: // float4 => float3
                                return ConversionTypes.RankTruncation2;
                            case SymbolKind.IntrinsicScalarType: // float4 => float
                                return ConversionTypes.RankTruncation2;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return ConversionTypes.None;
        }
    }
}