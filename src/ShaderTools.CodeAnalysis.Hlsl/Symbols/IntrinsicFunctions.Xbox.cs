using System.Collections.Generic;
using System.Linq;

namespace ShaderTools.CodeAnalysis.Hlsl.Symbols
{
    internal static partial class IntrinsicFunctions
    {
        public static readonly IEnumerable<FunctionSymbol> AllXboxFunctions = CreateXboxFunctions();

        private static IEnumerable<FunctionSymbol> CreateXboxFunctions()
        {
            var allFunctions = new List<FunctionSymbol>();

            allFunctions.AddRange(Create2(
                "__XB_PackF32ToSNORM16",
                "Converts a pair of 32-bit floats to a 32-bit packed pair of 16-bit signed, normalized values. Translates to GCN's v_cvt_pknorm_i16_f32.",
                IntrinsicTypes.AllFloatVectorTypes,
                "value1", "First value to convert.",
                "value2", "Second value to pack.",
                overrideReturnTypes: IntrinsicTypes.AllUintVectorTypes));

            allFunctions.AddRange(Create2(
                "__XB_PackF32ToUNORM16",
                "Converts a pair of 32-bit floats to a 32-bit packed pair of 16-bit unsigned, normalized values. Translates to GCN's v_cvt_pknorm_u16_f32.",
                IntrinsicTypes.AllFloatVectorTypes,
                "value1", "First value to convert.",
                "value2", "Second value to pack.",
                overrideReturnTypes: IntrinsicTypes.AllUintVectorTypes));

            allFunctions.AddRange(Create2(
                "__XB_PackI32ToI16",
                "Converts a pair of 32-bit signed integers to a 32-bit packed pair of 16-bit signed integers. Translates to GCN's v_cvt_pk_i16_i32.",
                IntrinsicTypes.AllIntVectorTypes,
                "value1", "First value to convert.",
                "value2", "Second value to pack.",
                overrideReturnTypes: IntrinsicTypes.AllUintVectorTypes));

            allFunctions.AddRange(Create2(
                "__XB_PackU32ToU16",
                "Converts a pair of 32-bit unsigned integers to a 32-bit packed pair of 16-bit unsigned integers. Translates to GCN's v_cvt_pk_u16_u32.",
                IntrinsicTypes.AllUintVectorTypes,
                "value1", "First value to convert.",
                "value2", "Second value to pack.",
                overrideReturnTypes: IntrinsicTypes.AllUintVectorTypes));

            allFunctions.Add(Create0(
                "__XB_MemTime",
                "Returns the current 64-bit timestamp on the GPU. Translate's to GCN's s_memtime.",
                IntrinsicTypes.Uint2));

            allFunctions.AddRange(Create1(
                "__XB_GetShaderUserData",
                "Get the value of the shader user data register 'index'. For further information see XDK documentation.",
                new[] { IntrinsicTypes.Uint },
                "index", "Index of the user data register to get."));

            allFunctions.AddRange(Create1(
                "__XB_UnpackByte0",
                "Extracts the lowest byte from a uint and converts the value to a float. Translates to GCN's v_cvt_f32_ubyte0.",
                IntrinsicTypes.AllUintVectorTypes,
                "value", "Value to unpack from.",
                IntrinsicTypes.AllFloatVectorTypes));

            allFunctions.AddRange(Create1(
                "__XB_UnpackByte1",
                "Extracts the second byte from a uint and converts the value to a float. Translates to GCN's v_cvt_f32_ubyte1.",
                IntrinsicTypes.AllUintVectorTypes,
                "value", "Value to unpack from.",
                IntrinsicTypes.AllFloatVectorTypes));

            allFunctions.AddRange(Create1(
                "__XB_UnpackByte2",
                "Extracts the third byte from a uint and converts the value to a float. Translates to GCN's v_cvt_f32_ubyte2.",
                IntrinsicTypes.AllUintVectorTypes,
                "value", "Value to unpack from.",
                IntrinsicTypes.AllFloatVectorTypes));

            allFunctions.AddRange(Create1(
                "__XB_UnpackByte3",
                "Extracts the highest byte from a uint and converts the value to a float. Translates to GCN's v_cvt_f32_ubyte3.",
                IntrinsicTypes.AllUintVectorTypes,
                "value", "Value to unpack from.",
                IntrinsicTypes.AllFloatVectorTypes));

            allFunctions.AddRange(Create3(
                "__XB_PackF32ToU8",
                "Copies 'src' to result then converts 'insertValue' to an 8-bit unsigned integer and inserts it into byte 'index' of result. Translates to GCN's v_cvt_pk_u8_f32.",
                IntrinsicTypes.AllUintVectorTypes,
                "insertValue", "Value to insert into result.",
                "index", "Index of byte to insert into.",
                "src", "Source values to insert into.",
                overrideParameterTypes1: IntrinsicTypes.AllFloatVectorTypes));

            allFunctions.AddRange(Create1(
                "f32tof16nearest",
                "Converts a 32-bit float to a 16-bit float using round-to-nearest rounding mode - f32tof16 uses round-to-zero. Translates to GCN's v_cvt_f16_f32.",
                IntrinsicTypes.AllFloatVectorTypes,
                "value", "Value to convert.",
                overrideReturnTypes: IntrinsicTypes.AllUintVectorTypes));

            allFunctions.AddRange(Create3(
                "__XB_IBFE",
                "Signed bitfield extract. Translates to GCN's v_bfe_i32",
                new[] { IntrinsicTypes.Uint },
                "width", "Number of bits to extract.",
                "offset", "Offset in bits from which to extract.",
                "value", "Value from which to extract."));

            allFunctions.AddRange(Create3(
                "__XB_UBFE",
                "Unsigned bitfield extract. Translates to GCN's v_bfe_u32",
                new[] { IntrinsicTypes.Uint },
                "width", "Number of bits to extract.",
                "offset", "Offset in bits from which to extract.",
                "value", "Value from which to extract."));

            allFunctions.AddRange(Create2(
                "__XB_BFM",
                "Generate a bitfield mask for use with __XB_BFI. Translates to GCN's v_bfm_b32.",
                IntrinsicTypes.AllUintVectorTypes,
                "maskWidth", "Width of the mask in bits.",
                "maskOffset", "Offset in bits from which to start the mask."));

            allFunctions.AddRange(Create3(
                "__XB_BFI",
                "Bitfield insert. Replaces bits in one value with bits from another according to a mask. Translates to GCN's v_bfi_b32.",
                new[] { IntrinsicTypes.Uint },
                "mask", "Bitfield mask used to select bits.",
                "preserve", "Bits to keep where 'mask' is binary 1.",
                "enable", "Bits to insert where 'mask' is 0."));

            allFunctions.AddRange(Create1(
                "__XB_F32ToI32RPI",
                "Converts a 32-bit float to a 32-bit signed integer using round-to-positive-infinity tiebreaker for 0.5. Translates to GCN's v_cvt_rpi_i32_f32.",
                IntrinsicTypes.AllFloatVectorTypes,
                "value", "Value to convert.",
                overrideReturnTypes: IntrinsicTypes.AllIntVectorTypes));

            allFunctions.AddRange(Create1(
                "__XB_F32ToI32FLR",
                "Converts a 32-bit float to a 32-bit signed integer using a floor operation. Translates to GCN's v_cvt_flr_i32_f32.",
                IntrinsicTypes.AllFloatVectorTypes,
                "value", "Value to convert.",
                overrideReturnTypes: IntrinsicTypes.AllIntVectorTypes));

            allFunctions.AddRange(Create1(
                "__XB_I4ToF32",
                "Converts a 32-bit float to a 4-bit signed integer. Used for interpolation in a shader. Translates to GCN's v_cvt_off_f32_i4.",
                IntrinsicTypes.AllIntVectorTypes,
                "value", "Value to convert.",
                overrideReturnTypes: IntrinsicTypes.AllFloatVectorTypes));

            allFunctions.AddRange(Create3(
                "__XB_Cube_ID",
                "Performs cubemap face ID determination. Translates to GCN's v_cubeid_f32.",
                new[] { IntrinsicTypes.Float },
                "x", "X component of the 3D vector.",
                "y", "Y component of the 3D vector.",
                "z", "Z component of the 3D vector."));

            allFunctions.AddRange(Create3(
                "__XB_Cube_MA",
                "Performs cubemap major axis determination. Translates to GCN's v_cubema_f32.",
                new[] { IntrinsicTypes.Float },
                "x", "X component of the 3D vector.",
                "y", "Y component of the 3D vector.",
                "z", "Z component of the 3D vector."));

            allFunctions.AddRange(Create3(
                "__XB_Cube_SC",
                "Performs cubemap S coordinate determination. Translates to GCN's v_cubesc_f32.",
                new[] { IntrinsicTypes.Float },
                "x", "X component of the 3D vector.",
                "y", "Y component of the 3D vector.",
                "z", "Z component of the 3D vector."));

            allFunctions.AddRange(Create3(
                "__XB_Cube_TC",
                "Performs cubemap T coordinate determination. Translates to GCN's v_cubetc_f32.",
                new[] { IntrinsicTypes.Float },
                "x", "X component of the 3D vector.",
                "y", "Y component of the 3D vector.",
                "z", "Z component of the 3D vector."));

            allFunctions.AddRange(Create3(
                "__XB_Min3_F32",
                "Calculates the minimum of three 32-bit floats. Translates to GCN's v_min3_f32.",
                new[] { IntrinsicTypes.Float },
                "x", "First value.",
                "y", "Second value.",
                "z", "Third value."));

            allFunctions.AddRange(Create3(
               "__XB_Min3_I32",
               "Calculates the minimum of three 32-bit signed integers. Translates to GCN's v_min3_i32.",
               new[] { IntrinsicTypes.Int },
               "x", "First value.",
               "y", "Second value.",
               "z", "Third value."));

            allFunctions.AddRange(Create3(
               "__XB_Min3_U32",
               "Calculates the minimum of three 32-bit unsigned integers. Translates to GCN's v_min3_u32.",
               new[] { IntrinsicTypes.Uint },
               "x", "First value.",
               "y", "Second value.",
               "z", "Third value."));

            allFunctions.AddRange(Create3(
               "__XB_Max3_F32",
               "Calculates the maximum of three 32-bit floats. Translates to GCN's v_max3_f32.",
               new[] { IntrinsicTypes.Float },
               "x", "First value.",
               "y", "Second value.",
               "z", "Third value."));

            allFunctions.AddRange(Create3(
                "__XB_Max3_I32",
                "Calculates the maximum of three 32-bit signed integers. Translates to GCN's v_max3_i32.",
                new[] { IntrinsicTypes.Int },
                "x", "First value.",
                "y", "Second value.",
                "z", "Third value."));

            allFunctions.AddRange(Create3(
                "__XB_Max3_U32",
                "Calculates the maximum of three 32-bit unsigned integers. Translates to GCN's v_max3_u32.",
                new[] { IntrinsicTypes.Uint },
                "x", "First value.",
                "y", "Second value.",
                "z", "Third value."));

            allFunctions.AddRange(Create3(
                "__XB_Med3_F32",
                "Calculates the median of three 32-bit floats. Translates to GCN's v_med3_f32.",
                new[] { IntrinsicTypes.Float },
                "x", "First value.",
                "y", "Second value.",
                "z", "Third value."));

            allFunctions.AddRange(Create3(
                "__XB_Med3_I32",
                "Calculates the median of three 32-bit signed integers. Translates to GCN's v_med3_i32.",
                new[] { IntrinsicTypes.Int },
                "x", "First value.",
                "y", "Second value.",
                "z", "Third value."));

            allFunctions.AddRange(Create3(
                "__XB_Med3_U32",
                "Calculates the median of three 32-bit unsigned integers. Translates to GCN's v_med3_u32.",
                new[] { IntrinsicTypes.Uint },
                "x", "First value.",
                "y", "Second value.",
                "z", "Third value."));

            allFunctions.AddRange(Create2(
                "__XB_MulI24",
                "Performs a 24-bit signed integer multiply. Returns the lower 32 bits of the 48-bit result. Translates to GCN's v_mul_i32_i24.",
                new[] { IntrinsicTypes.Int },
                "x", "First value.",
                "y", "Second value."));

            allFunctions.AddRange(Create2(
                "__XB_MulI24Hi",
                "Performs a 24-bit signed integer multiply. Returns the high-order 16 bits of the 48-bit result. Translates to GCN's v_mul_hi_i32_i24.",
                new[] { IntrinsicTypes.Int },
                "x", "First value.",
                "y", "Second value."));

            allFunctions.AddRange(Create2(
                "__XB_MulU24",
                "Performs a 24-bit unsigned integer multiply. Returns the lower 32 bits of the 48-bit result. Translates to GCN's v_mul_u32_u24.",
                new[] { IntrinsicTypes.Uint },
                "x", "First value.",
                "y", "Second value."));

            allFunctions.AddRange(Create2(
                "__XB_MulU24Hi",
                "Performs a 24-bit unsigned integer multiply. Returns the high-order 16 bits of the 48-bit result. Translates to GCN's v_mul_hi_u32_u24.",
                new[] { IntrinsicTypes.Uint },
                "x", "First value.",
                "y", "Second value."));

            allFunctions.AddRange(Create3(
                "__XB_MadI24",
                "Performs a 24-bit signed integer multiply add. Addend is 32 bits. Returns the low-order sign extended 32 bits of the result. Translates to GCN's v_mad_i32_i24.",
                new[] { IntrinsicTypes.Int },
                "a", "First value to multiply.",
                "b", "Second value to multiply.",
                "c", "Addend."));

            allFunctions.AddRange(Create3(
                "__XB_MadU24",
                "Performs a 24-bit unsigned integer multiply add. Addend is 32 bits. Returns the low-order 32 bits of the result. Translates to GCN's v_mad_u32_u24.",
                new[] { IntrinsicTypes.Uint },
                "a", "First value to multiply.",
                "b", "Second value to multiply.",
                "c", "Addend."));

            allFunctions.AddRange(Create1(
                "__XB_NegI64",
                "Performs a 64-bit signed integer negation. Does not translate to a specific GCN instruction.",
                new[] { IntrinsicTypes.Uint2 },
                "value", "Value to negate."));

            allFunctions.AddRange(Create2(
                "__XB_ShrI64",
                "Performs an arithmetic shift-right on a 64-bit signed integer. Translates to GCN's v_ashr_i64.",
                new[] { IntrinsicTypes.Uint2 },
                "value", "Value to shift right.",
                "bits", "The number of bits to shift right.",
                overrideParameterType2: IntrinsicTypes.Uint));

            allFunctions.AddRange(Create2(
                "__XB_ShlU64",
                "Performs a logical shift-left on a 64-bit unsigned integer. Translates to GCN's v_lshl_b64.",
                new[] { IntrinsicTypes.Uint2 },
                "value", "Value to shift left.",
                "bits", "The number of bits to shift left.",
                overrideParameterType2: IntrinsicTypes.Uint));

            allFunctions.AddRange(Create2(
                "__XB_ShrU64",
                "Performs a logical shift-right on a 64-bit unsigned integer. Translates to GCN's v_lshr_b64.",
                new[] { IntrinsicTypes.Uint2 },
                "value", "Value to shift right.",
                "bits", "The number of bits to shift right.",
                overrideParameterType2: IntrinsicTypes.Uint));

            allFunctions.AddRange(Create2(
                "__XB_AddI64",
                "Calculates the sum of two 64-bit signed integers. Does not translate to a specific GCN instruction.",
                new[] { IntrinsicTypes.Uint2 },
                "x", "First value to add.",
                "y", "Second value to add."));

            allFunctions.AddRange(Create2(
                "__XB_MulI64",
                "Calculates the product of two 64-bit signed integers. Does not translate to a specific GCN instruction.",
                new[] { IntrinsicTypes.Uint2 },
                "x", "First value to multiply.",
                "y", "Second value to multiply."));

            allFunctions.AddRange(Create2(
                "__XB_MulU64",
                "Calculates the product of two 64-bit unsigned integers. Does not translate to a specific GCN instruction.",
                new[] { IntrinsicTypes.Uint2 },
                "x", "First value to multiply.",
                "y", "Second value to multiply."));

            allFunctions.AddRange(Create2(
                "__XB_SubI64",
                "Calculates a subtraction of two 64-bit signed integers. Does not translate to a specific GCN instruction.",
                new[] { IntrinsicTypes.Uint2 },
                "x", "Minuend.",
                "y", "Subtrahend."));

            allFunctions.AddRange(Create2(
                "__XB_DivI64",
                "Calculates the division of two 64-bit signed integers. If 'denominator' is 0, result is 0. Does not translate to a specific GCN instruction.",
                new[] { IntrinsicTypes.Uint2 },
                "x", "Numerator.",
                "y", "Denominator."));

            allFunctions.AddRange(Create2(
                "__XB_ModI64",
                "Calculates the modulus of two 64-bit signed integers. If 'divisor' is 0, result is 0. Does not translate to a specific GCN instruction.",
                new[] { IntrinsicTypes.Uint2 },
                "x", "Dividend.",
                "y", "Divisor."));

            allFunctions.AddRange(Create2(
                "__XB_DivU64",
                "Calculates the division of two 64-bit unsigned integers. Does not translate to a specific GCN instruction.",
                new[] { IntrinsicTypes.Uint2 },
                "x", "Numerator.",
                "y", "Denominator."));

            allFunctions.AddRange(Create2(
                "__XB_ModU64",
                "Calculates the modulus of two 64-bit unsigned integers. Does not translate to a specific GCN instruction.",
                new[] { IntrinsicTypes.Uint2 },
                "x", "Dividend.",
                "y", "Divisor."));

            allFunctions.AddRange(Create2(
                "__XB_MaxI64",
                "Calculate the maximum of two 64-bit signed integers. Does not translate to a specific GCN instruction.",
                new[] { IntrinsicTypes.Uint2 },
                "x", "First value.",
                "y", "Second value."));

            allFunctions.AddRange(Create2(
                "__XB_MaxU64",
                "Calculate the maximum of two 64-bit unsigned integers. Does not translate to a specific GCN instruction.",
                new[] { IntrinsicTypes.Uint2 },
                "x", "First value.",
                "y", "Second value."));

            allFunctions.AddRange(Create2(
                "__XB_MinI64",
                "Calculate the minimum of two 64-bit signed integers. Does not translate to a specific GCN instruction.",
                new[] { IntrinsicTypes.Uint2 },
                "x", "First value.",
                "y", "Second value."));

            allFunctions.AddRange(Create2(
                "__XB_MinU64",
                "Calculate the minimum of two 64-bit unsigned integers. Does not translate to a specific GCN instruction.",
                new[] { IntrinsicTypes.Uint2 },
                "x", "First value.",
                "y", "Second value."));

            allFunctions.AddRange(Create1(
                "__XB_I64ToD",
                "Converts a 64-bit signed integer to a 64-bit double. Does not translate to a specific GCN instruction.",
                new[] { IntrinsicTypes.Uint2 },
                "value", "Value to convert.",
                overrideReturnTypes: new[] { IntrinsicTypes.Double }));

            allFunctions.AddRange(Create1(
                "__XB_U64ToD",
                "Converts a 64-bit unsigned integer to a 64-bit double. Does not translate to a specific GCN instruction.",
                new[] { IntrinsicTypes.Uint2 },
                "value", "Value to convert.",
                overrideReturnTypes: new[] { IntrinsicTypes.Double }));

            allFunctions.AddRange(Create1(
                "__XB_DToI64",
                "Converts a 64-bit double to a 64-bit signed integer. Does not translate to a specific GCN instruction.",
                new[] { IntrinsicTypes.Double },
                "value", "Value to convert.",
                overrideReturnTypes: new[] { IntrinsicTypes.Uint2 }));

            allFunctions.AddRange(Create1(
                "__XB_DToU64",
                "Converts a 64-bit double to a 64-bit unsigned integer. Does not translate to a specific GCN instruction.",
                new[] { IntrinsicTypes.Double },
                "value", "Value to convert.",
                overrideReturnTypes: new[] { IntrinsicTypes.Uint2 }));

            allFunctions.AddRange(Create1(
                "__XB_Log_Clamp",
                "Calculates the base2 logarithm of a 32-bit float. Clamps infinities to +-FLT_MAX. Translates to GCN's v_log_clamp_f32.",
                new[] { IntrinsicTypes.Float },
                "value", "The value."));

            allFunctions.AddRange(Create1(
                "__XB_Rcp_Clamp",
                "Calculates the reciprocal of a 32-bit float. Clamps infinities to +- FLT_MAX. Translates to GCN's v_rcp_clamp_f32.",
                new[] { IntrinsicTypes.Float },
                "value", "The value."));

            allFunctions.AddRange(Create1(
                "__XB_Rsq_Clamp",
                "Calculates the reciprocal square-root of a 32-bit float. Clamps infinities to +-FLT_MAX. Translates to GCN's v_rsq_clamp_f32.",
                new[] { IntrinsicTypes.Float },
                "value", "The value."));

            allFunctions.AddRange(Create1(
                "__XB_Ballot64",
                "Returns a 64-bit unsigned integer bitmask of the evaluation of the Boolean expression for all 64 lanes in the wavefront. True = 1, False = 0.",
                new[] { IntrinsicTypes.Bool },
                "condition", "Condition to test.",
                overrideReturnTypes: new[] { IntrinsicTypes.Uint2 }));

            allFunctions.AddRange(Create1(
                "__XB_MakeUniform",
                "Returns the value of the expression for the active lane of the current wave with the smallest index. Translates to GCN's v_readfirstlane_b32.",
                IntrinsicTypes.AllFloatVectorTypes.Union(IntrinsicTypes.AllDoubleVectorTypes).Union(IntrinsicTypes.AllIntVectorTypes).Union(IntrinsicTypes.AllUintVectorTypes).ToArray(),
                "value", "The value."));

            allFunctions.AddRange(Create2(
                "__XB_ReadLane",
                "Returns the value of the expression for the given lane index within the wave. Lane index must be uniform across the wave. Translates to GCN's v_readlane_b32.",
                new[] { IntrinsicTypes.Float, IntrinsicTypes.Int, IntrinsicTypes.Uint },
                "value", "Value to read from.",
                "laneIndex", "The lane index from which to read.",
                overrideParameterType2: IntrinsicTypes.Uint));

            allFunctions.AddRange(Create3(
                "__XB_WriteLane",
                "Writes a value to the specified lane in the wave. 'newValue' and 'laneIndex' must be uniform across the wave. Translates to GCN's v_writelane_b32.",
                new[] { IntrinsicTypes.Float, IntrinsicTypes.Int, IntrinsicTypes.Uint },
                "newValue", "Replacement value for specified lane.",
                "laneIndex", "The lane index to which to write.",
                "oldValue", "Value for non-written-to lanes.",
                overrideParameterTypes2: new[] { IntrinsicTypes.Uint }));

            allFunctions.AddRange(Create2(
                "__XB_LaneSwizzle",
                "Performs a wave-wide swizzle of input thread data based according to an offset mask. 'offsetMask' must be a literal value. Translates to GCN's ds_swizzle_b32.",
                new[] { IntrinsicTypes.Float, IntrinsicTypes.Int, IntrinsicTypes.Uint },
                "value", "Value to swizzle across threads.",
                "offsetMask", "See AMD GCN ISA for bit-layout.",
                overrideParameterType2: IntrinsicTypes.Uint));

            allFunctions.Add(Create0(
                "__XB_GetLaneID",
                "Returns the index of the current lane within the current wave. Does not translate to a specific GCN instruction.",
                IntrinsicTypes.Uint));

            allFunctions.Add(Create0(
                "__XB_GetCUID",
                "Returns a unique identifier indicating on which AMD GCN Compute Unit (CU) the wave is executing. Uses GCN's s_getreg_b32.",
                IntrinsicTypes.Uint));

            allFunctions.Add(Create0(
                "__XB_GetWaveID",
                "Returns a unique identifier indicating where on the GPU the wave is executing. Uses GCN's s_getreg_b32.",
                IntrinsicTypes.Uint));

            allFunctions.Add(Create0(
                "__XB_GetSIMDID",
                "Returns a unique identifier indicating on which SIMD the wave is executing. Uses GCN's s_getreg_b32.",
                IntrinsicTypes.Uint));

            allFunctions.Add(Create0(
                "__XB_GetSEID",
                "Returns the index of the AMD GCN Shader Engine (SE) on which the wave is executing. Uses GCN's s_getreg_b32.",
                IntrinsicTypes.Uint));

            allFunctions.AddRange(Create2(
                "__XB_GetRawInterpoland",
                "Returns the pre-interpolated value for a vertex attribute at the specified corner of the triangle. Corner specified using 'channel', (0, 1 or 2). Translates to GCN's v_interp_mov_f32.",
                new[] { IntrinsicTypes.Float, IntrinsicTypes.Int, IntrinsicTypes.Uint },
                "value", "Raw interpoland to retrieve.",
                "channel", "Specified triangle corner (0, 1 or 2).",
                overrideParameterType2: IntrinsicTypes.Uint));

            allFunctions.Add(Create0(
                "__XB_GetBarycentricCoords_Linear_Center",
                "Returns the barycentric coordinates to the current pixel using linear interpolation and a center-pixel location. Does not translate to any actual instructions.",
                IntrinsicTypes.Float2));

            allFunctions.Add(Create0(
                "__XB_GetBarycentricCoords_Linear_Centroid",
                "Returns the barycentric coordinates to the current pixel using linear interpolation and a centroid modifier. Does not translate to any actual instructions.",
                IntrinsicTypes.Float2));

            allFunctions.Add(Create0(
                "__XB_GetBarycentricCoords_Linear_Sample",
                "Returns the barycentric coordinates to the current pixel using linear interpolation and a sample modifier. Does not translate to any actual instructions.",
                IntrinsicTypes.Float2));

            allFunctions.Add(Create0(
                "__XB_GetBarycentricCoords_Noperspective_Center",
                "Returns the barycentric coordinates to the current pixel using non-perspective correct interpolation and a center-pixel location. Does not translate to any actual instructions.",
                IntrinsicTypes.Float2));

            allFunctions.Add(Create0(
                "__XB_GetBarycentricCoords_Noperspective_Centroid",
                "Returns the barycentric coordinates to the current pixel using non-perspective correct interpolation and a centroid modifier. Does not translate to any actual instructions.",
                IntrinsicTypes.Float2));

            allFunctions.Add(Create0(
                "__XB_GetBarycentricCoords_Noperspective_Sample",
                "Returns the barycentric coordinates to the current pixel using non-perspective correct interpolation and a sample modifier. Does not translate to any actual instructions.",
                IntrinsicTypes.Float2));

            allFunctions.AddRange(Create1(
                "__XB_WaveOR",
                "Performs a wave-wide logical-OR of all thread's values. Does not translate to a specific GCN instruction.",
                new[] { IntrinsicTypes.Uint },
                "value", "Value on which to perform wave-wide logical-OR."));

            allFunctions.AddRange(Create1(
                "__XB_WaveMin_F32",
                "Calculates a wave-wide 32-bit float minimum of all thread's values. Does not translate to a specific GCN instruction.",
                new[] { IntrinsicTypes.Float },
                "value", "Value of which to calculate a wave-wide minimum."));

            allFunctions.AddRange(Create1(
                "__XB_WaveMax_F32",
                "Calculates a wave-wide 32-bit float maximum of all thread's values. Does not translate to a specific GCN instruction.",
                new[] { IntrinsicTypes.Float },
                "value", "Value of which to calculate a wave-wide maximum."));

            allFunctions.AddRange(Create1(
                "__XB_WaveMin_U32",
                "Calculates a wave-wide 32-bit unsigned integer minimum of all thread's values. Does not translate to a specific GCN instruction.",
                new[] { IntrinsicTypes.Uint },
                "value", "Value of which to calculate a wave-wide minimum."));

            allFunctions.AddRange(Create1(
                "__XB_WaveMax_U32",
                "Calculates a wave-wide 32-bit unsigned integer maximum of all thread's values. Does not translate to a specific GCN instruction.",
                new[] { IntrinsicTypes.Uint },
                "value", "Value of which to calculate a wave-wide maximum."));

            allFunctions.AddRange(Create1(
                "__XB_WaveMin_I32",
                "Calculates a wave-wide 32-bit signed integer minimum of all thread's values. Does not translate to a specific GCN instruction.",
                new[] { IntrinsicTypes.Int },
                "value", "Value of which to calculate a wave-wide minimum."));

            allFunctions.AddRange(Create1(
                "__XB_WaveMax_I32",
                "Calculates a wave-wide 32-bit signed integer maximum of all thread's values. Does not translate to a specific GCN instruction.",
                new[] { IntrinsicTypes.Int },
                "value", "Value of which to calculate a wave-wide maximum."));

            allFunctions.AddRange(Create1(
                "__XB_WaveAND",
                "Performs a wave-wide logical-AND of all thread's values. Does not translate to a specific GCN instruction.",
                new[] { IntrinsicTypes.Uint },
                "value", "Value on which to perform wave-wide logical-AND."));

            allFunctions.AddRange(Create1(
                "__XB_WaveAdd_F32",
                "Calculates a wave-wide 32-bit float sum of all thread's values. Does not translate to a specific GCN instruction.",
                new[] { IntrinsicTypes.Float },
                "value", "Value on which to perform a wave-wide sum."));

            allFunctions.AddRange(Create1(
                "__XB_S_BCNT0_U32",
                "Using AMD GCN's scalar instruction set, count the number of zero-bits in the specified 32-bit value. Translates to GCN's s_bcnt0_i32_b32.",
                new[] { IntrinsicTypes.Uint },
                "value", "32-bit value on which to count zero bits."));

            allFunctions.AddRange(Create1(
                "__XB_S_BCNT0_U64",
                "Using AMD GCN's scalar instruction set, count the number of zero-bits in the specified 64-bit value. Translates to GCN's s_bcnt0_i32_b64.",
                new[] { IntrinsicTypes.Uint2 },
                "value", "64-bit value on which to count zero bits.",
                overrideReturnTypes: new[] { IntrinsicTypes.Uint }));

            allFunctions.AddRange(Create1(
                "__XB_S_BCNT1_U32",
                "Using AMD GCN's scalar instruction set, count the number of one-bits in the specified 32-bit value. Translates to GCN's s_bcnt1_i32_b32.",
                new[] { IntrinsicTypes.Uint },
                "value", "32-bit value on which to count one bits."));

            allFunctions.AddRange(Create1(
                "__XB_S_BCNT1_U64",
                "Using AMD GCN's scalar instruction set, count the number of one-bits in the specified 64-bit value. Translates to GCN's s_bcnt1_i32_b64.",
                new[] { IntrinsicTypes.Uint2 },
                "value", "64-bit value on which to count one bits.",
                overrideReturnTypes: new[] { IntrinsicTypes.Uint }));

            allFunctions.AddRange(Create1(
                "__XB_S_BREV_U32",
                "Using AMD GCN's scalar instruction set, reverse the bits in the specified 32-bit value. Translates to GCN's s_brev_b32.",
                new[] { IntrinsicTypes.Uint },
                "value", "32-bit value on which to reverse the bits."));

            allFunctions.AddRange(Create1(
                "__XB_S_BREV_U64",
                "Using AMD GCN's scalar instruction set, reverse the bits in the specified 64-bit value. Translates to GCN's s_brev_b64.",
                new[] { IntrinsicTypes.Uint2 },
                "value", "64-bit value on which to reverse the bits.",
                overrideReturnTypes: new[] { IntrinsicTypes.Uint }));

            allFunctions.AddRange(Create1(
                "__XB_S_FF0_U32",
                "Using AMD GCN's scalar instruction set, find the index of the first zero bit from the LSB of a 32-bit value. Returns -1 if no zeros found. Translates to GCN's s_ff0_i32_b32.",
                new[] { IntrinsicTypes.Uint },
                "value", "32-bit value to scan for the first zero bit."));

            allFunctions.AddRange(Create1(
                "__XB_S_FF0_U64",
                "Using AMD GCN's scalar instruction set, find the index of the first zero bit from the LSB of a 64-bit value. Returns -1 if no zeros found. Translates to GCN's s_ff0_i32_b64.",
                new[] { IntrinsicTypes.Uint2 },
                "value", "64-bit value to scan for the first zero bit.",
                overrideReturnTypes: new[] { IntrinsicTypes.Uint }));

            allFunctions.AddRange(Create1(
                "__XB_S_FF1_U32",
                "Using AMD GCN's scalar instruction set, find the index of the first one bit from the LSB of a 32-bit value. Returns -1 if no ones found. Translates to GCN's s_ff1_i32_b32.",
                new[] { IntrinsicTypes.Uint },
                "value", "32-bit value to scan for the first one bit."));

            allFunctions.AddRange(Create1(
                "__XB_S_FF1_U64",
                "Using AMD GCN's scalar instruction set, find the index of the first one bit from the LSB of a 64-bit value. Returns -1 if no ones found. Translates to GCN's s_ff1_i32_b64.",
                new[] { IntrinsicTypes.Uint2 },
                "value", "64-bit value to scan for the first one bit.",
                overrideReturnTypes: new[] { IntrinsicTypes.Uint }));

            allFunctions.AddRange(Create1(
                "__XB_S_FFO_U32",
                "Using AMD GCN's scalar instruction set, find the index of the first one bit from the MSB of a 32-bit value. Returns -1 if no ones found. Translates to GCN's s_flbit_i32_b32.",
                new[] { IntrinsicTypes.Uint },
                "value", "32-bit value to scan for the first one bit."));

            allFunctions.AddRange(Create1(
                "__XB_S_FFO_U64",
                "Using AMD GCN's scalar instruction set, find the index of the first one bit from the MSB of a 64-bit value. Returns -1 if no ones found. Translates to GCN's s_flbit_i32_b64.",
                new[] { IntrinsicTypes.Uint2 },
                "value", "64-bit value to scan for the first one bit.",
                overrideReturnTypes: new[] { IntrinsicTypes.Uint }));

            allFunctions.AddRange(Create1(
                "__XB_S_FOSB_U32",
                "Using AMD GCN's scalar instruction set, find the first bit opposite of sign bit from the MSB of a 32-bit value. Return -1 if all bits are the same. Translates to GCN's s_flbit_i32.",
                new[] { IntrinsicTypes.Uint },
                "value", "32-bit value to scan for bits opposite to sign bit."));

            allFunctions.AddRange(Create1(
                "__XB_S_FOSB_U64",
                "Using AMD GCN's scalar instruction set, find the first bit opposite of sign bit from the MSB of a 64-bit value. Return -1 if all bits are the same. Translates to GCN's s_flbit_i32_i64.",
                new[] { IntrinsicTypes.Uint2 },
                "value", "64-bit value to scan for bits opposite to sign bit.",
                overrideReturnTypes: new[] { IntrinsicTypes.Uint }));

            allFunctions.AddRange(Create1(
                "__XB_MBCNT64",
                "Counts the number of one bits in a 64-bit mask below the current lane's index (0-63). Translates to use of GCN's v_mbcnt[hi|lo]_u32_b32.",
                new[] { IntrinsicTypes.Uint2 },
                "mask", "64-bit value to count one bits on.",
                overrideReturnTypes: new[] { IntrinsicTypes.Uint }));

            allFunctions.AddRange(Create2(
                "__XB_GdsOrderedCount",
                "Increment an append counter. The operation is done in wavefront-creation order. Translates to use of GCN's ds_ordered_count.",
                new[] { IntrinsicTypes.Uint },
                "value", "32-bit value to add to the count.",
                "flags", "See XDK documentation for details."));

            allFunctions.AddRange(Create2(
                "__XB_GDS_Write_U32",
                "Write the specified value to the specified address in Global Data Share (GDS). Translates to use of GCN's ds_write_b32.",
                new[] { IntrinsicTypes.Uint },
                "address", "Address in bytes to write to.",
                "value", "32-bit value to write to GDS.",
                overrideReturnTypes: new[] { IntrinsicTypes.Void }));

            allFunctions.AddRange(Create1(
                "__XB_GDS_Read_U32",
                "Read a value from the specified address in Global Data Share (GDS). Translates to use of GCN's ds_read_b32.",
                new[] { IntrinsicTypes.Uint },
                "address", "Address in bytes to read from."));

            allFunctions.Add(Create0(
                "__XB_GetEntryM0",
                "Returns the value of the m0 register upon shader entry. Does not translate to any actual instructions.",
                IntrinsicTypes.Uint));

            allFunctions.Add(Create0(
                "__XB_GetEntryActiveMask64",
                "Returns the value of the 'exec' mask (Execution Mask) upon shader entry. Does not translate to any actual instructions.",
                IntrinsicTypes.Uint2));

            allFunctions.AddRange(Create1(
                "__XB_Sin_F32",
                "Generates an AMD GCN 'v_sin_f32' instruction without performing domain fixup as per use of HLSL 'sin'.",
                new[] { IntrinsicTypes.Float },
                "value", "Value to pass to v_sin_f32."));

            allFunctions.AddRange(Create1(
                "__XB_Cos_F32",
                "Generates an AMD GCN 'v_cos_f32' instruction without performing domain fixup as per use of HLSL 'cos'.",
                new[] { IntrinsicTypes.Float },
                "value", "Value to pass to v_cos_f32."));

            allFunctions.AddRange(Create2(
                "__XB_V_CNDMASK_B32",
                "Performs a thread-wise conditional selection between two 32-bit values depending on a 64-bit mask (one bit per thread). Translates to GCN's v_cndmask_b32.",
                new[] { IntrinsicTypes.Uint2 },
                "mask", "64-bit mask to perform thread-wise selection",
                "value", "Select value.x if mask-bit is 0, else value.y.",
                overrideReturnTypes: new[] { IntrinsicTypes.Uint }));

            return allFunctions;
        }
    }
}
