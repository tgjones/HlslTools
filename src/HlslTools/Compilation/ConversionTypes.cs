using System;

namespace HlslTools.Compilation
{
    [Flags]
    public enum ConversionTypes : ulong
    {
        None = 0,

        // Better to worse.

        SignedToUnsigned     = (ulong) 1 << (Conversion.NumConversionBits * 0),
        UnsignedToSigned     = (ulong) 1 << (Conversion.NumConversionBits * 1),
        FloatPromotion       = (ulong) 1 << (Conversion.NumConversionBits * 2),
        IntToFloatConversion = (ulong) 1 << (Conversion.NumConversionBits * 3),
        FloatTruncation      = (ulong) 1 << (Conversion.NumConversionBits * 4),
        FloatToIntConversion = (ulong) 1 << (Conversion.NumConversionBits * 5),
        SameSizePromotion    = (ulong) 1 << (Conversion.NumConversionBits * 6),
        SameSizeTruncation   = (ulong) 1 << (Conversion.NumConversionBits * 7),
        ScalarPromotion      = (ulong) 1 << (Conversion.NumConversionBits * 8),
        RankTruncation2      = RankTruncation + 1,
        RankTruncation       = (ulong) 1 << (Conversion.NumConversionBits * 9),
        DimensionTruncation  = (ulong) 1 << (Conversion.NumConversionBits * 10)
    }
}