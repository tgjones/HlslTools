﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using HlslTools.Syntax;

namespace HlslTools.Symbols
{
    internal static class IntrinsicTypes
    {
        public static readonly IntrinsicScalarTypeSymbol Void;
        public static readonly IntrinsicScalarTypeSymbol String;
        public static readonly IntrinsicScalarTypeSymbol Bool;
        public static readonly IntrinsicScalarTypeSymbol Int;
        public static readonly IntrinsicScalarTypeSymbol Uint;
        public static readonly IntrinsicScalarTypeSymbol Half;
        public static readonly IntrinsicScalarTypeSymbol Float;
        public static readonly IntrinsicScalarTypeSymbol Double;
        public static readonly IntrinsicScalarTypeSymbol Min16Float;
        public static readonly IntrinsicScalarTypeSymbol Min10Float;
        public static readonly IntrinsicScalarTypeSymbol Min16Int;
        public static readonly IntrinsicScalarTypeSymbol Min12Int;
        public static readonly IntrinsicScalarTypeSymbol Min16Uint;

        public static readonly IntrinsicVectorTypeSymbol Bool1;
        public static readonly IntrinsicVectorTypeSymbol Bool2;
        public static readonly IntrinsicVectorTypeSymbol Bool3;
        public static readonly IntrinsicVectorTypeSymbol Bool4;

        public static readonly IntrinsicVectorTypeSymbol Int1;
        public static readonly IntrinsicVectorTypeSymbol Int2;
        public static readonly IntrinsicVectorTypeSymbol Int3;
        public static readonly IntrinsicVectorTypeSymbol Int4;

        public static readonly IntrinsicVectorTypeSymbol Uint1;
        public static readonly IntrinsicVectorTypeSymbol Uint2;
        public static readonly IntrinsicVectorTypeSymbol Uint3;
        public static readonly IntrinsicVectorTypeSymbol Uint4;

        public static readonly IntrinsicVectorTypeSymbol Half1;
        public static readonly IntrinsicVectorTypeSymbol Half2;
        public static readonly IntrinsicVectorTypeSymbol Half3;
        public static readonly IntrinsicVectorTypeSymbol Half4;

        public static readonly IntrinsicVectorTypeSymbol Float1;
        public static readonly IntrinsicVectorTypeSymbol Float2;
        public static readonly IntrinsicVectorTypeSymbol Float3;
        public static readonly IntrinsicVectorTypeSymbol Float4;

        public static readonly IntrinsicVectorTypeSymbol Double1;
        public static readonly IntrinsicVectorTypeSymbol Double2;
        public static readonly IntrinsicVectorTypeSymbol Double3;
        public static readonly IntrinsicVectorTypeSymbol Double4;

        public static readonly IntrinsicVectorTypeSymbol Min16Float1;
        public static readonly IntrinsicVectorTypeSymbol Min16Float2;
        public static readonly IntrinsicVectorTypeSymbol Min16Float3;
        public static readonly IntrinsicVectorTypeSymbol Min16Float4;

        public static readonly IntrinsicVectorTypeSymbol Min10Float1;
        public static readonly IntrinsicVectorTypeSymbol Min10Float2;
        public static readonly IntrinsicVectorTypeSymbol Min10Float3;
        public static readonly IntrinsicVectorTypeSymbol Min10Float4;

        public static readonly IntrinsicVectorTypeSymbol Min16Int1;
        public static readonly IntrinsicVectorTypeSymbol Min16Int2;
        public static readonly IntrinsicVectorTypeSymbol Min16Int3;
        public static readonly IntrinsicVectorTypeSymbol Min16Int4;

        public static readonly IntrinsicVectorTypeSymbol Min12Int1;
        public static readonly IntrinsicVectorTypeSymbol Min12Int2;
        public static readonly IntrinsicVectorTypeSymbol Min12Int3;
        public static readonly IntrinsicVectorTypeSymbol Min12Int4;

        public static readonly IntrinsicVectorTypeSymbol Min16Uint1;
        public static readonly IntrinsicVectorTypeSymbol Min16Uint2;
        public static readonly IntrinsicVectorTypeSymbol Min16Uint3;
        public static readonly IntrinsicVectorTypeSymbol Min16Uint4;

        public static readonly IntrinsicMatrixTypeSymbol Bool1x1;
        public static readonly IntrinsicMatrixTypeSymbol Bool1x2;
        public static readonly IntrinsicMatrixTypeSymbol Bool1x3;
        public static readonly IntrinsicMatrixTypeSymbol Bool1x4;
        public static readonly IntrinsicMatrixTypeSymbol Bool2x1;
        public static readonly IntrinsicMatrixTypeSymbol Bool2x2;
        public static readonly IntrinsicMatrixTypeSymbol Bool2x3;
        public static readonly IntrinsicMatrixTypeSymbol Bool2x4;
        public static readonly IntrinsicMatrixTypeSymbol Bool3x1;
        public static readonly IntrinsicMatrixTypeSymbol Bool3x2;
        public static readonly IntrinsicMatrixTypeSymbol Bool3x3;
        public static readonly IntrinsicMatrixTypeSymbol Bool3x4;
        public static readonly IntrinsicMatrixTypeSymbol Bool4x1;
        public static readonly IntrinsicMatrixTypeSymbol Bool4x2;
        public static readonly IntrinsicMatrixTypeSymbol Bool4x3;
        public static readonly IntrinsicMatrixTypeSymbol Bool4x4;

        public static readonly IntrinsicMatrixTypeSymbol Int1x1;
        public static readonly IntrinsicMatrixTypeSymbol Int1x2;
        public static readonly IntrinsicMatrixTypeSymbol Int1x3;
        public static readonly IntrinsicMatrixTypeSymbol Int1x4;
        public static readonly IntrinsicMatrixTypeSymbol Int2x1;
        public static readonly IntrinsicMatrixTypeSymbol Int2x2;
        public static readonly IntrinsicMatrixTypeSymbol Int2x3;
        public static readonly IntrinsicMatrixTypeSymbol Int2x4;
        public static readonly IntrinsicMatrixTypeSymbol Int3x1;
        public static readonly IntrinsicMatrixTypeSymbol Int3x2;
        public static readonly IntrinsicMatrixTypeSymbol Int3x3;
        public static readonly IntrinsicMatrixTypeSymbol Int3x4;
        public static readonly IntrinsicMatrixTypeSymbol Int4x1;
        public static readonly IntrinsicMatrixTypeSymbol Int4x2;
        public static readonly IntrinsicMatrixTypeSymbol Int4x3;
        public static readonly IntrinsicMatrixTypeSymbol Int4x4;

        public static readonly IntrinsicMatrixTypeSymbol Uint1x1;
        public static readonly IntrinsicMatrixTypeSymbol Uint1x2;
        public static readonly IntrinsicMatrixTypeSymbol Uint1x3;
        public static readonly IntrinsicMatrixTypeSymbol Uint1x4;
        public static readonly IntrinsicMatrixTypeSymbol Uint2x1;
        public static readonly IntrinsicMatrixTypeSymbol Uint2x2;
        public static readonly IntrinsicMatrixTypeSymbol Uint2x3;
        public static readonly IntrinsicMatrixTypeSymbol Uint2x4;
        public static readonly IntrinsicMatrixTypeSymbol Uint3x1;
        public static readonly IntrinsicMatrixTypeSymbol Uint3x2;
        public static readonly IntrinsicMatrixTypeSymbol Uint3x3;
        public static readonly IntrinsicMatrixTypeSymbol Uint3x4;
        public static readonly IntrinsicMatrixTypeSymbol Uint4x1;
        public static readonly IntrinsicMatrixTypeSymbol Uint4x2;
        public static readonly IntrinsicMatrixTypeSymbol Uint4x3;
        public static readonly IntrinsicMatrixTypeSymbol Uint4x4;

        public static readonly IntrinsicMatrixTypeSymbol Half1x1;
        public static readonly IntrinsicMatrixTypeSymbol Half1x2;
        public static readonly IntrinsicMatrixTypeSymbol Half1x3;
        public static readonly IntrinsicMatrixTypeSymbol Half1x4;
        public static readonly IntrinsicMatrixTypeSymbol Half2x1;
        public static readonly IntrinsicMatrixTypeSymbol Half2x2;
        public static readonly IntrinsicMatrixTypeSymbol Half2x3;
        public static readonly IntrinsicMatrixTypeSymbol Half2x4;
        public static readonly IntrinsicMatrixTypeSymbol Half3x1;
        public static readonly IntrinsicMatrixTypeSymbol Half3x2;
        public static readonly IntrinsicMatrixTypeSymbol Half3x3;
        public static readonly IntrinsicMatrixTypeSymbol Half3x4;
        public static readonly IntrinsicMatrixTypeSymbol Half4x1;
        public static readonly IntrinsicMatrixTypeSymbol Half4x2;
        public static readonly IntrinsicMatrixTypeSymbol Half4x3;
        public static readonly IntrinsicMatrixTypeSymbol Half4x4;

        public static readonly IntrinsicMatrixTypeSymbol Float1x1;
        public static readonly IntrinsicMatrixTypeSymbol Float1x2;
        public static readonly IntrinsicMatrixTypeSymbol Float1x3;
        public static readonly IntrinsicMatrixTypeSymbol Float1x4;
        public static readonly IntrinsicMatrixTypeSymbol Float2x1;
        public static readonly IntrinsicMatrixTypeSymbol Float2x2;
        public static readonly IntrinsicMatrixTypeSymbol Float2x3;
        public static readonly IntrinsicMatrixTypeSymbol Float2x4;
        public static readonly IntrinsicMatrixTypeSymbol Float3x1;
        public static readonly IntrinsicMatrixTypeSymbol Float3x2;
        public static readonly IntrinsicMatrixTypeSymbol Float3x3;
        public static readonly IntrinsicMatrixTypeSymbol Float3x4;
        public static readonly IntrinsicMatrixTypeSymbol Float4x1;
        public static readonly IntrinsicMatrixTypeSymbol Float4x2;
        public static readonly IntrinsicMatrixTypeSymbol Float4x3;
        public static readonly IntrinsicMatrixTypeSymbol Float4x4;

        public static readonly IntrinsicMatrixTypeSymbol Double1x1;
        public static readonly IntrinsicMatrixTypeSymbol Double1x2;
        public static readonly IntrinsicMatrixTypeSymbol Double1x3;
        public static readonly IntrinsicMatrixTypeSymbol Double1x4;
        public static readonly IntrinsicMatrixTypeSymbol Double2x1;
        public static readonly IntrinsicMatrixTypeSymbol Double2x2;
        public static readonly IntrinsicMatrixTypeSymbol Double2x3;
        public static readonly IntrinsicMatrixTypeSymbol Double2x4;
        public static readonly IntrinsicMatrixTypeSymbol Double3x1;
        public static readonly IntrinsicMatrixTypeSymbol Double3x2;
        public static readonly IntrinsicMatrixTypeSymbol Double3x3;
        public static readonly IntrinsicMatrixTypeSymbol Double3x4;
        public static readonly IntrinsicMatrixTypeSymbol Double4x1;
        public static readonly IntrinsicMatrixTypeSymbol Double4x2;
        public static readonly IntrinsicMatrixTypeSymbol Double4x3;
        public static readonly IntrinsicMatrixTypeSymbol Double4x4;

        public static readonly IntrinsicMatrixTypeSymbol Min16Float1x1;
        public static readonly IntrinsicMatrixTypeSymbol Min16Float1x2;
        public static readonly IntrinsicMatrixTypeSymbol Min16Float1x3;
        public static readonly IntrinsicMatrixTypeSymbol Min16Float1x4;
        public static readonly IntrinsicMatrixTypeSymbol Min16Float2x1;
        public static readonly IntrinsicMatrixTypeSymbol Min16Float2x2;
        public static readonly IntrinsicMatrixTypeSymbol Min16Float2x3;
        public static readonly IntrinsicMatrixTypeSymbol Min16Float2x4;
        public static readonly IntrinsicMatrixTypeSymbol Min16Float3x1;
        public static readonly IntrinsicMatrixTypeSymbol Min16Float3x2;
        public static readonly IntrinsicMatrixTypeSymbol Min16Float3x3;
        public static readonly IntrinsicMatrixTypeSymbol Min16Float3x4;
        public static readonly IntrinsicMatrixTypeSymbol Min16Float4x1;
        public static readonly IntrinsicMatrixTypeSymbol Min16Float4x2;
        public static readonly IntrinsicMatrixTypeSymbol Min16Float4x3;
        public static readonly IntrinsicMatrixTypeSymbol Min16Float4x4;

        public static readonly IntrinsicMatrixTypeSymbol Min10Float1x1;
        public static readonly IntrinsicMatrixTypeSymbol Min10Float1x2;
        public static readonly IntrinsicMatrixTypeSymbol Min10Float1x3;
        public static readonly IntrinsicMatrixTypeSymbol Min10Float1x4;
        public static readonly IntrinsicMatrixTypeSymbol Min10Float2x1;
        public static readonly IntrinsicMatrixTypeSymbol Min10Float2x2;
        public static readonly IntrinsicMatrixTypeSymbol Min10Float2x3;
        public static readonly IntrinsicMatrixTypeSymbol Min10Float2x4;
        public static readonly IntrinsicMatrixTypeSymbol Min10Float3x1;
        public static readonly IntrinsicMatrixTypeSymbol Min10Float3x2;
        public static readonly IntrinsicMatrixTypeSymbol Min10Float3x3;
        public static readonly IntrinsicMatrixTypeSymbol Min10Float3x4;
        public static readonly IntrinsicMatrixTypeSymbol Min10Float4x1;
        public static readonly IntrinsicMatrixTypeSymbol Min10Float4x2;
        public static readonly IntrinsicMatrixTypeSymbol Min10Float4x3;
        public static readonly IntrinsicMatrixTypeSymbol Min10Float4x4;

        public static readonly IntrinsicMatrixTypeSymbol Min16Int1x1;
        public static readonly IntrinsicMatrixTypeSymbol Min16Int1x2;
        public static readonly IntrinsicMatrixTypeSymbol Min16Int1x3;
        public static readonly IntrinsicMatrixTypeSymbol Min16Int1x4;
        public static readonly IntrinsicMatrixTypeSymbol Min16Int2x1;
        public static readonly IntrinsicMatrixTypeSymbol Min16Int2x2;
        public static readonly IntrinsicMatrixTypeSymbol Min16Int2x3;
        public static readonly IntrinsicMatrixTypeSymbol Min16Int2x4;
        public static readonly IntrinsicMatrixTypeSymbol Min16Int3x1;
        public static readonly IntrinsicMatrixTypeSymbol Min16Int3x2;
        public static readonly IntrinsicMatrixTypeSymbol Min16Int3x3;
        public static readonly IntrinsicMatrixTypeSymbol Min16Int3x4;
        public static readonly IntrinsicMatrixTypeSymbol Min16Int4x1;
        public static readonly IntrinsicMatrixTypeSymbol Min16Int4x2;
        public static readonly IntrinsicMatrixTypeSymbol Min16Int4x3;
        public static readonly IntrinsicMatrixTypeSymbol Min16Int4x4;

        public static readonly IntrinsicMatrixTypeSymbol Min12Int1x1;
        public static readonly IntrinsicMatrixTypeSymbol Min12Int1x2;
        public static readonly IntrinsicMatrixTypeSymbol Min12Int1x3;
        public static readonly IntrinsicMatrixTypeSymbol Min12Int1x4;
        public static readonly IntrinsicMatrixTypeSymbol Min12Int2x1;
        public static readonly IntrinsicMatrixTypeSymbol Min12Int2x2;
        public static readonly IntrinsicMatrixTypeSymbol Min12Int2x3;
        public static readonly IntrinsicMatrixTypeSymbol Min12Int2x4;
        public static readonly IntrinsicMatrixTypeSymbol Min12Int3x1;
        public static readonly IntrinsicMatrixTypeSymbol Min12Int3x2;
        public static readonly IntrinsicMatrixTypeSymbol Min12Int3x3;
        public static readonly IntrinsicMatrixTypeSymbol Min12Int3x4;
        public static readonly IntrinsicMatrixTypeSymbol Min12Int4x1;
        public static readonly IntrinsicMatrixTypeSymbol Min12Int4x2;
        public static readonly IntrinsicMatrixTypeSymbol Min12Int4x3;
        public static readonly IntrinsicMatrixTypeSymbol Min12Int4x4;

        public static readonly IntrinsicMatrixTypeSymbol Min16Uint1x1;
        public static readonly IntrinsicMatrixTypeSymbol Min16Uint1x2;
        public static readonly IntrinsicMatrixTypeSymbol Min16Uint1x3;
        public static readonly IntrinsicMatrixTypeSymbol Min16Uint1x4;
        public static readonly IntrinsicMatrixTypeSymbol Min16Uint2x1;
        public static readonly IntrinsicMatrixTypeSymbol Min16Uint2x2;
        public static readonly IntrinsicMatrixTypeSymbol Min16Uint2x3;
        public static readonly IntrinsicMatrixTypeSymbol Min16Uint2x4;
        public static readonly IntrinsicMatrixTypeSymbol Min16Uint3x1;
        public static readonly IntrinsicMatrixTypeSymbol Min16Uint3x2;
        public static readonly IntrinsicMatrixTypeSymbol Min16Uint3x3;
        public static readonly IntrinsicMatrixTypeSymbol Min16Uint3x4;
        public static readonly IntrinsicMatrixTypeSymbol Min16Uint4x1;
        public static readonly IntrinsicMatrixTypeSymbol Min16Uint4x2;
        public static readonly IntrinsicMatrixTypeSymbol Min16Uint4x3;
        public static readonly IntrinsicMatrixTypeSymbol Min16Uint4x4;

        public static readonly IntrinsicScalarTypeSymbol[] AllScalarTypes;

        public static readonly IntrinsicVectorTypeSymbol[] AllBoolVectorTypes;
        public static readonly IntrinsicVectorTypeSymbol[] AllIntVectorTypes;
        public static readonly IntrinsicVectorTypeSymbol[] AllUintVectorTypes;
        public static readonly IntrinsicVectorTypeSymbol[] AllHalfVectorTypes;
        public static readonly IntrinsicVectorTypeSymbol[] AllFloatVectorTypes;
        public static readonly IntrinsicVectorTypeSymbol[] AllDoubleVectorTypes;
        public static readonly IntrinsicVectorTypeSymbol[] AllMin16FloatVectorTypes;
        public static readonly IntrinsicVectorTypeSymbol[] AllMin10FloatVectorTypes;
        public static readonly IntrinsicVectorTypeSymbol[] AllMin16IntVectorTypes;
        public static readonly IntrinsicVectorTypeSymbol[] AllMin12IntVectorTypes;
        public static readonly IntrinsicVectorTypeSymbol[] AllMin16UintVectorTypes;
        public static readonly IntrinsicVectorTypeSymbol[] AllVectorTypes;

        public static readonly IntrinsicMatrixTypeSymbol[] AllBoolMatrixTypes;
        public static readonly IntrinsicMatrixTypeSymbol[] AllIntMatrixTypes;
        public static readonly IntrinsicMatrixTypeSymbol[] AllUintMatrixTypes;
        public static readonly IntrinsicMatrixTypeSymbol[] AllHalfMatrixTypes;
        public static readonly IntrinsicMatrixTypeSymbol[] AllFloatMatrixTypes;
        public static readonly IntrinsicMatrixTypeSymbol[] AllDoubleMatrixTypes;
        public static readonly IntrinsicMatrixTypeSymbol[] AllMin16FloatMatrixTypes;
        public static readonly IntrinsicMatrixTypeSymbol[] AllMin10FloatMatrixTypes;
        public static readonly IntrinsicMatrixTypeSymbol[] AllMin16IntMatrixTypes;
        public static readonly IntrinsicMatrixTypeSymbol[] AllMin12IntMatrixTypes;
        public static readonly IntrinsicMatrixTypeSymbol[] AllMin16UintMatrixTypes;
        public static readonly IntrinsicMatrixTypeSymbol[] AllMatrixTypes;

        public static readonly IntrinsicNumericTypeSymbol[] AllBoolTypes;
        public static readonly IntrinsicNumericTypeSymbol[] AllIntTypes;
        public static readonly IntrinsicNumericTypeSymbol[] AllUintTypes;
        public static readonly IntrinsicNumericTypeSymbol[] AllHalfTypes;
        public static readonly IntrinsicNumericTypeSymbol[] AllFloatTypes;
        public static readonly IntrinsicNumericTypeSymbol[] AllDoubleTypes;
        public static readonly IntrinsicNumericTypeSymbol[] AllMin16FloatTypes;
        public static readonly IntrinsicNumericTypeSymbol[] AllMin10FloatTypes;
        public static readonly IntrinsicNumericTypeSymbol[] AllMin16IntTypes;
        public static readonly IntrinsicNumericTypeSymbol[] AllMin12IntTypes;
        public static readonly IntrinsicNumericTypeSymbol[] AllMin16UintTypes;

        public static readonly IntrinsicNumericTypeSymbol[] AllIntegralTypes;
        public static readonly IntrinsicNumericTypeSymbol[] AllNumericNonBoolTypes;
        public static readonly IntrinsicNumericTypeSymbol[] AllNumericTypes;

        public static readonly IntrinsicObjectTypeSymbol Sampler;
        public static readonly IntrinsicObjectTypeSymbol Sampler1D;
        public static readonly IntrinsicObjectTypeSymbol Sampler2D;
        public static readonly IntrinsicObjectTypeSymbol Sampler3D;
        public static readonly IntrinsicObjectTypeSymbol SamplerCube;
        public static readonly IntrinsicObjectTypeSymbol SamplerState;
        public static readonly IntrinsicObjectTypeSymbol SamplerComparisonState;
        public static readonly IntrinsicObjectTypeSymbol LegacyTexture;

        public static readonly IntrinsicObjectTypeSymbol BlendState;
        public static readonly IntrinsicObjectTypeSymbol DepthStencilState;
        public static readonly IntrinsicObjectTypeSymbol RasterizerState;

        public static readonly IntrinsicObjectTypeSymbol GeometryShader;
        public static readonly IntrinsicObjectTypeSymbol PixelShader;
        public static readonly IntrinsicObjectTypeSymbol VertexShader;

        public static readonly TypeSymbol[] AllTypes;

        static IntrinsicTypes()
        {
            // Scalar types.
            Void = new IntrinsicScalarTypeSymbol("void", "Represents a void value.", ScalarType.Void);
            String = new IntrinsicScalarTypeSymbol("string", "Represents a string value.", ScalarType.String);
            Bool = new IntrinsicScalarTypeSymbol("bool", "Represents a boolean value.", ScalarType.Bool);
            Int = new IntrinsicScalarTypeSymbol("int", "Represents a 32-bit signed integer value.", ScalarType.Int);
            Uint = new IntrinsicScalarTypeSymbol("uint", "Represents a 32-bit unsigned integer value.", ScalarType.Uint);
            Half = new IntrinsicScalarTypeSymbol("half", "Represents a 16-bit floating point value.", ScalarType.Half);
            Float = new IntrinsicScalarTypeSymbol("float", "Represents a 32-bit floating point value.", ScalarType.Float);
            Double = new IntrinsicScalarTypeSymbol("double", "Represents a 64-bit floating point value.", ScalarType.Double);
            Min16Float = new IntrinsicScalarTypeSymbol("min16float", "Represents minimum a 16-bit floating point value.", ScalarType.Min16Float);
            Min10Float = new IntrinsicScalarTypeSymbol("min10float", "Represents minimum a 10-bit floating point value.", ScalarType.Min10Float);
            Min16Int = new IntrinsicScalarTypeSymbol("min16int", "Represents minimum a 16-bit signed integer value.", ScalarType.Min16Int);
            Min12Int = new IntrinsicScalarTypeSymbol("min12int", "Represents minimum a 12-bit signed integer value.", ScalarType.Min12Int);
            Min16Uint = new IntrinsicScalarTypeSymbol("min16uint", "Represents minimum a 16-bit unsigned integer value.", ScalarType.Min16Uint);

            // Vector types.
            Bool1 = new IntrinsicVectorTypeSymbol("bool1", "Represents a vector containing 1 boolean component.", ScalarType.Bool, 1);
            Bool2 = new IntrinsicVectorTypeSymbol("bool2", "Represents a vector containing 2 boolean components.", ScalarType.Bool, 2);
            Bool3 = new IntrinsicVectorTypeSymbol("bool3", "Represents a vector containing 3 boolean components.", ScalarType.Bool, 3);
            Bool4 = new IntrinsicVectorTypeSymbol("bool4", "Represents a vector containing 4 boolean components.", ScalarType.Bool, 4);
            Int1 = new IntrinsicVectorTypeSymbol("int1", "Represents a vector containing 1 signed integer component.", ScalarType.Int, 1);
            Int2 = new IntrinsicVectorTypeSymbol("int2", "Represents a vector containing 2 signed integer components.", ScalarType.Int, 2);
            Int3 = new IntrinsicVectorTypeSymbol("int3", "Represents a vector containing 3 signed integer components.", ScalarType.Int, 3);
            Int4 = new IntrinsicVectorTypeSymbol("int4", "Represents a vector containing 4 signed integer components.", ScalarType.Int, 4);
            Uint1 = new IntrinsicVectorTypeSymbol("uint1", "Represents a vector containing 1 unsigned integer component.", ScalarType.Uint, 1);
            Uint2 = new IntrinsicVectorTypeSymbol("uint2", "Represents a vector containing 2 unsigned integer components.", ScalarType.Uint, 2);
            Uint3 = new IntrinsicVectorTypeSymbol("uint3", "Represents a vector containing 3 unsigned integer components.", ScalarType.Uint, 3);
            Uint4 = new IntrinsicVectorTypeSymbol("uint4", "Represents a vector containing 4 unsigned integer components.", ScalarType.Uint, 4);
            Half1 = new IntrinsicVectorTypeSymbol("half1", "Represents a vector containing 1 floating point component.", ScalarType.Half, 1);
            Half2 = new IntrinsicVectorTypeSymbol("half2", "Represents a vector containing 2 floating point components.", ScalarType.Half, 2);
            Half3 = new IntrinsicVectorTypeSymbol("half3", "Represents a vector containing 3 floating point components.", ScalarType.Half, 3);
            Half4 = new IntrinsicVectorTypeSymbol("half4", "Represents a vector containing 4 floating point components.", ScalarType.Half, 4);
            Float1 = new IntrinsicVectorTypeSymbol("float1", "Represents a vector containing 1 floating point component.", ScalarType.Float, 1);
            Float2 = new IntrinsicVectorTypeSymbol("float2", "Represents a vector containing 2 floating point components.", ScalarType.Float, 2);
            Float3 = new IntrinsicVectorTypeSymbol("float3", "Represents a vector containing 3 floating point components.", ScalarType.Float, 3);
            Float4 = new IntrinsicVectorTypeSymbol("float4", "Represents a vector containing 4 floating point components.", ScalarType.Float, 4);
            Double1 = new IntrinsicVectorTypeSymbol("double1", "Represents a vector containing 1 floating point component.", ScalarType.Double, 1);
            Double2 = new IntrinsicVectorTypeSymbol("double2", "Represents a vector containing 2 floating point components.", ScalarType.Double, 2);
            Double3 = new IntrinsicVectorTypeSymbol("double3", "Represents a vector containing 3 floating point components.", ScalarType.Double, 3);
            Double4 = new IntrinsicVectorTypeSymbol("double4", "Represents a vector containing 4 floating point components.", ScalarType.Double, 4);
            Min16Float1 = new IntrinsicVectorTypeSymbol("min16float1", "Represents a vector containing 1 floating point component.", ScalarType.Min16Float, 1);
            Min16Float2 = new IntrinsicVectorTypeSymbol("min16float2", "Represents a vector containing 2 floating point components.", ScalarType.Min16Float, 2);
            Min16Float3 = new IntrinsicVectorTypeSymbol("min16float3", "Represents a vector containing 3 floating point components.", ScalarType.Min16Float, 3);
            Min16Float4 = new IntrinsicVectorTypeSymbol("min16float4", "Represents a vector containing 4 floating point components.", ScalarType.Min16Float, 4);
            Min10Float1 = new IntrinsicVectorTypeSymbol("min10float1", "Represents a vector containing 1 floating point component.", ScalarType.Min10Float, 1);
            Min10Float2 = new IntrinsicVectorTypeSymbol("min10float2", "Represents a vector containing 2 floating point components.", ScalarType.Min10Float, 2);
            Min10Float3 = new IntrinsicVectorTypeSymbol("min10float3", "Represents a vector containing 3 floating point components.", ScalarType.Min10Float, 3);
            Min10Float4 = new IntrinsicVectorTypeSymbol("min10float4", "Represents a vector containing 4 floating point components.", ScalarType.Min10Float, 4);
            Min16Int1 = new IntrinsicVectorTypeSymbol("min16int1", "Represents a vector containing 1 signed integer component.", ScalarType.Min16Int, 1);
            Min16Int2 = new IntrinsicVectorTypeSymbol("min16int2", "Represents a vector containing 2 signed integer components.", ScalarType.Min16Int, 2);
            Min16Int3 = new IntrinsicVectorTypeSymbol("min16int3", "Represents a vector containing 3 signed integer components.", ScalarType.Min16Int, 3);
            Min16Int4 = new IntrinsicVectorTypeSymbol("min16int4", "Represents a vector containing 4 signed integer components.", ScalarType.Min16Int, 4);
            Min12Int1 = new IntrinsicVectorTypeSymbol("min12int1", "Represents a vector containing 1 signed integer component.", ScalarType.Min12Int, 1);
            Min12Int2 = new IntrinsicVectorTypeSymbol("min12int2", "Represents a vector containing 2 signed integer components.", ScalarType.Min12Int, 2);
            Min12Int3 = new IntrinsicVectorTypeSymbol("min12int3", "Represents a vector containing 3 signed integer components.", ScalarType.Min12Int, 3);
            Min12Int4 = new IntrinsicVectorTypeSymbol("min12int4", "Represents a vector containing 4 signed integer components.", ScalarType.Min12Int, 4);
            Min16Uint1 = new IntrinsicVectorTypeSymbol("min16uint1", "Represents a vector containing 1 unsigned integer component.", ScalarType.Min16Uint, 1);
            Min16Uint2 = new IntrinsicVectorTypeSymbol("min16uint2", "Represents a vector containing 2 unsigned integer components.", ScalarType.Min16Uint, 2);
            Min16Uint3 = new IntrinsicVectorTypeSymbol("min16uint3", "Represents a vector containing 3 unsigned integer components.", ScalarType.Min16Uint, 3);
            Min16Uint4 = new IntrinsicVectorTypeSymbol("min16uint4", "Represents a vector containing 4 unsigned integer components.", ScalarType.Min16Uint, 4);

            Bool.AddMembers(CreateScalarTypeFields(1, Bool, Bool1, Bool2, Bool3, Bool4));
            Int.AddMembers(CreateScalarTypeFields(1, Int, Int1, Int2, Int3, Int4));
            Uint.AddMembers(CreateScalarTypeFields(1, Uint, Uint1, Uint2, Uint3, Uint4));
            Half.AddMembers(CreateScalarTypeFields(1, Half, Half1, Half2, Half3, Half4));
            Float.AddMembers(CreateScalarTypeFields(1, Float, Float1, Float2, Float3, Float4));
            Double.AddMembers(CreateScalarTypeFields(1, Double, Double1, Double2, Double3, Double4));
            Min16Float.AddMembers(CreateScalarTypeFields(1, Min16Float, Min16Float1, Min16Float2, Min16Float3, Min16Float4));
            Min10Float.AddMembers(CreateScalarTypeFields(1, Min10Float, Min10Float1, Min10Float2, Min10Float3, Min10Float4));
            Min16Int.AddMembers(CreateScalarTypeFields(1, Min16Int, Min16Int1, Min16Int2, Min16Int3, Min16Int4));
            Min12Int.AddMembers(CreateScalarTypeFields(1, Min12Int, Min12Int1, Min12Int2, Min12Int3, Min12Int4));
            Min16Uint.AddMembers(CreateScalarTypeFields(1, Min16Uint, Min16Uint1, Min16Uint2, Min16Uint3, Min16Uint4));

            Bool1.AddMembers(CreateVectorTypeFields(1, Bool1, Bool, Bool1, Bool2, Bool3, Bool4));
            Bool2.AddMembers(CreateVectorTypeFields(2, Bool2, Bool, Bool1, Bool2, Bool3, Bool4));
            Bool3.AddMembers(CreateVectorTypeFields(3, Bool3, Bool, Bool1, Bool2, Bool3, Bool4));
            Bool4.AddMembers(CreateVectorTypeFields(4, Bool4, Bool, Bool1, Bool2, Bool3, Bool4));
            Int1.AddMembers(CreateVectorTypeFields(1, Int1, Int, Int1, Int2, Int3, Int4));
            Int2.AddMembers(CreateVectorTypeFields(2, Int2, Int, Int1, Int2, Int3, Int4));
            Int3.AddMembers(CreateVectorTypeFields(3, Int3, Int, Int1, Int2, Int3, Int4));
            Int4.AddMembers(CreateVectorTypeFields(4, Int4, Int, Int1, Int2, Int3, Int4));
            Uint1.AddMembers(CreateVectorTypeFields(1, Uint1, Uint, Uint1, Uint2, Uint3, Uint4));
            Uint2.AddMembers(CreateVectorTypeFields(2, Uint2, Uint, Uint1, Uint2, Uint3, Uint4));
            Uint3.AddMembers(CreateVectorTypeFields(3, Uint3, Uint, Uint1, Uint2, Uint3, Uint4));
            Uint4.AddMembers(CreateVectorTypeFields(4, Uint4, Uint, Uint1, Uint2, Uint3, Uint4));
            Half1.AddMembers(CreateVectorTypeFields(1, Half1, Half, Half1, Half2, Half3, Half4));
            Half2.AddMembers(CreateVectorTypeFields(2, Half2, Half, Half1, Half2, Half3, Half4));
            Half3.AddMembers(CreateVectorTypeFields(3, Half3, Half, Half1, Half2, Half3, Half4));
            Half4.AddMembers(CreateVectorTypeFields(4, Half4, Half, Half1, Half2, Half3, Half4));
            Float1.AddMembers(CreateVectorTypeFields(1, Float1, Float, Float1, Float2, Float3, Float4));
            Float2.AddMembers(CreateVectorTypeFields(2, Float2, Float, Float1, Float2, Float3, Float4));
            Float3.AddMembers(CreateVectorTypeFields(3, Float3, Float, Float1, Float2, Float3, Float4));
            Float4.AddMembers(CreateVectorTypeFields(4, Float4, Float, Float1, Float2, Float3, Float4));
            Double1.AddMembers(CreateVectorTypeFields(1, Double1, Double, Double1, Double2, Double3, Double4));
            Double2.AddMembers(CreateVectorTypeFields(2, Double2, Double, Double1, Double2, Double3, Double4));
            Double3.AddMembers(CreateVectorTypeFields(3, Double3, Double, Double1, Double2, Double3, Double4));
            Double4.AddMembers(CreateVectorTypeFields(4, Double4, Double, Double1, Double2, Double3, Double4));
            Min16Float1.AddMembers(CreateVectorTypeFields(1, Min16Float1, Min16Float, Min16Float1, Min16Float2, Min16Float3, Min16Float4));
            Min16Float2.AddMembers(CreateVectorTypeFields(2, Min16Float2, Min16Float, Min16Float1, Min16Float2, Min16Float3, Min16Float4));
            Min16Float3.AddMembers(CreateVectorTypeFields(3, Min16Float3, Min16Float, Min16Float1, Min16Float2, Min16Float3, Min16Float4));
            Min16Float4.AddMembers(CreateVectorTypeFields(4, Min16Float4, Min16Float, Min16Float1, Min16Float2, Min16Float3, Min16Float4));
            Min10Float1.AddMembers(CreateVectorTypeFields(1, Min10Float1, Min10Float, Min10Float1, Min10Float2, Min10Float3, Min10Float4));
            Min10Float2.AddMembers(CreateVectorTypeFields(2, Min10Float2, Min10Float, Min10Float1, Min10Float2, Min10Float3, Min10Float4));
            Min10Float3.AddMembers(CreateVectorTypeFields(3, Min10Float3, Min10Float, Min10Float1, Min10Float2, Min10Float3, Min10Float4));
            Min10Float4.AddMembers(CreateVectorTypeFields(4, Min10Float4, Min10Float, Min10Float1, Min10Float2, Min10Float3, Min10Float4));
            Min16Int1.AddMembers(CreateVectorTypeFields(1, Min16Int1, Min16Int, Min16Int1, Min16Int2, Min16Int3, Min16Int4));
            Min16Int2.AddMembers(CreateVectorTypeFields(2, Min16Int2, Min16Int, Min16Int1, Min16Int2, Min16Int3, Min16Int4));
            Min16Int3.AddMembers(CreateVectorTypeFields(3, Min16Int3, Min16Int, Min16Int1, Min16Int2, Min16Int3, Min16Int4));
            Min16Int4.AddMembers(CreateVectorTypeFields(4, Min16Int4, Min16Int, Min16Int1, Min16Int2, Min16Int3, Min16Int4));
            Min12Int1.AddMembers(CreateVectorTypeFields(1, Min12Int1, Min12Int, Min12Int1, Min12Int2, Min12Int3, Min12Int4));
            Min12Int2.AddMembers(CreateVectorTypeFields(2, Min12Int2, Min12Int, Min12Int1, Min12Int2, Min12Int3, Min12Int4));
            Min12Int3.AddMembers(CreateVectorTypeFields(3, Min12Int3, Min12Int, Min12Int1, Min12Int2, Min12Int3, Min12Int4));
            Min12Int4.AddMembers(CreateVectorTypeFields(4, Min12Int4, Min12Int, Min12Int1, Min12Int2, Min12Int3, Min12Int4));
            Min16Uint1.AddMembers(CreateVectorTypeFields(1, Min16Uint1, Min16Uint, Min16Uint1, Min16Uint2, Min16Uint3, Min16Uint4));
            Min16Uint2.AddMembers(CreateVectorTypeFields(2, Min16Uint2, Min16Uint, Min16Uint1, Min16Uint2, Min16Uint3, Min16Uint4));
            Min16Uint3.AddMembers(CreateVectorTypeFields(3, Min16Uint3, Min16Uint, Min16Uint1, Min16Uint2, Min16Uint3, Min16Uint4));
            Min16Uint4.AddMembers(CreateVectorTypeFields(4, Min16Uint4, Min16Uint, Min16Uint1, Min16Uint2, Min16Uint3, Min16Uint4));

            // Matrix types.
            Bool1x1 = new IntrinsicMatrixTypeSymbol("bool1x1", "Represents a matrix containing 1 row and 1 column.", ScalarType.Bool, 1, 1);
            Bool1x2 = new IntrinsicMatrixTypeSymbol("bool1x2", "Represents a matrix containing 1 row and 2 columns.", ScalarType.Bool, 1, 2);
            Bool1x3 = new IntrinsicMatrixTypeSymbol("bool1x3", "Represents a matrix containing 1 row and 3 columns.", ScalarType.Bool, 1, 3);
            Bool1x4 = new IntrinsicMatrixTypeSymbol("bool1x4", "Represents a matrix containing 1 row and 4 columns.", ScalarType.Bool, 1, 4);
            Bool2x1 = new IntrinsicMatrixTypeSymbol("bool2x1", "Represents a matrix containing 2 rows and 1 column.", ScalarType.Bool, 2, 1);
            Bool2x2 = new IntrinsicMatrixTypeSymbol("bool2x2", "Represents a matrix containing 2 rows and 2 columns.", ScalarType.Bool, 2, 2);
            Bool2x3 = new IntrinsicMatrixTypeSymbol("bool2x3", "Represents a matrix containing 2 rows and 3 columns.", ScalarType.Bool, 2, 3);
            Bool2x4 = new IntrinsicMatrixTypeSymbol("bool2x4", "Represents a matrix containing 2 rows and 4 columns.", ScalarType.Bool, 2, 4);
            Bool3x1 = new IntrinsicMatrixTypeSymbol("bool3x1", "Represents a matrix containing 3 rows and 1 column.", ScalarType.Bool, 3, 1);
            Bool3x2 = new IntrinsicMatrixTypeSymbol("bool3x2", "Represents a matrix containing 3 rows and 2 columns.", ScalarType.Bool, 3, 2);
            Bool3x3 = new IntrinsicMatrixTypeSymbol("bool3x3", "Represents a matrix containing 3 rows and 3 columns.", ScalarType.Bool, 3, 3);
            Bool3x4 = new IntrinsicMatrixTypeSymbol("bool3x4", "Represents a matrix containing 3 rows and 4 columns.", ScalarType.Bool, 3, 4);
            Bool4x1 = new IntrinsicMatrixTypeSymbol("bool4x1", "Represents a matrix containing 4 rows and 1 column.", ScalarType.Bool, 4, 1);
            Bool4x2 = new IntrinsicMatrixTypeSymbol("bool4x2", "Represents a matrix containing 4 rows and 2 columns.", ScalarType.Bool, 4, 2);
            Bool4x3 = new IntrinsicMatrixTypeSymbol("bool4x3", "Represents a matrix containing 4 rows and 3 columns.", ScalarType.Bool, 4, 3);
            Bool4x4 = new IntrinsicMatrixTypeSymbol("bool4x4", "Represents a matrix containing 4 rows and 4 columns.", ScalarType.Bool, 4, 4);
            Int1x1 = new IntrinsicMatrixTypeSymbol("int1x1", "Represents a matrix containing 1 row and 1 column.", ScalarType.Int, 1, 1);
            Int1x2 = new IntrinsicMatrixTypeSymbol("int1x2", "Represents a matrix containing 1 row and 2 columns.", ScalarType.Int, 1, 2);
            Int1x3 = new IntrinsicMatrixTypeSymbol("int1x3", "Represents a matrix containing 1 row and 3 columns.", ScalarType.Int, 1, 3);
            Int1x4 = new IntrinsicMatrixTypeSymbol("int1x4", "Represents a matrix containing 1 row and 4 columns.", ScalarType.Int, 1, 4);
            Int2x1 = new IntrinsicMatrixTypeSymbol("int2x1", "Represents a matrix containing 2 rows and 1 column.", ScalarType.Int, 2, 1);
            Int2x2 = new IntrinsicMatrixTypeSymbol("int2x2", "Represents a matrix containing 2 rows and 2 columns.", ScalarType.Int, 2, 2);
            Int2x3 = new IntrinsicMatrixTypeSymbol("int2x3", "Represents a matrix containing 2 rows and 3 columns.", ScalarType.Int, 2, 3);
            Int2x4 = new IntrinsicMatrixTypeSymbol("int2x4", "Represents a matrix containing 2 rows and 4 columns.", ScalarType.Int, 2, 4);
            Int3x1 = new IntrinsicMatrixTypeSymbol("int3x1", "Represents a matrix containing 3 rows and 1 column.", ScalarType.Int, 3, 1);
            Int3x2 = new IntrinsicMatrixTypeSymbol("int3x2", "Represents a matrix containing 3 rows and 2 columns.", ScalarType.Int, 3, 2);
            Int3x3 = new IntrinsicMatrixTypeSymbol("int3x3", "Represents a matrix containing 3 rows and 3 columns.", ScalarType.Int, 3, 3);
            Int3x4 = new IntrinsicMatrixTypeSymbol("int3x4", "Represents a matrix containing 3 rows and 4 columns.", ScalarType.Int, 3, 4);
            Int4x1 = new IntrinsicMatrixTypeSymbol("int4x1", "Represents a matrix containing 4 rows and 1 column.", ScalarType.Int, 4, 1);
            Int4x2 = new IntrinsicMatrixTypeSymbol("int4x2", "Represents a matrix containing 4 rows and 2 columns.", ScalarType.Int, 4, 2);
            Int4x3 = new IntrinsicMatrixTypeSymbol("int4x3", "Represents a matrix containing 4 rows and 3 columns.", ScalarType.Int, 4, 3);
            Int4x4 = new IntrinsicMatrixTypeSymbol("int4x4", "Represents a matrix containing 4 rows and 4 columns.", ScalarType.Int, 4, 4);
            Uint1x1 = new IntrinsicMatrixTypeSymbol("uint1x1", "Represents a matrix containing 1 row and 1 column.", ScalarType.Uint, 1, 1);
            Uint1x2 = new IntrinsicMatrixTypeSymbol("uint1x2", "Represents a matrix containing 1 row and 2 columns.", ScalarType.Uint, 1, 2);
            Uint1x3 = new IntrinsicMatrixTypeSymbol("uint1x3", "Represents a matrix containing 1 row and 3 columns.", ScalarType.Uint, 1, 3);
            Uint1x4 = new IntrinsicMatrixTypeSymbol("uint1x4", "Represents a matrix containing 1 row and 4 columns.", ScalarType.Uint, 1, 4);
            Uint2x1 = new IntrinsicMatrixTypeSymbol("uint2x1", "Represents a matrix containing 2 rows and 1 column.", ScalarType.Uint, 2, 1);
            Uint2x2 = new IntrinsicMatrixTypeSymbol("uint2x2", "Represents a matrix containing 2 rows and 2 columns.", ScalarType.Uint, 2, 2);
            Uint2x3 = new IntrinsicMatrixTypeSymbol("uint2x3", "Represents a matrix containing 2 rows and 3 columns.", ScalarType.Uint, 2, 3);
            Uint2x4 = new IntrinsicMatrixTypeSymbol("uint2x4", "Represents a matrix containing 2 rows and 4 columns.", ScalarType.Uint, 2, 4);
            Uint3x1 = new IntrinsicMatrixTypeSymbol("uint3x1", "Represents a matrix containing 3 rows and 1 column.", ScalarType.Uint, 3, 1);
            Uint3x2 = new IntrinsicMatrixTypeSymbol("uint3x2", "Represents a matrix containing 3 rows and 2 columns.", ScalarType.Uint, 3, 2);
            Uint3x3 = new IntrinsicMatrixTypeSymbol("uint3x3", "Represents a matrix containing 3 rows and 3 columns.", ScalarType.Uint, 3, 3);
            Uint3x4 = new IntrinsicMatrixTypeSymbol("uint3x4", "Represents a matrix containing 3 rows and 4 columns.", ScalarType.Uint, 3, 4);
            Uint4x1 = new IntrinsicMatrixTypeSymbol("uint4x1", "Represents a matrix containing 4 rows and 1 column.", ScalarType.Uint, 4, 1);
            Uint4x2 = new IntrinsicMatrixTypeSymbol("uint4x2", "Represents a matrix containing 4 rows and 2 columns.", ScalarType.Uint, 4, 2);
            Uint4x3 = new IntrinsicMatrixTypeSymbol("uint4x3", "Represents a matrix containing 4 rows and 3 columns.", ScalarType.Uint, 4, 3);
            Uint4x4 = new IntrinsicMatrixTypeSymbol("uint4x4", "Represents a matrix containing 4 rows and 4 columns.", ScalarType.Uint, 4, 4);
            Half1x1 = new IntrinsicMatrixTypeSymbol("half1x1", "Represents a matrix containing 1 row and 1 column.", ScalarType.Half, 1, 1);
            Half1x2 = new IntrinsicMatrixTypeSymbol("half1x2", "Represents a matrix containing 1 row and 2 columns.", ScalarType.Half, 1, 2);
            Half1x3 = new IntrinsicMatrixTypeSymbol("half1x3", "Represents a matrix containing 1 row and 3 columns.", ScalarType.Half, 1, 3);
            Half1x4 = new IntrinsicMatrixTypeSymbol("half1x4", "Represents a matrix containing 1 row and 4 columns.", ScalarType.Half, 1, 4);
            Half2x1 = new IntrinsicMatrixTypeSymbol("half2x1", "Represents a matrix containing 2 rows and 1 column.", ScalarType.Half, 2, 1);
            Half2x2 = new IntrinsicMatrixTypeSymbol("half2x2", "Represents a matrix containing 2 rows and 2 columns.", ScalarType.Half, 2, 2);
            Half2x3 = new IntrinsicMatrixTypeSymbol("half2x3", "Represents a matrix containing 2 rows and 3 columns.", ScalarType.Half, 2, 3);
            Half2x4 = new IntrinsicMatrixTypeSymbol("half2x4", "Represents a matrix containing 2 rows and 4 columns.", ScalarType.Half, 2, 4);
            Half3x1 = new IntrinsicMatrixTypeSymbol("half3x1", "Represents a matrix containing 3 rows and 1 column.", ScalarType.Half, 3, 1);
            Half3x2 = new IntrinsicMatrixTypeSymbol("half3x2", "Represents a matrix containing 3 rows and 2 columns.", ScalarType.Half, 3, 2);
            Half3x3 = new IntrinsicMatrixTypeSymbol("half3x3", "Represents a matrix containing 3 rows and 3 columns.", ScalarType.Half, 3, 3);
            Half3x4 = new IntrinsicMatrixTypeSymbol("half3x4", "Represents a matrix containing 3 rows and 4 columns.", ScalarType.Half, 3, 4);
            Half4x1 = new IntrinsicMatrixTypeSymbol("half4x1", "Represents a matrix containing 4 rows and 1 column.", ScalarType.Half, 4, 1);
            Half4x2 = new IntrinsicMatrixTypeSymbol("half4x2", "Represents a matrix containing 4 rows and 2 columns.", ScalarType.Half, 4, 2);
            Half4x3 = new IntrinsicMatrixTypeSymbol("half4x3", "Represents a matrix containing 4 rows and 3 columns.", ScalarType.Half, 4, 3);
            Half4x4 = new IntrinsicMatrixTypeSymbol("half4x4", "Represents a matrix containing 4 rows and 4 columns.", ScalarType.Half, 4, 4);
            Float1x1 = new IntrinsicMatrixTypeSymbol("float1x1", "Represents a matrix containing 1 row and 1 column.", ScalarType.Float, 1, 1);
            Float1x2 = new IntrinsicMatrixTypeSymbol("float1x2", "Represents a matrix containing 1 row and 2 columns.", ScalarType.Float, 1, 2);
            Float1x3 = new IntrinsicMatrixTypeSymbol("float1x3", "Represents a matrix containing 1 row and 3 columns.", ScalarType.Float, 1, 3);
            Float1x4 = new IntrinsicMatrixTypeSymbol("float1x4", "Represents a matrix containing 1 row and 4 columns.", ScalarType.Float, 1, 4);
            Float2x1 = new IntrinsicMatrixTypeSymbol("float2x1", "Represents a matrix containing 2 rows and 1 column.", ScalarType.Float, 2, 1);
            Float2x2 = new IntrinsicMatrixTypeSymbol("float2x2", "Represents a matrix containing 2 rows and 2 columns.", ScalarType.Float, 2, 2);
            Float2x3 = new IntrinsicMatrixTypeSymbol("float2x3", "Represents a matrix containing 2 rows and 3 columns.", ScalarType.Float, 2, 3);
            Float2x4 = new IntrinsicMatrixTypeSymbol("float2x4", "Represents a matrix containing 2 rows and 4 columns.", ScalarType.Float, 2, 4);
            Float3x1 = new IntrinsicMatrixTypeSymbol("float3x1", "Represents a matrix containing 3 rows and 1 column.", ScalarType.Float, 3, 1);
            Float3x2 = new IntrinsicMatrixTypeSymbol("float3x2", "Represents a matrix containing 3 rows and 2 columns.", ScalarType.Float, 3, 2);
            Float3x3 = new IntrinsicMatrixTypeSymbol("float3x3", "Represents a matrix containing 3 rows and 3 columns.", ScalarType.Float, 3, 3);
            Float3x4 = new IntrinsicMatrixTypeSymbol("float3x4", "Represents a matrix containing 3 rows and 4 columns.", ScalarType.Float, 3, 4);
            Float4x1 = new IntrinsicMatrixTypeSymbol("float4x1", "Represents a matrix containing 4 rows and 1 column.", ScalarType.Float, 4, 1);
            Float4x2 = new IntrinsicMatrixTypeSymbol("float4x2", "Represents a matrix containing 4 rows and 2 columns.", ScalarType.Float, 4, 2);
            Float4x3 = new IntrinsicMatrixTypeSymbol("float4x3", "Represents a matrix containing 4 rows and 3 columns.", ScalarType.Float, 4, 3);
            Float4x4 = new IntrinsicMatrixTypeSymbol("float4x4", "Represents a matrix containing 4 rows and 4 columns.", ScalarType.Float, 4, 4);
            Double1x1 = new IntrinsicMatrixTypeSymbol("double1x1", "Represents a matrix containing 1 row and 1 column.", ScalarType.Double, 1, 1);
            Double1x2 = new IntrinsicMatrixTypeSymbol("double1x2", "Represents a matrix containing 1 row and 2 columns.", ScalarType.Double, 1, 2);
            Double1x3 = new IntrinsicMatrixTypeSymbol("double1x3", "Represents a matrix containing 1 row and 3 columns.", ScalarType.Double, 1, 3);
            Double1x4 = new IntrinsicMatrixTypeSymbol("double1x4", "Represents a matrix containing 1 row and 4 columns.", ScalarType.Double, 1, 4);
            Double2x1 = new IntrinsicMatrixTypeSymbol("double2x1", "Represents a matrix containing 2 rows and 1 column.", ScalarType.Double, 2, 1);
            Double2x2 = new IntrinsicMatrixTypeSymbol("double2x2", "Represents a matrix containing 2 rows and 2 columns.", ScalarType.Double, 2, 2);
            Double2x3 = new IntrinsicMatrixTypeSymbol("double2x3", "Represents a matrix containing 2 rows and 3 columns.", ScalarType.Double, 2, 3);
            Double2x4 = new IntrinsicMatrixTypeSymbol("double2x4", "Represents a matrix containing 2 rows and 4 columns.", ScalarType.Double, 2, 4);
            Double3x1 = new IntrinsicMatrixTypeSymbol("double3x1", "Represents a matrix containing 3 rows and 1 column.", ScalarType.Double, 3, 1);
            Double3x2 = new IntrinsicMatrixTypeSymbol("double3x2", "Represents a matrix containing 3 rows and 2 columns.", ScalarType.Double, 3, 2);
            Double3x3 = new IntrinsicMatrixTypeSymbol("double3x3", "Represents a matrix containing 3 rows and 3 columns.", ScalarType.Double, 3, 3);
            Double3x4 = new IntrinsicMatrixTypeSymbol("double3x4", "Represents a matrix containing 3 rows and 4 columns.", ScalarType.Double, 3, 4);
            Double4x1 = new IntrinsicMatrixTypeSymbol("double4x1", "Represents a matrix containing 4 rows and 1 column.", ScalarType.Double, 4, 1);
            Double4x2 = new IntrinsicMatrixTypeSymbol("double4x2", "Represents a matrix containing 4 rows and 2 columns.", ScalarType.Double, 4, 2);
            Double4x3 = new IntrinsicMatrixTypeSymbol("double4x3", "Represents a matrix containing 4 rows and 3 columns.", ScalarType.Double, 4, 3);
            Double4x4 = new IntrinsicMatrixTypeSymbol("double4x4", "Represents a matrix containing 4 rows and 4 columns.", ScalarType.Double, 4, 4);
            Min16Float1x1 = new IntrinsicMatrixTypeSymbol("min16float1x1", "Represents a matrix containing 1 row and 1 column.", ScalarType.Min16Float, 1, 1);
            Min16Float1x2 = new IntrinsicMatrixTypeSymbol("min16float1x2", "Represents a matrix containing 1 row and 2 columns.", ScalarType.Min16Float, 1, 2);
            Min16Float1x3 = new IntrinsicMatrixTypeSymbol("min16float1x3", "Represents a matrix containing 1 row and 3 columns.", ScalarType.Min16Float, 1, 3);
            Min16Float1x4 = new IntrinsicMatrixTypeSymbol("min16float1x4", "Represents a matrix containing 1 row and 4 columns.", ScalarType.Min16Float, 1, 4);
            Min16Float2x1 = new IntrinsicMatrixTypeSymbol("min16float2x1", "Represents a matrix containing 2 rows and 1 column.", ScalarType.Min16Float, 2, 1);
            Min16Float2x2 = new IntrinsicMatrixTypeSymbol("min16float2x2", "Represents a matrix containing 2 rows and 2 columns.", ScalarType.Min16Float, 2, 2);
            Min16Float2x3 = new IntrinsicMatrixTypeSymbol("min16float2x3", "Represents a matrix containing 2 rows and 3 columns.", ScalarType.Min16Float, 2, 3);
            Min16Float2x4 = new IntrinsicMatrixTypeSymbol("min16float2x4", "Represents a matrix containing 2 rows and 4 columns.", ScalarType.Min16Float, 2, 4);
            Min16Float3x1 = new IntrinsicMatrixTypeSymbol("min16float3x1", "Represents a matrix containing 3 rows and 1 column.", ScalarType.Min16Float, 3, 1);
            Min16Float3x2 = new IntrinsicMatrixTypeSymbol("min16float3x2", "Represents a matrix containing 3 rows and 2 columns.", ScalarType.Min16Float, 3, 2);
            Min16Float3x3 = new IntrinsicMatrixTypeSymbol("min16float3x3", "Represents a matrix containing 3 rows and 3 columns.", ScalarType.Min16Float, 3, 3);
            Min16Float3x4 = new IntrinsicMatrixTypeSymbol("min16float3x4", "Represents a matrix containing 3 rows and 4 columns.", ScalarType.Min16Float, 3, 4);
            Min16Float4x1 = new IntrinsicMatrixTypeSymbol("min16float4x1", "Represents a matrix containing 4 rows and 1 column.", ScalarType.Min16Float, 4, 1);
            Min16Float4x2 = new IntrinsicMatrixTypeSymbol("min16float4x2", "Represents a matrix containing 4 rows and 2 columns.", ScalarType.Min16Float, 4, 2);
            Min16Float4x3 = new IntrinsicMatrixTypeSymbol("min16float4x3", "Represents a matrix containing 4 rows and 3 columns.", ScalarType.Min16Float, 4, 3);
            Min16Float4x4 = new IntrinsicMatrixTypeSymbol("min16float4x4", "Represents a matrix containing 4 rows and 4 columns.", ScalarType.Min16Float, 4, 4);
            Min10Float1x1 = new IntrinsicMatrixTypeSymbol("min10float1x1", "Represents a matrix containing 1 row and 1 column.", ScalarType.Min10Float, 1, 1);
            Min10Float1x2 = new IntrinsicMatrixTypeSymbol("min10float1x2", "Represents a matrix containing 1 row and 2 columns.", ScalarType.Min10Float, 1, 2);
            Min10Float1x3 = new IntrinsicMatrixTypeSymbol("min10float1x3", "Represents a matrix containing 1 row and 3 columns.", ScalarType.Min10Float, 1, 3);
            Min10Float1x4 = new IntrinsicMatrixTypeSymbol("min10float1x4", "Represents a matrix containing 1 row and 4 columns.", ScalarType.Min10Float, 1, 4);
            Min10Float2x1 = new IntrinsicMatrixTypeSymbol("min10float2x1", "Represents a matrix containing 2 rows and 1 column.", ScalarType.Min10Float, 2, 1);
            Min10Float2x2 = new IntrinsicMatrixTypeSymbol("min10float2x2", "Represents a matrix containing 2 rows and 2 columns.", ScalarType.Min10Float, 2, 2);
            Min10Float2x3 = new IntrinsicMatrixTypeSymbol("min10float2x3", "Represents a matrix containing 2 rows and 3 columns.", ScalarType.Min10Float, 2, 3);
            Min10Float2x4 = new IntrinsicMatrixTypeSymbol("min10float2x4", "Represents a matrix containing 2 rows and 4 columns.", ScalarType.Min10Float, 2, 4);
            Min10Float3x1 = new IntrinsicMatrixTypeSymbol("min10float3x1", "Represents a matrix containing 3 rows and 1 column.", ScalarType.Min10Float, 3, 1);
            Min10Float3x2 = new IntrinsicMatrixTypeSymbol("min10float3x2", "Represents a matrix containing 3 rows and 2 columns.", ScalarType.Min10Float, 3, 2);
            Min10Float3x3 = new IntrinsicMatrixTypeSymbol("min10float3x3", "Represents a matrix containing 3 rows and 3 columns.", ScalarType.Min10Float, 3, 3);
            Min10Float3x4 = new IntrinsicMatrixTypeSymbol("min10float3x4", "Represents a matrix containing 3 rows and 4 columns.", ScalarType.Min10Float, 3, 4);
            Min10Float4x1 = new IntrinsicMatrixTypeSymbol("min10float4x1", "Represents a matrix containing 4 rows and 1 column.", ScalarType.Min10Float, 4, 1);
            Min10Float4x2 = new IntrinsicMatrixTypeSymbol("min10float4x2", "Represents a matrix containing 4 rows and 2 columns.", ScalarType.Min10Float, 4, 2);
            Min10Float4x3 = new IntrinsicMatrixTypeSymbol("min10float4x3", "Represents a matrix containing 4 rows and 3 columns.", ScalarType.Min10Float, 4, 3);
            Min10Float4x4 = new IntrinsicMatrixTypeSymbol("min10float4x4", "Represents a matrix containing 4 rows and 4 columns.", ScalarType.Min10Float, 4, 4);
            Min16Int1x1 = new IntrinsicMatrixTypeSymbol("min16int1x1", "Represents a matrix containing 1 row and 1 column.", ScalarType.Min16Int, 1, 1);
            Min16Int1x2 = new IntrinsicMatrixTypeSymbol("min16int1x2", "Represents a matrix containing 1 row and 2 columns.", ScalarType.Min16Int, 1, 2);
            Min16Int1x3 = new IntrinsicMatrixTypeSymbol("min16int1x3", "Represents a matrix containing 1 row and 3 columns.", ScalarType.Min16Int, 1, 3);
            Min16Int1x4 = new IntrinsicMatrixTypeSymbol("min16int1x4", "Represents a matrix containing 1 row and 4 columns.", ScalarType.Min16Int, 1, 4);
            Min16Int2x1 = new IntrinsicMatrixTypeSymbol("min16int2x1", "Represents a matrix containing 2 rows and 1 column.", ScalarType.Min16Int, 2, 1);
            Min16Int2x2 = new IntrinsicMatrixTypeSymbol("min16int2x2", "Represents a matrix containing 2 rows and 2 columns.", ScalarType.Min16Int, 2, 2);
            Min16Int2x3 = new IntrinsicMatrixTypeSymbol("min16int2x3", "Represents a matrix containing 2 rows and 3 columns.", ScalarType.Min16Int, 2, 3);
            Min16Int2x4 = new IntrinsicMatrixTypeSymbol("min16int2x4", "Represents a matrix containing 2 rows and 4 columns.", ScalarType.Min16Int, 2, 4);
            Min16Int3x1 = new IntrinsicMatrixTypeSymbol("min16int3x1", "Represents a matrix containing 3 rows and 1 column.", ScalarType.Min16Int, 3, 1);
            Min16Int3x2 = new IntrinsicMatrixTypeSymbol("min16int3x2", "Represents a matrix containing 3 rows and 2 columns.", ScalarType.Min16Int, 3, 2);
            Min16Int3x3 = new IntrinsicMatrixTypeSymbol("min16int3x3", "Represents a matrix containing 3 rows and 3 columns.", ScalarType.Min16Int, 3, 3);
            Min16Int3x4 = new IntrinsicMatrixTypeSymbol("min16int3x4", "Represents a matrix containing 3 rows and 4 columns.", ScalarType.Min16Int, 3, 4);
            Min16Int4x1 = new IntrinsicMatrixTypeSymbol("min16int4x1", "Represents a matrix containing 4 rows and 1 column.", ScalarType.Min16Int, 4, 1);
            Min16Int4x2 = new IntrinsicMatrixTypeSymbol("min16int4x2", "Represents a matrix containing 4 rows and 2 columns.", ScalarType.Min16Int, 4, 2);
            Min16Int4x3 = new IntrinsicMatrixTypeSymbol("min16int4x3", "Represents a matrix containing 4 rows and 3 columns.", ScalarType.Min16Int, 4, 3);
            Min16Int4x4 = new IntrinsicMatrixTypeSymbol("min16int4x4", "Represents a matrix containing 4 rows and 4 columns.", ScalarType.Min16Int, 4, 4);
            Min12Int1x1 = new IntrinsicMatrixTypeSymbol("min12int1x1", "Represents a matrix containing 1 row and 1 column.", ScalarType.Min12Int, 1, 1);
            Min12Int1x2 = new IntrinsicMatrixTypeSymbol("min12int1x2", "Represents a matrix containing 1 row and 2 columns.", ScalarType.Min12Int, 1, 2);
            Min12Int1x3 = new IntrinsicMatrixTypeSymbol("min12int1x3", "Represents a matrix containing 1 row and 3 columns.", ScalarType.Min12Int, 1, 3);
            Min12Int1x4 = new IntrinsicMatrixTypeSymbol("min12int1x4", "Represents a matrix containing 1 row and 4 columns.", ScalarType.Min12Int, 1, 4);
            Min12Int2x1 = new IntrinsicMatrixTypeSymbol("min12int2x1", "Represents a matrix containing 2 rows and 1 column.", ScalarType.Min12Int, 2, 1);
            Min12Int2x2 = new IntrinsicMatrixTypeSymbol("min12int2x2", "Represents a matrix containing 2 rows and 2 columns.", ScalarType.Min12Int, 2, 2);
            Min12Int2x3 = new IntrinsicMatrixTypeSymbol("min12int2x3", "Represents a matrix containing 2 rows and 3 columns.", ScalarType.Min12Int, 2, 3);
            Min12Int2x4 = new IntrinsicMatrixTypeSymbol("min12int2x4", "Represents a matrix containing 2 rows and 4 columns.", ScalarType.Min12Int, 2, 4);
            Min12Int3x1 = new IntrinsicMatrixTypeSymbol("min12int3x1", "Represents a matrix containing 3 rows and 1 column.", ScalarType.Min12Int, 3, 1);
            Min12Int3x2 = new IntrinsicMatrixTypeSymbol("min12int3x2", "Represents a matrix containing 3 rows and 2 columns.", ScalarType.Min12Int, 3, 2);
            Min12Int3x3 = new IntrinsicMatrixTypeSymbol("min12int3x3", "Represents a matrix containing 3 rows and 3 columns.", ScalarType.Min12Int, 3, 3);
            Min12Int3x4 = new IntrinsicMatrixTypeSymbol("min12int3x4", "Represents a matrix containing 3 rows and 4 columns.", ScalarType.Min12Int, 3, 4);
            Min12Int4x1 = new IntrinsicMatrixTypeSymbol("min12int4x1", "Represents a matrix containing 4 rows and 1 column.", ScalarType.Min12Int, 4, 1);
            Min12Int4x2 = new IntrinsicMatrixTypeSymbol("min12int4x2", "Represents a matrix containing 4 rows and 2 columns.", ScalarType.Min12Int, 4, 2);
            Min12Int4x3 = new IntrinsicMatrixTypeSymbol("min12int4x3", "Represents a matrix containing 4 rows and 3 columns.", ScalarType.Min12Int, 4, 3);
            Min12Int4x4 = new IntrinsicMatrixTypeSymbol("min12int4x4", "Represents a matrix containing 4 rows and 4 columns.", ScalarType.Min12Int, 4, 4);
            Min16Uint1x1 = new IntrinsicMatrixTypeSymbol("min16uint1x1", "Represents a matrix containing 1 row and 1 column.", ScalarType.Min16Uint, 1, 1);
            Min16Uint1x2 = new IntrinsicMatrixTypeSymbol("min16uint1x2", "Represents a matrix containing 1 row and 2 columns.", ScalarType.Min16Uint, 1, 2);
            Min16Uint1x3 = new IntrinsicMatrixTypeSymbol("min16uint1x3", "Represents a matrix containing 1 row and 3 columns.", ScalarType.Min16Uint, 1, 3);
            Min16Uint1x4 = new IntrinsicMatrixTypeSymbol("min16uint1x4", "Represents a matrix containing 1 row and 4 columns.", ScalarType.Min16Uint, 1, 4);
            Min16Uint2x1 = new IntrinsicMatrixTypeSymbol("min16uint2x1", "Represents a matrix containing 2 rows and 1 column.", ScalarType.Min16Uint, 2, 1);
            Min16Uint2x2 = new IntrinsicMatrixTypeSymbol("min16uint2x2", "Represents a matrix containing 2 rows and 2 columns.", ScalarType.Min16Uint, 2, 2);
            Min16Uint2x3 = new IntrinsicMatrixTypeSymbol("min16uint2x3", "Represents a matrix containing 2 rows and 3 columns.", ScalarType.Min16Uint, 2, 3);
            Min16Uint2x4 = new IntrinsicMatrixTypeSymbol("min16uint2x4", "Represents a matrix containing 2 rows and 4 columns.", ScalarType.Min16Uint, 2, 4);
            Min16Uint3x1 = new IntrinsicMatrixTypeSymbol("min16uint3x1", "Represents a matrix containing 3 rows and 1 column.", ScalarType.Min16Uint, 3, 1);
            Min16Uint3x2 = new IntrinsicMatrixTypeSymbol("min16uint3x2", "Represents a matrix containing 3 rows and 2 columns.", ScalarType.Min16Uint, 3, 2);
            Min16Uint3x3 = new IntrinsicMatrixTypeSymbol("min16uint3x3", "Represents a matrix containing 3 rows and 3 columns.", ScalarType.Min16Uint, 3, 3);
            Min16Uint3x4 = new IntrinsicMatrixTypeSymbol("min16uint3x4", "Represents a matrix containing 3 rows and 4 columns.", ScalarType.Min16Uint, 3, 4);
            Min16Uint4x1 = new IntrinsicMatrixTypeSymbol("min16uint4x1", "Represents a matrix containing 4 rows and 1 column.", ScalarType.Min16Uint, 4, 1);
            Min16Uint4x2 = new IntrinsicMatrixTypeSymbol("min16uint4x2", "Represents a matrix containing 4 rows and 2 columns.", ScalarType.Min16Uint, 4, 2);
            Min16Uint4x3 = new IntrinsicMatrixTypeSymbol("min16uint4x3", "Represents a matrix containing 4 rows and 3 columns.", ScalarType.Min16Uint, 4, 3);
            Min16Uint4x4 = new IntrinsicMatrixTypeSymbol("min16uint4x4", "Represents a matrix containing 4 rows and 4 columns.", ScalarType.Min16Uint, 4, 4);

            Bool1x1.AddMembers(CreateMatrixTypeMembers(Bool1x1, Bool1, Bool2, Bool3, Bool4));
            Bool1x2.AddMembers(CreateMatrixTypeMembers(Bool1x2, Bool1, Bool2, Bool3, Bool4));
            Bool1x3.AddMembers(CreateMatrixTypeMembers(Bool1x3, Bool1, Bool2, Bool3, Bool4));
            Bool1x4.AddMembers(CreateMatrixTypeMembers(Bool1x4, Bool1, Bool2, Bool3, Bool4));
            Bool2x1.AddMembers(CreateMatrixTypeMembers(Bool2x1, Bool1, Bool2, Bool3, Bool4));
            Bool2x2.AddMembers(CreateMatrixTypeMembers(Bool2x2, Bool1, Bool2, Bool3, Bool4));
            Bool2x3.AddMembers(CreateMatrixTypeMembers(Bool2x3, Bool1, Bool2, Bool3, Bool4));
            Bool2x4.AddMembers(CreateMatrixTypeMembers(Bool2x4, Bool1, Bool2, Bool3, Bool4));
            Bool3x1.AddMembers(CreateMatrixTypeMembers(Bool3x1, Bool1, Bool2, Bool3, Bool4));
            Bool3x2.AddMembers(CreateMatrixTypeMembers(Bool3x2, Bool1, Bool2, Bool3, Bool4));
            Bool3x3.AddMembers(CreateMatrixTypeMembers(Bool3x3, Bool1, Bool2, Bool3, Bool4));
            Bool3x4.AddMembers(CreateMatrixTypeMembers(Bool3x4, Bool1, Bool2, Bool3, Bool4));
            Bool4x1.AddMembers(CreateMatrixTypeMembers(Bool4x1, Bool1, Bool2, Bool3, Bool4));
            Bool4x2.AddMembers(CreateMatrixTypeMembers(Bool4x2, Bool1, Bool2, Bool3, Bool4));
            Bool4x3.AddMembers(CreateMatrixTypeMembers(Bool4x3, Bool1, Bool2, Bool3, Bool4));
            Bool4x4.AddMembers(CreateMatrixTypeMembers(Bool4x4, Bool1, Bool2, Bool3, Bool4));
            Int1x1.AddMembers(CreateMatrixTypeMembers(Int1x1, Int1, Int2, Int3, Int4));
            Int1x2.AddMembers(CreateMatrixTypeMembers(Int1x2, Int1, Int2, Int3, Int4));
            Int1x3.AddMembers(CreateMatrixTypeMembers(Int1x3, Int1, Int2, Int3, Int4));
            Int1x4.AddMembers(CreateMatrixTypeMembers(Int1x4, Int1, Int2, Int3, Int4));
            Int2x1.AddMembers(CreateMatrixTypeMembers(Int2x1, Int1, Int2, Int3, Int4));
            Int2x2.AddMembers(CreateMatrixTypeMembers(Int2x2, Int1, Int2, Int3, Int4));
            Int2x3.AddMembers(CreateMatrixTypeMembers(Int2x3, Int1, Int2, Int3, Int4));
            Int2x4.AddMembers(CreateMatrixTypeMembers(Int2x4, Int1, Int2, Int3, Int4));
            Int3x1.AddMembers(CreateMatrixTypeMembers(Int3x1, Int1, Int2, Int3, Int4));
            Int3x2.AddMembers(CreateMatrixTypeMembers(Int3x2, Int1, Int2, Int3, Int4));
            Int3x3.AddMembers(CreateMatrixTypeMembers(Int3x3, Int1, Int2, Int3, Int4));
            Int3x4.AddMembers(CreateMatrixTypeMembers(Int3x4, Int1, Int2, Int3, Int4));
            Int4x1.AddMembers(CreateMatrixTypeMembers(Int4x1, Int1, Int2, Int3, Int4));
            Int4x2.AddMembers(CreateMatrixTypeMembers(Int4x2, Int1, Int2, Int3, Int4));
            Int4x3.AddMembers(CreateMatrixTypeMembers(Int4x3, Int1, Int2, Int3, Int4));
            Int4x4.AddMembers(CreateMatrixTypeMembers(Int4x4, Int1, Int2, Int3, Int4));
            Uint1x1.AddMembers(CreateMatrixTypeMembers(Uint1x1, Uint1, Uint2, Uint3, Uint4));
            Uint1x2.AddMembers(CreateMatrixTypeMembers(Uint1x2, Uint1, Uint2, Uint3, Uint4));
            Uint1x3.AddMembers(CreateMatrixTypeMembers(Uint1x3, Uint1, Uint2, Uint3, Uint4));
            Uint1x4.AddMembers(CreateMatrixTypeMembers(Uint1x4, Uint1, Uint2, Uint3, Uint4));
            Uint2x1.AddMembers(CreateMatrixTypeMembers(Uint2x1, Uint1, Uint2, Uint3, Uint4));
            Uint2x2.AddMembers(CreateMatrixTypeMembers(Uint2x2, Uint1, Uint2, Uint3, Uint4));
            Uint2x3.AddMembers(CreateMatrixTypeMembers(Uint2x3, Uint1, Uint2, Uint3, Uint4));
            Uint2x4.AddMembers(CreateMatrixTypeMembers(Uint2x4, Uint1, Uint2, Uint3, Uint4));
            Uint3x1.AddMembers(CreateMatrixTypeMembers(Uint3x1, Uint1, Uint2, Uint3, Uint4));
            Uint3x2.AddMembers(CreateMatrixTypeMembers(Uint3x2, Uint1, Uint2, Uint3, Uint4));
            Uint3x3.AddMembers(CreateMatrixTypeMembers(Uint3x3, Uint1, Uint2, Uint3, Uint4));
            Uint3x4.AddMembers(CreateMatrixTypeMembers(Uint3x4, Uint1, Uint2, Uint3, Uint4));
            Uint4x1.AddMembers(CreateMatrixTypeMembers(Uint4x1, Uint1, Uint2, Uint3, Uint4));
            Uint4x2.AddMembers(CreateMatrixTypeMembers(Uint4x2, Uint1, Uint2, Uint3, Uint4));
            Uint4x3.AddMembers(CreateMatrixTypeMembers(Uint4x3, Uint1, Uint2, Uint3, Uint4));
            Uint4x4.AddMembers(CreateMatrixTypeMembers(Uint4x4, Uint1, Uint2, Uint3, Uint4));
            Half1x1.AddMembers(CreateMatrixTypeMembers(Half1x1, Half1, Half2, Half3, Half4));
            Half1x2.AddMembers(CreateMatrixTypeMembers(Half1x2, Half1, Half2, Half3, Half4));
            Half1x3.AddMembers(CreateMatrixTypeMembers(Half1x3, Half1, Half2, Half3, Half4));
            Half1x4.AddMembers(CreateMatrixTypeMembers(Half1x4, Half1, Half2, Half3, Half4));
            Half2x1.AddMembers(CreateMatrixTypeMembers(Half2x1, Half1, Half2, Half3, Half4));
            Half2x2.AddMembers(CreateMatrixTypeMembers(Half2x2, Half1, Half2, Half3, Half4));
            Half2x3.AddMembers(CreateMatrixTypeMembers(Half2x3, Half1, Half2, Half3, Half4));
            Half2x4.AddMembers(CreateMatrixTypeMembers(Half2x4, Half1, Half2, Half3, Half4));
            Half3x1.AddMembers(CreateMatrixTypeMembers(Half3x1, Half1, Half2, Half3, Half4));
            Half3x2.AddMembers(CreateMatrixTypeMembers(Half3x2, Half1, Half2, Half3, Half4));
            Half3x3.AddMembers(CreateMatrixTypeMembers(Half3x3, Half1, Half2, Half3, Half4));
            Half3x4.AddMembers(CreateMatrixTypeMembers(Half3x4, Half1, Half2, Half3, Half4));
            Half4x1.AddMembers(CreateMatrixTypeMembers(Half4x1, Half1, Half2, Half3, Half4));
            Half4x2.AddMembers(CreateMatrixTypeMembers(Half4x2, Half1, Half2, Half3, Half4));
            Half4x3.AddMembers(CreateMatrixTypeMembers(Half4x3, Half1, Half2, Half3, Half4));
            Half4x4.AddMembers(CreateMatrixTypeMembers(Half4x4, Half1, Half2, Half3, Half4));
            Float1x1.AddMembers(CreateMatrixTypeMembers(Float1x1, Float1, Float2, Float3, Float4));
            Float1x2.AddMembers(CreateMatrixTypeMembers(Float1x2, Float1, Float2, Float3, Float4));
            Float1x3.AddMembers(CreateMatrixTypeMembers(Float1x3, Float1, Float2, Float3, Float4));
            Float1x4.AddMembers(CreateMatrixTypeMembers(Float1x4, Float1, Float2, Float3, Float4));
            Float2x1.AddMembers(CreateMatrixTypeMembers(Float2x1, Float1, Float2, Float3, Float4));
            Float2x2.AddMembers(CreateMatrixTypeMembers(Float2x2, Float1, Float2, Float3, Float4));
            Float2x3.AddMembers(CreateMatrixTypeMembers(Float2x3, Float1, Float2, Float3, Float4));
            Float2x4.AddMembers(CreateMatrixTypeMembers(Float2x4, Float1, Float2, Float3, Float4));
            Float3x1.AddMembers(CreateMatrixTypeMembers(Float3x1, Float1, Float2, Float3, Float4));
            Float3x2.AddMembers(CreateMatrixTypeMembers(Float3x2, Float1, Float2, Float3, Float4));
            Float3x3.AddMembers(CreateMatrixTypeMembers(Float3x3, Float1, Float2, Float3, Float4));
            Float3x4.AddMembers(CreateMatrixTypeMembers(Float3x4, Float1, Float2, Float3, Float4));
            Float4x1.AddMembers(CreateMatrixTypeMembers(Float4x1, Float1, Float2, Float3, Float4));
            Float4x2.AddMembers(CreateMatrixTypeMembers(Float4x2, Float1, Float2, Float3, Float4));
            Float4x3.AddMembers(CreateMatrixTypeMembers(Float4x3, Float1, Float2, Float3, Float4));
            Float4x4.AddMembers(CreateMatrixTypeMembers(Float4x4, Float1, Float2, Float3, Float4));
            Double1x1.AddMembers(CreateMatrixTypeMembers(Double1x1, Double1, Double2, Double3, Double4));
            Double1x2.AddMembers(CreateMatrixTypeMembers(Double1x2, Double1, Double2, Double3, Double4));
            Double1x3.AddMembers(CreateMatrixTypeMembers(Double1x3, Double1, Double2, Double3, Double4));
            Double1x4.AddMembers(CreateMatrixTypeMembers(Double1x4, Double1, Double2, Double3, Double4));
            Double2x1.AddMembers(CreateMatrixTypeMembers(Double2x1, Double1, Double2, Double3, Double4));
            Double2x2.AddMembers(CreateMatrixTypeMembers(Double2x2, Double1, Double2, Double3, Double4));
            Double2x3.AddMembers(CreateMatrixTypeMembers(Double2x3, Double1, Double2, Double3, Double4));
            Double2x4.AddMembers(CreateMatrixTypeMembers(Double2x4, Double1, Double2, Double3, Double4));
            Double3x1.AddMembers(CreateMatrixTypeMembers(Double3x1, Double1, Double2, Double3, Double4));
            Double3x2.AddMembers(CreateMatrixTypeMembers(Double3x2, Double1, Double2, Double3, Double4));
            Double3x3.AddMembers(CreateMatrixTypeMembers(Double3x3, Double1, Double2, Double3, Double4));
            Double3x4.AddMembers(CreateMatrixTypeMembers(Double3x4, Double1, Double2, Double3, Double4));
            Double4x1.AddMembers(CreateMatrixTypeMembers(Double4x1, Double1, Double2, Double3, Double4));
            Double4x2.AddMembers(CreateMatrixTypeMembers(Double4x2, Double1, Double2, Double3, Double4));
            Double4x3.AddMembers(CreateMatrixTypeMembers(Double4x3, Double1, Double2, Double3, Double4));
            Double4x4.AddMembers(CreateMatrixTypeMembers(Double4x4, Double1, Double2, Double3, Double4));
            Min16Float1x1.AddMembers(CreateMatrixTypeMembers(Min16Float1x1, Min16Float1, Min16Float2, Min16Float3, Min16Float4));
            Min16Float1x2.AddMembers(CreateMatrixTypeMembers(Min16Float1x2, Min16Float1, Min16Float2, Min16Float3, Min16Float4));
            Min16Float1x3.AddMembers(CreateMatrixTypeMembers(Min16Float1x3, Min16Float1, Min16Float2, Min16Float3, Min16Float4));
            Min16Float1x4.AddMembers(CreateMatrixTypeMembers(Min16Float1x4, Min16Float1, Min16Float2, Min16Float3, Min16Float4));
            Min16Float2x1.AddMembers(CreateMatrixTypeMembers(Min16Float2x1, Min16Float1, Min16Float2, Min16Float3, Min16Float4));
            Min16Float2x2.AddMembers(CreateMatrixTypeMembers(Min16Float2x2, Min16Float1, Min16Float2, Min16Float3, Min16Float4));
            Min16Float2x3.AddMembers(CreateMatrixTypeMembers(Min16Float2x3, Min16Float1, Min16Float2, Min16Float3, Min16Float4));
            Min16Float2x4.AddMembers(CreateMatrixTypeMembers(Min16Float2x4, Min16Float1, Min16Float2, Min16Float3, Min16Float4));
            Min16Float3x1.AddMembers(CreateMatrixTypeMembers(Min16Float3x1, Min16Float1, Min16Float2, Min16Float3, Min16Float4));
            Min16Float3x2.AddMembers(CreateMatrixTypeMembers(Min16Float3x2, Min16Float1, Min16Float2, Min16Float3, Min16Float4));
            Min16Float3x3.AddMembers(CreateMatrixTypeMembers(Min16Float3x3, Min16Float1, Min16Float2, Min16Float3, Min16Float4));
            Min16Float3x4.AddMembers(CreateMatrixTypeMembers(Min16Float3x4, Min16Float1, Min16Float2, Min16Float3, Min16Float4));
            Min16Float4x1.AddMembers(CreateMatrixTypeMembers(Min16Float4x1, Min16Float1, Min16Float2, Min16Float3, Min16Float4));
            Min16Float4x2.AddMembers(CreateMatrixTypeMembers(Min16Float4x2, Min16Float1, Min16Float2, Min16Float3, Min16Float4));
            Min16Float4x3.AddMembers(CreateMatrixTypeMembers(Min16Float4x3, Min16Float1, Min16Float2, Min16Float3, Min16Float4));
            Min16Float4x4.AddMembers(CreateMatrixTypeMembers(Min16Float4x4, Min16Float1, Min16Float2, Min16Float3, Min16Float4));
            Min10Float1x1.AddMembers(CreateMatrixTypeMembers(Min10Float1x1, Min10Float1, Min10Float2, Min10Float3, Min10Float4));
            Min10Float1x2.AddMembers(CreateMatrixTypeMembers(Min10Float1x2, Min10Float1, Min10Float2, Min10Float3, Min10Float4));
            Min10Float1x3.AddMembers(CreateMatrixTypeMembers(Min10Float1x3, Min10Float1, Min10Float2, Min10Float3, Min10Float4));
            Min10Float1x4.AddMembers(CreateMatrixTypeMembers(Min10Float1x4, Min10Float1, Min10Float2, Min10Float3, Min10Float4));
            Min10Float2x1.AddMembers(CreateMatrixTypeMembers(Min10Float2x1, Min10Float1, Min10Float2, Min10Float3, Min10Float4));
            Min10Float2x2.AddMembers(CreateMatrixTypeMembers(Min10Float2x2, Min10Float1, Min10Float2, Min10Float3, Min10Float4));
            Min10Float2x3.AddMembers(CreateMatrixTypeMembers(Min10Float2x3, Min10Float1, Min10Float2, Min10Float3, Min10Float4));
            Min10Float2x4.AddMembers(CreateMatrixTypeMembers(Min10Float2x4, Min10Float1, Min10Float2, Min10Float3, Min10Float4));
            Min10Float3x1.AddMembers(CreateMatrixTypeMembers(Min10Float3x1, Min10Float1, Min10Float2, Min10Float3, Min10Float4));
            Min10Float3x2.AddMembers(CreateMatrixTypeMembers(Min10Float3x2, Min10Float1, Min10Float2, Min10Float3, Min10Float4));
            Min10Float3x3.AddMembers(CreateMatrixTypeMembers(Min10Float3x3, Min10Float1, Min10Float2, Min10Float3, Min10Float4));
            Min10Float3x4.AddMembers(CreateMatrixTypeMembers(Min10Float3x4, Min10Float1, Min10Float2, Min10Float3, Min10Float4));
            Min10Float4x1.AddMembers(CreateMatrixTypeMembers(Min10Float4x1, Min10Float1, Min10Float2, Min10Float3, Min10Float4));
            Min10Float4x2.AddMembers(CreateMatrixTypeMembers(Min10Float4x2, Min10Float1, Min10Float2, Min10Float3, Min10Float4));
            Min10Float4x3.AddMembers(CreateMatrixTypeMembers(Min10Float4x3, Min10Float1, Min10Float2, Min10Float3, Min10Float4));
            Min10Float4x4.AddMembers(CreateMatrixTypeMembers(Min10Float4x4, Min10Float1, Min10Float2, Min10Float3, Min10Float4));
            Min16Int1x1.AddMembers(CreateMatrixTypeMembers(Min16Int1x1, Min16Int1, Min16Int2, Min16Int3, Min16Int4));
            Min16Int1x2.AddMembers(CreateMatrixTypeMembers(Min16Int1x2, Min16Int1, Min16Int2, Min16Int3, Min16Int4));
            Min16Int1x3.AddMembers(CreateMatrixTypeMembers(Min16Int1x3, Min16Int1, Min16Int2, Min16Int3, Min16Int4));
            Min16Int1x4.AddMembers(CreateMatrixTypeMembers(Min16Int1x4, Min16Int1, Min16Int2, Min16Int3, Min16Int4));
            Min16Int2x1.AddMembers(CreateMatrixTypeMembers(Min16Int2x1, Min16Int1, Min16Int2, Min16Int3, Min16Int4));
            Min16Int2x2.AddMembers(CreateMatrixTypeMembers(Min16Int2x2, Min16Int1, Min16Int2, Min16Int3, Min16Int4));
            Min16Int2x3.AddMembers(CreateMatrixTypeMembers(Min16Int2x3, Min16Int1, Min16Int2, Min16Int3, Min16Int4));
            Min16Int2x4.AddMembers(CreateMatrixTypeMembers(Min16Int2x4, Min16Int1, Min16Int2, Min16Int3, Min16Int4));
            Min16Int3x1.AddMembers(CreateMatrixTypeMembers(Min16Int3x1, Min16Int1, Min16Int2, Min16Int3, Min16Int4));
            Min16Int3x2.AddMembers(CreateMatrixTypeMembers(Min16Int3x2, Min16Int1, Min16Int2, Min16Int3, Min16Int4));
            Min16Int3x3.AddMembers(CreateMatrixTypeMembers(Min16Int3x3, Min16Int1, Min16Int2, Min16Int3, Min16Int4));
            Min16Int3x4.AddMembers(CreateMatrixTypeMembers(Min16Int3x4, Min16Int1, Min16Int2, Min16Int3, Min16Int4));
            Min16Int4x1.AddMembers(CreateMatrixTypeMembers(Min16Int4x1, Min16Int1, Min16Int2, Min16Int3, Min16Int4));
            Min16Int4x2.AddMembers(CreateMatrixTypeMembers(Min16Int4x2, Min16Int1, Min16Int2, Min16Int3, Min16Int4));
            Min16Int4x3.AddMembers(CreateMatrixTypeMembers(Min16Int4x3, Min16Int1, Min16Int2, Min16Int3, Min16Int4));
            Min16Int4x4.AddMembers(CreateMatrixTypeMembers(Min16Int4x4, Min16Int1, Min16Int2, Min16Int3, Min16Int4));
            Min12Int1x1.AddMembers(CreateMatrixTypeMembers(Min12Int1x1, Min12Int1, Min12Int2, Min12Int3, Min12Int4));
            Min12Int1x2.AddMembers(CreateMatrixTypeMembers(Min12Int1x2, Min12Int1, Min12Int2, Min12Int3, Min12Int4));
            Min12Int1x3.AddMembers(CreateMatrixTypeMembers(Min12Int1x3, Min12Int1, Min12Int2, Min12Int3, Min12Int4));
            Min12Int1x4.AddMembers(CreateMatrixTypeMembers(Min12Int1x4, Min12Int1, Min12Int2, Min12Int3, Min12Int4));
            Min12Int2x1.AddMembers(CreateMatrixTypeMembers(Min12Int2x1, Min12Int1, Min12Int2, Min12Int3, Min12Int4));
            Min12Int2x2.AddMembers(CreateMatrixTypeMembers(Min12Int2x2, Min12Int1, Min12Int2, Min12Int3, Min12Int4));
            Min12Int2x3.AddMembers(CreateMatrixTypeMembers(Min12Int2x3, Min12Int1, Min12Int2, Min12Int3, Min12Int4));
            Min12Int2x4.AddMembers(CreateMatrixTypeMembers(Min12Int2x4, Min12Int1, Min12Int2, Min12Int3, Min12Int4));
            Min12Int3x1.AddMembers(CreateMatrixTypeMembers(Min12Int3x1, Min12Int1, Min12Int2, Min12Int3, Min12Int4));
            Min12Int3x2.AddMembers(CreateMatrixTypeMembers(Min12Int3x2, Min12Int1, Min12Int2, Min12Int3, Min12Int4));
            Min12Int3x3.AddMembers(CreateMatrixTypeMembers(Min12Int3x3, Min12Int1, Min12Int2, Min12Int3, Min12Int4));
            Min12Int3x4.AddMembers(CreateMatrixTypeMembers(Min12Int3x4, Min12Int1, Min12Int2, Min12Int3, Min12Int4));
            Min12Int4x1.AddMembers(CreateMatrixTypeMembers(Min12Int4x1, Min12Int1, Min12Int2, Min12Int3, Min12Int4));
            Min12Int4x2.AddMembers(CreateMatrixTypeMembers(Min12Int4x2, Min12Int1, Min12Int2, Min12Int3, Min12Int4));
            Min12Int4x3.AddMembers(CreateMatrixTypeMembers(Min12Int4x3, Min12Int1, Min12Int2, Min12Int3, Min12Int4));
            Min12Int4x4.AddMembers(CreateMatrixTypeMembers(Min12Int4x4, Min12Int1, Min12Int2, Min12Int3, Min12Int4));
            Min16Uint1x1.AddMembers(CreateMatrixTypeMembers(Min16Uint1x1, Min16Uint1, Min16Uint2, Min16Uint3, Min16Uint4));
            Min16Uint1x2.AddMembers(CreateMatrixTypeMembers(Min16Uint1x2, Min16Uint1, Min16Uint2, Min16Uint3, Min16Uint4));
            Min16Uint1x3.AddMembers(CreateMatrixTypeMembers(Min16Uint1x3, Min16Uint1, Min16Uint2, Min16Uint3, Min16Uint4));
            Min16Uint1x4.AddMembers(CreateMatrixTypeMembers(Min16Uint1x4, Min16Uint1, Min16Uint2, Min16Uint3, Min16Uint4));
            Min16Uint2x1.AddMembers(CreateMatrixTypeMembers(Min16Uint2x1, Min16Uint1, Min16Uint2, Min16Uint3, Min16Uint4));
            Min16Uint2x2.AddMembers(CreateMatrixTypeMembers(Min16Uint2x2, Min16Uint1, Min16Uint2, Min16Uint3, Min16Uint4));
            Min16Uint2x3.AddMembers(CreateMatrixTypeMembers(Min16Uint2x3, Min16Uint1, Min16Uint2, Min16Uint3, Min16Uint4));
            Min16Uint2x4.AddMembers(CreateMatrixTypeMembers(Min16Uint2x4, Min16Uint1, Min16Uint2, Min16Uint3, Min16Uint4));
            Min16Uint3x1.AddMembers(CreateMatrixTypeMembers(Min16Uint3x1, Min16Uint1, Min16Uint2, Min16Uint3, Min16Uint4));
            Min16Uint3x2.AddMembers(CreateMatrixTypeMembers(Min16Uint3x2, Min16Uint1, Min16Uint2, Min16Uint3, Min16Uint4));
            Min16Uint3x3.AddMembers(CreateMatrixTypeMembers(Min16Uint3x3, Min16Uint1, Min16Uint2, Min16Uint3, Min16Uint4));
            Min16Uint3x4.AddMembers(CreateMatrixTypeMembers(Min16Uint3x4, Min16Uint1, Min16Uint2, Min16Uint3, Min16Uint4));
            Min16Uint4x1.AddMembers(CreateMatrixTypeMembers(Min16Uint4x1, Min16Uint1, Min16Uint2, Min16Uint3, Min16Uint4));
            Min16Uint4x2.AddMembers(CreateMatrixTypeMembers(Min16Uint4x2, Min16Uint1, Min16Uint2, Min16Uint3, Min16Uint4));
            Min16Uint4x3.AddMembers(CreateMatrixTypeMembers(Min16Uint4x3, Min16Uint1, Min16Uint2, Min16Uint3, Min16Uint4));
            Min16Uint4x4.AddMembers(CreateMatrixTypeMembers(Min16Uint4x4, Min16Uint1, Min16Uint2, Min16Uint3, Min16Uint4));

            AllScalarTypes = new[]
            {
                Bool,
                Uint,
                Int,
                Half,
                Float,
                Double,
                Min16Float,
                Min10Float,
                Min16Int,
                Min12Int,
                Min16Uint
            };

            AllBoolVectorTypes = new[]
            {
                Bool1,
                Bool2,
                Bool3,
                Bool4
            };

            AllIntVectorTypes = new[]
            {
                Int1,
                Int2,
                Int3,
                Int4
            };

            AllUintVectorTypes = new[]
            {
                Uint1,
                Uint2,
                Uint3,
                Uint4
            };

            AllHalfVectorTypes = new[]
            {
                Half1,
                Half2,
                Half3,
                Half4
            };

            AllFloatVectorTypes = new[]
            {
                Float1,
                Float2,
                Float3,
                Float4
            };

            AllDoubleVectorTypes = new[]
            {
                Double1,
                Double2,
                Double3,
                Double4
            };

            AllMin16FloatVectorTypes = new[]
            {
                Min16Float1,
                Min16Float2,
                Min16Float3,
                Min16Float4
            };

            AllMin10FloatVectorTypes = new[]
            {
                Min10Float1,
                Min10Float2,
                Min10Float3,
                Min10Float4
            };

            AllMin16IntVectorTypes = new[]
            {
                Min16Int1,
                Min16Int2,
                Min16Int3,
                Min16Int4
            };

            AllMin12IntVectorTypes = new[]
            {
                Min12Int1,
                Min12Int2,
                Min12Int3,
                Min12Int4
            };

            AllMin16UintVectorTypes = new[]
            {
                Min16Uint1,
                Min16Uint2,
                Min16Uint3,
                Min16Uint4
            };

            AllVectorTypes = AllBoolVectorTypes
                .Union(AllIntVectorTypes)
                .Union(AllUintVectorTypes)
                .Union(AllHalfVectorTypes)
                .Union(AllFloatVectorTypes)
                .Union(AllDoubleVectorTypes)
                .Union(AllMin16FloatVectorTypes)
                .Union(AllMin10FloatVectorTypes)
                .Union(AllMin16IntVectorTypes)
                .Union(AllMin12IntVectorTypes)
                .Union(AllMin16UintVectorTypes)
                .ToArray();

            AllBoolMatrixTypes = new[]
            {
                Bool1x1,
                Bool1x2,
                Bool1x3,
                Bool1x4,
                Bool2x1,
                Bool2x2,
                Bool2x3,
                Bool2x4,
                Bool3x1,
                Bool3x2,
                Bool3x3,
                Bool3x4,
                Bool4x1,
                Bool4x2,
                Bool4x3,
                Bool4x4
            };

            AllIntMatrixTypes = new[]
            {
                Int1x1,
                Int1x2,
                Int1x3,
                Int1x4,
                Int2x1,
                Int2x2,
                Int2x3,
                Int2x4,
                Int3x1,
                Int3x2,
                Int3x3,
                Int3x4,
                Int4x1,
                Int4x2,
                Int4x3,
                Int4x4
            };

            AllUintMatrixTypes = new[]
            {
                Uint1x1,
                Uint1x2,
                Uint1x3,
                Uint1x4,
                Uint2x1,
                Uint2x2,
                Uint2x3,
                Uint2x4,
                Uint3x1,
                Uint3x2,
                Uint3x3,
                Uint3x4,
                Uint4x1,
                Uint4x2,
                Uint4x3,
                Uint4x4
            };

            AllHalfMatrixTypes = new[]
            {
                Half1x1,
                Half1x2,
                Half1x3,
                Half1x4,
                Half2x1,
                Half2x2,
                Half2x3,
                Half2x4,
                Half3x1,
                Half3x2,
                Half3x3,
                Half3x4,
                Half4x1,
                Half4x2,
                Half4x3,
                Half4x4
            };

            AllFloatMatrixTypes = new[]
            {
                Float1x1,
                Float1x2,
                Float1x3,
                Float1x4,
                Float2x1,
                Float2x2,
                Float2x3,
                Float2x4,
                Float3x1,
                Float3x2,
                Float3x3,
                Float3x4,
                Float4x1,
                Float4x2,
                Float4x3,
                Float4x4
            };

            AllDoubleMatrixTypes = new[]
            {
                Double1x1,
                Double1x2,
                Double1x3,
                Double1x4,
                Double2x1,
                Double2x2,
                Double2x3,
                Double2x4,
                Double3x1,
                Double3x2,
                Double3x3,
                Double3x4,
                Double4x1,
                Double4x2,
                Double4x3,
                Double4x4
            };

            AllMin16FloatMatrixTypes = new[]
            {
                Min16Float1x1,
                Min16Float1x2,
                Min16Float1x3,
                Min16Float1x4,
                Min16Float2x1,
                Min16Float2x2,
                Min16Float2x3,
                Min16Float2x4,
                Min16Float3x1,
                Min16Float3x2,
                Min16Float3x3,
                Min16Float3x4,
                Min16Float4x1,
                Min16Float4x2,
                Min16Float4x3,
                Min16Float4x4
            };

            AllMin10FloatMatrixTypes = new[]
            {
                Min10Float1x1,
                Min10Float1x2,
                Min10Float1x3,
                Min10Float1x4,
                Min10Float2x1,
                Min10Float2x2,
                Min10Float2x3,
                Min10Float2x4,
                Min10Float3x1,
                Min10Float3x2,
                Min10Float3x3,
                Min10Float3x4,
                Min10Float4x1,
                Min10Float4x2,
                Min10Float4x3,
                Min10Float4x4
            };

            AllMin16IntMatrixTypes = new[]
            {
                Min16Int1x1,
                Min16Int1x2,
                Min16Int1x3,
                Min16Int1x4,
                Min16Int2x1,
                Min16Int2x2,
                Min16Int2x3,
                Min16Int2x4,
                Min16Int3x1,
                Min16Int3x2,
                Min16Int3x3,
                Min16Int3x4,
                Min16Int4x1,
                Min16Int4x2,
                Min16Int4x3,
                Min16Int4x4
            };

            AllMin12IntMatrixTypes = new[]
            {
                    Min12Int1x1,
                    Min12Int1x2,
                    Min12Int1x3,
                    Min12Int1x4,
                    Min12Int2x1,
                    Min12Int2x2,
                    Min12Int2x3,
                    Min12Int2x4,
                    Min12Int3x1,
                    Min12Int3x2,
                    Min12Int3x3,
                    Min12Int3x4,
                    Min12Int4x1,
                    Min12Int4x2,
                    Min12Int4x3,
                    Min12Int4x4
            };

            AllMin16UintMatrixTypes = new[]
            {
                    Min16Uint1x1,
                    Min16Uint1x2,
                    Min16Uint1x3,
                    Min16Uint1x4,
                    Min16Uint2x1,
                    Min16Uint2x2,
                    Min16Uint2x3,
                    Min16Uint2x4,
                    Min16Uint3x1,
                    Min16Uint3x2,
                    Min16Uint3x3,
                    Min16Uint3x4,
                    Min16Uint4x1,
                    Min16Uint4x2,
                    Min16Uint4x3,
                    Min16Uint4x4
            };

            AllMatrixTypes = AllBoolMatrixTypes
                .Union(AllIntMatrixTypes)
                .Union(AllUintMatrixTypes)
                .Union(AllHalfMatrixTypes)
                .Union(AllFloatMatrixTypes)
                .Union(AllDoubleMatrixTypes)
                .Union(AllMin16FloatMatrixTypes)
                .Union(AllMin10FloatMatrixTypes)
                .Union(AllMin16IntMatrixTypes)
                .Union(AllMin12IntMatrixTypes)
                .Union(AllMin16UintMatrixTypes)
                .ToArray();

            AllBoolTypes = new IntrinsicNumericTypeSymbol[] { Bool }
                .Union(AllBoolVectorTypes)
                .Union(AllBoolMatrixTypes)
                .ToArray();

            AllIntTypes = new IntrinsicNumericTypeSymbol[] { Int }
                .Union(AllIntVectorTypes)
                .Union(AllIntMatrixTypes)
                .ToArray();

            AllUintTypes = new IntrinsicNumericTypeSymbol[] { Uint }
                .Union(AllUintVectorTypes)
                .Union(AllUintMatrixTypes)
                .ToArray();

            AllHalfTypes = new IntrinsicNumericTypeSymbol[] { Half }
                .Union(AllHalfVectorTypes)
                .Union(AllHalfMatrixTypes)
                .ToArray();

            AllFloatTypes = new IntrinsicNumericTypeSymbol[] { Float }
                .Union(AllFloatVectorTypes)
                .Union(AllFloatMatrixTypes)
                .ToArray();

            AllDoubleTypes = new IntrinsicNumericTypeSymbol[] { Double }
                .Union(AllDoubleVectorTypes)
                .Union(AllDoubleMatrixTypes)
                .ToArray();

            AllMin16FloatTypes = new IntrinsicNumericTypeSymbol[] { Min16Float }
                .Union(AllMin16FloatVectorTypes)
                .Union(AllMin16FloatMatrixTypes)
                .ToArray();

            AllMin10FloatTypes = new IntrinsicNumericTypeSymbol[] { Min10Float }
                .Union(AllMin10FloatVectorTypes)
                .Union(AllMin10FloatMatrixTypes)
                .ToArray();

            AllMin16IntTypes = new IntrinsicNumericTypeSymbol[] { Min16Int }
                .Union(AllMin16IntVectorTypes)
                .Union(AllMin16IntMatrixTypes)
                .ToArray();

            AllMin12IntTypes = new IntrinsicNumericTypeSymbol[] { Min12Int }
                .Union(AllMin12IntVectorTypes)
                .Union(AllMin12IntMatrixTypes)
                .ToArray();

            AllMin16UintTypes = new IntrinsicNumericTypeSymbol[] { Min16Uint }
                .Union(AllMin16UintVectorTypes)
                .Union(AllMin16UintMatrixTypes)
                .ToArray();

            AllIntegralTypes = AllBoolTypes
                .Union(AllIntTypes)
                .Union(AllUintTypes)
                .Union(AllMin16IntTypes)
                .Union(AllMin12IntTypes)
                .Union(AllMin16UintTypes)
                .ToArray();

            AllNumericNonBoolTypes = AllIntTypes
                .Union(AllUintTypes)
                .Union(AllHalfTypes)
                .Union(AllFloatTypes)
                .Union(AllDoubleTypes)
                .Union(AllMin16FloatTypes)
                .Union(AllMin10FloatTypes)
                .Union(AllMin16IntTypes)
                .Union(AllMin12IntTypes)
                .Union(AllMin16UintTypes)
                .ToArray();

            AllNumericTypes = AllScalarTypes
                .Cast<IntrinsicNumericTypeSymbol>()
                .Union(AllVectorTypes)
                .Union(AllMatrixTypes)
                .ToArray();

            Sampler = new IntrinsicObjectTypeSymbol("sampler", "", PredefinedObjectType.Sampler);
            Sampler1D = new IntrinsicObjectTypeSymbol("sampler1D", "", PredefinedObjectType.Sampler1D);
            Sampler2D = new IntrinsicObjectTypeSymbol("sampler2D", "", PredefinedObjectType.Sampler2D);
            Sampler3D = new IntrinsicObjectTypeSymbol("sampler3D", "", PredefinedObjectType.Sampler3D);
            SamplerCube = new IntrinsicObjectTypeSymbol("samplerCUBE", "", PredefinedObjectType.SamplerCube);
            SamplerState = new IntrinsicObjectTypeSymbol("SamplerState", "", PredefinedObjectType.SamplerState);
            SamplerComparisonState = new IntrinsicObjectTypeSymbol("SamplerComparisonState", "", PredefinedObjectType.SamplerComparisonState);

            LegacyTexture = new IntrinsicObjectTypeSymbol("texture", "", PredefinedObjectType.Texture);

            BlendState = new IntrinsicObjectTypeSymbol("BlendState", "", PredefinedObjectType.BlendState);
            DepthStencilState = new IntrinsicObjectTypeSymbol("DepthStencilState", "", PredefinedObjectType.DepthStencilState);
            RasterizerState = new IntrinsicObjectTypeSymbol("RasterizerState", "", PredefinedObjectType.RasterizerState);

            GeometryShader = new IntrinsicObjectTypeSymbol("GeometryShader", "", PredefinedObjectType.GeometryShader);
            PixelShader = new IntrinsicObjectTypeSymbol("PixelShader", "", PredefinedObjectType.PixelShader);
            VertexShader = new IntrinsicObjectTypeSymbol("VertexShader", "", PredefinedObjectType.VertexShader);

            ByteAddressBuffer = CreatePredefinedObjectType("ByteAddressBuffer",
                "A read-only buffer that is indexed in bytes.",
                PredefinedObjectType.ByteAddressBuffer,
                CreateByteAddressBufferMethods);

            RWByteAddressBuffer = CreatePredefinedObjectType("RWByteAddressBuffer",
                "A read/write buffer that indexes in bytes.",
                PredefinedObjectType.RWByteAddressBuffer,
                CreateRWByteAddressBufferMethods);

            RasterizerOrderedByteAddressBuffer = CreatePredefinedObjectType("RasterizerOrderedByteAddressBuffer",
                "A rasterizer ordered read/write buffer that indexes in bytes.",
                PredefinedObjectType.RasterizerOrderedByteAddressBuffer,
                CreateRWByteAddressBufferMethods);

            AllTypes = AllNumericTypes
                .Cast<TypeSymbol>()
                .Union(new[] { Sampler, Sampler1D, Sampler2D, Sampler3D, SamplerCube, SamplerState, SamplerComparisonState, LegacyTexture })
                .Union(new[] { BlendState, DepthStencilState, RasterizerState })
                .Union(new[] { GeometryShader, PixelShader, VertexShader })
                .Union(new[] { ByteAddressBuffer, RWByteAddressBuffer, RasterizerOrderedByteAddressBuffer })
                .ToArray();
        }

        private static IEnumerable<Symbol> CreateScalarTypeFields(int numComponents,
            IntrinsicNumericTypeSymbol parentType,
            IntrinsicVectorTypeSymbol v1,
            IntrinsicVectorTypeSymbol v2,
            IntrinsicVectorTypeSymbol v3,
            IntrinsicVectorTypeSymbol v4)
        {
            var componentNameSets = new[] { "xyzw", "rgba" }.Select(x => x.Substring(0, numComponents).ToCharArray()).ToList();
            var vectorTypes = new[] { v1, v2, v3, v4 };

            foreach (var componentNameSet in componentNameSets)
                for (var i = 0; i < 4; i++)
                    foreach (var namePermutation in GetVectorComponentNamePermutations(componentNameSet, i + 1))
                        yield return new FieldSymbol(namePermutation, "", parentType, vectorTypes[i]);
        }

        private static IEnumerable<Symbol> CreateVectorTypeFields(
            int numComponents, IntrinsicVectorTypeSymbol vectorType, IntrinsicScalarTypeSymbol scalarType,
            IntrinsicVectorTypeSymbol v1,
            IntrinsicVectorTypeSymbol v2,
            IntrinsicVectorTypeSymbol v3,
            IntrinsicVectorTypeSymbol v4)
        {
            foreach (var field in CreateScalarTypeFields(numComponents, vectorType, v1, v2, v3, v4))
                yield return field;

            yield return new IndexerSymbol("[]", "", vectorType, Uint, scalarType);
        }

        private static IEnumerable<string> GetVectorComponentNamePermutations(char[] components, int num)
        {
            // for example:

            // components = ['x'], num = 1
            // => return ['x']

            // components = ['x'], num = 4
            // => return ['xxxx']

            // components = ['x', 'y'], num = 1
            // => return ['x', 'y']

            // components = ['x', 'y'], num = 2
            // => return ['xx', 'xy', 'yx', 'yy']

            // Yes, I probably should use some kind of clever code to generate the possible combinations.
            switch (num)
            {
                case 1:
                    return from a in components
                           select string.Concat(a);
                case 2:
                    return from a in components
                           from b in components
                           select string.Concat(a, b);
                case 3:
                    return from a in components
                           from b in components
                           from c in components
                           select string.Concat(a, b, c);
                case 4:
                    return from a in components
                           from b in components
                           from c in components
                           from d in components
                           select string.Concat(a, b, c, d);
                default:
                    throw new ArgumentOutOfRangeException("num");
            }
        }

        private static IEnumerable<Symbol> CreateMatrixTypeMembers(IntrinsicMatrixTypeSymbol matrixType,
            IntrinsicVectorTypeSymbol v1,
            IntrinsicVectorTypeSymbol v2,
            IntrinsicVectorTypeSymbol v3,
            IntrinsicVectorTypeSymbol v4)
        {
            var componentNameSets = new[]
            {
                new[,]
                {
                    { "_m00", "_m01", "_m02", "_m03" },
                    { "_m10", "_m11", "_m12", "_m13" },
                    { "_m20", "_m21", "_m22", "_m23" },
                    { "_m30", "_m31", "_m32", "_m33" }
                },
                new[,]
                {
                    { "_11", "_12", "_13", "_14" },
                    { "_21", "_22", "_23", "_24" },
                    { "_31", "_32", "_33", "_34" },
                    { "_41", "_42", "_43", "_44" }
                }
            }.Select(x => Slice(x, 0, matrixType.Rows - 1, 0, matrixType.Cols - 1)).Select(x => x.Cast<string>().ToArray()).ToList();

            var vectorTypes = new[] { v1, v2, v3, v4 };

            foreach (var componentNameSet in componentNameSets)
                for (var i = 0; i < 4; i++)
                    foreach (var namePermutation in GetMatrixComponentNamePermutations(componentNameSet, i + 1))
                        yield return new FieldSymbol(namePermutation, "", matrixType, vectorTypes[i]);

            yield return new IndexerSymbol("[]", "", matrixType, Uint, vectorTypes[matrixType.Cols - 1]);
        }

        private static T[,] Slice<T>(T[,] source, int fromIdxRank0, int toIdxRank0, int fromIdxRank1, int toIdxRank1)
        {
            T[,] ret = new T[toIdxRank0 - fromIdxRank0 + 1, toIdxRank1 - fromIdxRank1 + 1];

            for (int srcIdxRank0 = fromIdxRank0, dstIdxRank0 = 0; srcIdxRank0 <= toIdxRank0; srcIdxRank0++, dstIdxRank0++)
            {
                for (int srcIdxRank1 = fromIdxRank1, dstIdxRank1 = 0; srcIdxRank1 <= toIdxRank1; srcIdxRank1++, dstIdxRank1++)
                {
                    ret[dstIdxRank0, dstIdxRank1] = source[srcIdxRank0, srcIdxRank1];
                }
            }
            return ret;
        }

        private static IEnumerable<string> GetMatrixComponentNamePermutations(string[] components, int num)
        {
            // for example:

            // components = ["_11"], num = 1
            // => return ['x']

            // components = ["_11], num = 4
            // => return ["_11_11_11_11"]

            // components = ["_11", "_12"], num = 1
            // => return ["_11", "_12"]

            // components = ["_11", "_12"], num = 2
            // => return ["_11_11", "_11_12", "_12_11", "_12_12"]

            // components = ["_11", "_12", "_13, "_21", "_22", "_23", "_31", "_32", "_33"], num = 2
            // => return ["_11_11", "_11_12", "_11_13", "_11_21", ...]

            // Yes, I probably should use some kind of clever code to generate the possible combinations.
            switch (num)
            {
                case 1:
                    return from a in components
                           select string.Concat(a);
                case 2:
                    return from a in components
                           from b in components
                           select string.Concat(a, b);
                case 3:
                    return from a in components
                           from b in components
                           from c in components
                           select string.Concat(a, b, c);
                case 4:
                    return from a in components
                           from b in components
                           from c in components
                           from d in components
                           select string.Concat(a, b, c, d);
                default:
                    throw new ArgumentOutOfRangeException(nameof(num));
            }
        }

        public static IntrinsicScalarTypeSymbol GetScalarType(ScalarType scalarType)
        {
            return AllScalarTypes[(int)scalarType - 1];
        }

        public static IntrinsicVectorTypeSymbol GetVectorType(ScalarType scalarType, int numComponents)
        {
            return AllVectorTypes[(((int)scalarType - 1) * 4) + (numComponents - 1)];
        }

        public static IntrinsicMatrixTypeSymbol GetMatrixType(ScalarType scalarType, int numRows, int numCols)
        {
            return AllMatrixTypes[(((int)scalarType - 1) * 16) + ((numRows - 1) * 4) + (numCols - 1)];
        }

        public static IntrinsicObjectTypeSymbol CreateRWTextureType(PredefinedObjectType textureType, TypeSymbol valueType, ScalarType scalarType)
        {
            string name, documentation;

            switch (textureType)
            {
                case PredefinedObjectType.RWBuffer:
                    name = "RWBuffer";
                    documentation = "A read-write buffer type";
                    break;
                case PredefinedObjectType.RWTexture1D:
                    name = "RWTexture1D";
                    documentation = "A read-write texture type";
                    break;
                case PredefinedObjectType.RWTexture1DArray:
                    name = "RWTexture1DArray";
                    documentation = "An array of read-write 1D textures";
                    break;
                case PredefinedObjectType.RWTexture2D:
                    name = "RWTexture2D";
                    documentation = "A read-write 2D texture type";
                    break;
                case PredefinedObjectType.RWTexture2DArray:
                    name = "RWTexture2DArray";
                    documentation = "An array of read-write 2D textures";
                    break;
                case PredefinedObjectType.RWTexture3D:
                    name = "RWTexture3D";
                    documentation = "A read-write 3D texture type";
                    break;
                case PredefinedObjectType.RasterizerOrderedBuffer:
                    name = "RasterizerOrderedBuffer";
                    documentation = "A read-write buffer type";
                    break;
                case PredefinedObjectType.RasterizerOrderedTexture1D:
                    name = "RasterizerOrderedTexture1D";
                    documentation = "A read-write texture type";
                    break;
                case PredefinedObjectType.RasterizerOrderedTexture1DArray:
                    name = "RasterizerOrderedTexture1DArray";
                    documentation = "An array of read-write 1D textures";
                    break;
                case PredefinedObjectType.RasterizerOrderedTexture2D:
                    name = "RasterizerOrderedTexture2D";
                    documentation = "A read-write 2D texture type";
                    break;
                case PredefinedObjectType.RasterizerOrderedTexture2DArray:
                    name = "RasterizerOrderedTexture2DArray";
                    documentation = "An array of read-write 2D textures";
                    break;
                case PredefinedObjectType.RasterizerOrderedTexture3D:
                    name = "RasterizerOrderedTexture3D";
                    documentation = "A read-write 3D texture type";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return CreatePredefinedObjectType(name, documentation, textureType, t => CreateRWTextureMethods(textureType, t, valueType, scalarType));
        }

        private static IEnumerable<Symbol> CreateRWTextureMethods(PredefinedObjectType textureType, TypeSymbol parent, TypeSymbol valueType, ScalarType scalarType)
        {
            yield return CreateTextureGetDimensionsMethod(parent, textureType, Uint);
            yield return CreateTextureGetDimensionsMethod(parent, textureType, Float);

            var intLocationType = GetTextureIntLocationType(textureType);

            yield return new FunctionSymbol("Load", "Reads texel data without any filtering or sampling.", parent,
                valueType, m => new[]
                {
                    new ParameterSymbol("location", "The texture coordinates", m, intLocationType)
                });
            yield return new FunctionSymbol("Load", "Reads texel data without any filtering or sampling.", parent,
                valueType, m => new[]
                {
                    new ParameterSymbol("location", "The texture coordinates", m, intLocationType),
                    new ParameterSymbol("status", "The status of the operation.", m, Uint, ParameterDirection.Out)
                });

            var indexType = GetTextureIndexType(textureType);
            yield return new IndexerSymbol("[]", "Returns a resource variable.", parent, indexType, valueType, false);
        }

        public static IntrinsicObjectTypeSymbol CreateTextureType(PredefinedObjectType textureType, TypeSymbol valueType, ScalarType scalarType)
        {
            string name, documentation;

            switch (textureType)
            {
                case PredefinedObjectType.Buffer:
                    name = "Buffer";
                    documentation = "A buffer type";
                    break;
                case PredefinedObjectType.Texture1D:
                    name = "Texture1D";
                    documentation = "A 1D texture type";
                    break;
                case PredefinedObjectType.Texture1DArray:
                    name = "Texture1DArray";
                    documentation = "An array of 1D textures";
                    break;
                case PredefinedObjectType.Texture2D:
                    name = "Texture2D";
                    documentation = "A 2D texture type";
                    break;
                case PredefinedObjectType.Texture2DArray:
                    name = "Texture2DArray";
                    documentation = "An array of 2D textures";
                    break;
                case PredefinedObjectType.Texture3D:
                    name = "Texture3D";
                    documentation = "A 3D texture type";
                    break;
                case PredefinedObjectType.TextureCube:
                    name = "TextureCube";
                    documentation = "A cube texture type";
                    break;
                case PredefinedObjectType.TextureCubeArray:
                    name = "TextureCubeArray";
                    documentation = "An array of cube textures";
                    break;
                case PredefinedObjectType.Texture2DMS:
                    name = "Texture2DMS";
                    documentation = "A 2D multisampled texture type";
                    break;
                case PredefinedObjectType.Texture2DMSArray:
                    name = "Texture2DMSArray";
                    documentation = "An array of 2D multisampled textures";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return CreatePredefinedObjectType(name, documentation, textureType, t => CreateTextureMethods(textureType, t, valueType, scalarType));
        }

        private static IEnumerable<Symbol> CreateTextureMethods(PredefinedObjectType textureType, TypeSymbol parent, TypeSymbol valueType, ScalarType scalarType)
        {
            TypeSymbol locationType = null;
            switch (textureType)
            {
                case PredefinedObjectType.Texture1D:
                    locationType = Float;
                    break;
                case PredefinedObjectType.Texture1DArray:
                case PredefinedObjectType.Texture2D:
                    locationType = Float2;
                    break;
                case PredefinedObjectType.Texture2DArray:
                case PredefinedObjectType.Texture3D:
                case PredefinedObjectType.TextureCube:
                    locationType = Float3;
                    break;
                case PredefinedObjectType.TextureCubeArray:
                    locationType = Float4;
                    break;
            }

            TypeSymbol offsetType = null;
            switch (textureType)
            {
                case PredefinedObjectType.Texture1D:
                case PredefinedObjectType.Texture1DArray:
                    offsetType = Int;
                    break;
                case PredefinedObjectType.Texture2D:
                case PredefinedObjectType.Texture2DArray:
                case PredefinedObjectType.Texture2DMS:
                case PredefinedObjectType.Texture2DMSArray:
                    offsetType = Int2;
                    break;
                case PredefinedObjectType.Texture3D:
                    offsetType = Int3;
                    break;
            }

            switch (textureType)
            {
                case PredefinedObjectType.Buffer:
                case PredefinedObjectType.Texture2DMS:
                case PredefinedObjectType.Texture2DMSArray:
                    break;
                default:
                    TypeSymbol vectorType;
                    switch (textureType)
                    {
                        case PredefinedObjectType.Texture1D:
                        case PredefinedObjectType.Texture1DArray:
                            vectorType = Float1;
                            break;
                        case PredefinedObjectType.Texture2D:
                        case PredefinedObjectType.Texture2DArray:
                            vectorType = Float2;
                            break;
                        case PredefinedObjectType.Texture3D:
                        case PredefinedObjectType.TextureCube:
                        case PredefinedObjectType.TextureCubeArray:
                            vectorType = Float3;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    yield return new FunctionSymbol("CalculateLevelOfDetail", "Calculates the level of detail.", parent,
                        Float, m => new[]
                        {
                            new ParameterSymbol("samplerState", "A sampler state.", m, SamplerState),
                            new ParameterSymbol("x", "The linear interpolation value, which is a floating-point number between 0.0 and 1.0 inclusive.", m, vectorType)
                        });
                    yield return new FunctionSymbol("CalculateLevelOfDetailUnclamped", "Calculates the LOD without clamping the result.", parent,
                        Float, m => new[]
                        {
                            new ParameterSymbol("samplerState", "A sampler state.", m, SamplerState),
                            new ParameterSymbol("x", "The linear interpolation value, which is a floating-point number between 0.0 and 1.0 inclusive.", m, vectorType)
                        });
                    break;
            }

            switch (textureType)
            {
                case PredefinedObjectType.Texture2D:
                case PredefinedObjectType.Texture2DArray:
                case PredefinedObjectType.TextureCube:
                case PredefinedObjectType.TextureCubeArray:
                    yield return new FunctionSymbol("Gather", "Gets the four samples (red component only) that would be used for bilinear interpolation when sampling a texture.", parent,
                        GetVectorType(scalarType, 4), m => new[]
                        {
                            new ParameterSymbol("samplerState", "A sampler state.", m, SamplerState),
                            new ParameterSymbol("location", "The texture coordinates.", m, locationType)
                        });
                    foreach (var method in CreateTextureGatherComponentMethods(parent, scalarType, "Alpha", locationType, offsetType))
                        yield return method;
                    foreach (var method in CreateTextureGatherComponentMethods(parent, scalarType, "Red", locationType, offsetType))
                        yield return method;
                    foreach (var method in CreateTextureGatherComponentMethods(parent, scalarType, "Green", locationType, offsetType))
                        yield return method;
                    foreach (var method in CreateTextureGatherComponentMethods(parent, scalarType, "Blue", locationType, offsetType))
                        yield return method;
                    break;
            }

            switch (textureType)
            {
                case PredefinedObjectType.Texture2D:
                case PredefinedObjectType.Texture2DArray:
                    yield return new FunctionSymbol("Gather", "Gets the four samples (red component only) that would be used for bilinear interpolation when sampling a texture.", parent,
                        GetVectorType(scalarType, 4), m => new[]
                        {
                            new ParameterSymbol("samplerState", "A sampler state.", m, SamplerState),
                            new ParameterSymbol("location", "The texture coordinates.", m, locationType),
                            new ParameterSymbol("offset", "An optional texture coordinate offset, which can be used for any texture-object types. The offset is applied to the location before sampling.", m, offsetType)
                        });
                    break;
            }

            switch (textureType)
            {
                case PredefinedObjectType.Buffer:
                    yield return new FunctionSymbol("GetDimensions", "Gets the length of the buffer.", parent,
                        Void, m => new[]
                        {
                            new ParameterSymbol("dim", "The length, in bytes, of the buffer.", m, Uint, ParameterDirection.Out)
                        });
                    break;
                default:
                    yield return CreateTextureGetDimensionsWithMipLevelMethod(parent, textureType, Uint);
                    yield return CreateTextureGetDimensionsWithMipLevelMethod(parent, textureType, Float);
                    yield return CreateTextureGetDimensionsMethod(parent, textureType, Uint);
                    yield return CreateTextureGetDimensionsMethod(parent, textureType, Float);
                    break;
            }

            switch (textureType)
            {
                case PredefinedObjectType.Texture2DMS:
                case PredefinedObjectType.Texture2DMSArray:
                    yield return new FunctionSymbol("GetSamplePosition", "Gets the position of the specified sample.", parent,
                        Float2, m => new[]
                        {
                            new ParameterSymbol("sampleIndex", "The zero-based sample index.", m, Int)
                        });
                    break;
            }

            switch (textureType)
            {
                case PredefinedObjectType.TextureCube:
                case PredefinedObjectType.TextureCubeArray:
                    break;
                default:
                    var intLocationType = GetTextureIntLocationType(textureType);
                    switch (textureType)
                    {
                        case PredefinedObjectType.Texture2DMS:
                        case PredefinedObjectType.Texture2DMSArray:
                            yield return new FunctionSymbol("Load", "Reads texel data without any filtering or sampling.", parent,
                                valueType, m => new[]
                                {
                                    new ParameterSymbol("location", "The texture coordinates; the last component specifies the mipmap level. This method uses a 0-based coordinate system and not a 0.0-1.0 UV system. ", m, intLocationType),
                                    new ParameterSymbol("sampleIndex", "A sampling index.", m, Int),
                                });
                            yield return new FunctionSymbol("Load", "Reads texel data without any filtering or sampling.", parent,
                                valueType, m => new[]
                                {
                                    new ParameterSymbol("location", "The texture coordinates; the last component specifies the mipmap level. This method uses a 0-based coordinate system and not a 0.0-1.0 UV system.", m, intLocationType),
                                    new ParameterSymbol("sampleIndex", "A sampling index.", m, Int),
                                    new ParameterSymbol("offset", "An offset applied to the texture coordinates before sampling.", m, offsetType)
                                });
                            break;
                        default:
                            yield return new FunctionSymbol("Load", "Reads texel data without any filtering or sampling.", parent,
                                valueType, m => new[]
                                {
                                    new ParameterSymbol("location", "The texture coordinates; the last component specifies the mipmap level. This method uses a 0-based coordinate system and not a 0.0-1.0 UV system. ", m, intLocationType)
                                });
                            if (offsetType != null)
                                yield return new FunctionSymbol("Load", "Reads texel data without any filtering or sampling.", parent,
                                    valueType, m => new[]
                                    {
                                        new ParameterSymbol("location", "The texture coordinates; the last component specifies the mipmap level. This method uses a 0-based coordinate system and not a 0.0-1.0 UV system.", m, intLocationType),
                                        new ParameterSymbol("offset", "An offset applied to the texture coordinates before sampling.", m, offsetType)
                                    });
                            break;
                    }
                    break;
            }

            switch (textureType)
            {
                case PredefinedObjectType.Buffer:
                case PredefinedObjectType.Texture2DMS:
                case PredefinedObjectType.Texture2DMSArray:
                    break;
                default:
                    yield return new FunctionSymbol("Sample", "Samples a texture.", parent,
                        valueType, m => new[]
                        {
                            new ParameterSymbol("samplerState", "A sampler state.", m, SamplerState),
                            new ParameterSymbol("location", "The texture coordinates.", m, locationType)
                        });
                    switch (textureType)
                    {
                        case PredefinedObjectType.TextureCube:
                        case PredefinedObjectType.TextureCubeArray:
                            break;
                        default:
                            yield return new FunctionSymbol("Sample", "Samples a texture.", parent,
                                valueType, m => new[]
                                {
                                    new ParameterSymbol("samplerState", "A sampler state.", m, SamplerState),
                                    new ParameterSymbol("location", "The texture coordinates.", m, locationType),
                                    new ParameterSymbol("offset", "A texture coordinate offset, which can be used for any texture-object type; the offset is applied to the location before sampling. Use an offset only at an integer miplevel; otherwise, you may get results that do not translate well to hardware.", m, offsetType)
                                });
                            break;
                    }
                    break;
            }

            switch (textureType)
            {
                case PredefinedObjectType.Buffer:
                case PredefinedObjectType.Texture2DMS:
                case PredefinedObjectType.Texture2DMSArray:
                    break;
                default:
                    yield return new FunctionSymbol("SampleBias", "Samples a texture, after applying the input bias to the mipmap level.", parent,
                        valueType, m => new[]
                        {
                            new ParameterSymbol("samplerState", "A sampler state.", m, SamplerState),
                            new ParameterSymbol("location", "The texture coordinates.", m, locationType),
                            new ParameterSymbol("bias", "The bias value, which is a floating-point number between 0.0 and 1.0 inclusive, is applied to a mip level before sampling.", m, Float)
                        });
                    switch (textureType)
                    {
                        case PredefinedObjectType.TextureCube:
                        case PredefinedObjectType.TextureCubeArray:
                            break;
                        default:
                            yield return new FunctionSymbol("SampleBias", "Samples a texture, after applying the input bias to the mipmap level.", parent,
                                valueType, m => new[]
                                {
                                    new ParameterSymbol("samplerState", "A sampler state.", m, SamplerState),
                                    new ParameterSymbol("location", "The texture coordinates.", m, locationType),
                                    new ParameterSymbol("bias", "The bias value, which is a floating-point number between 0.0 and 1.0 inclusive, is applied to a mip level before sampling.", m, Float),
                                    new ParameterSymbol("offset", "A texture coordinate offset, which can be used for any texture-object type; the offset is applied to the location before sampling. Use an offset only at an integer miplevel; otherwise, you may get results that do not translate well to hardware.", m, offsetType)
                                });
                            break;
                    }
                    break;
            }

            switch (textureType)
            {
                case PredefinedObjectType.Buffer:
                case PredefinedObjectType.Texture2DMS:
                case PredefinedObjectType.Texture2DMSArray:
                case PredefinedObjectType.Texture3D:
                    break;
                default:
                    yield return new FunctionSymbol("SampleCmp", "Samples a texture and compares a single component against the specified comparison value.", parent,
                        valueType, m => new[]
                        {
                            new ParameterSymbol("samplerState", "A sampler-comparison state, which is the sampler state plus a comparison state (a comparison function and a comparison filter)", m, SamplerComparisonState),
                            new ParameterSymbol("location", "The texture coordinates.", m, locationType),
                            new ParameterSymbol("compareValue", "A floating-point value to use as a comparison value.", m, Float)
                        });
                    yield return new FunctionSymbol("SampleCmpLevelZero", "Samples a texture on mipmap level 0 only and compares a single component against the specified comparison value.", parent,
                        valueType, m => new[]
                        {
                            new ParameterSymbol("samplerState", "A sampler-comparison state, which is the sampler state plus a comparison state (a comparison function and a comparison filter)", m, SamplerComparisonState),
                            new ParameterSymbol("location", "The texture coordinates.", m, locationType),
                            new ParameterSymbol("compareValue", "A floating-point value to use as a comparison value.", m, Float)
                        });
                    switch (textureType)
                    {
                        case PredefinedObjectType.TextureCube:
                        case PredefinedObjectType.TextureCubeArray:
                            break;
                        default:
                            yield return new FunctionSymbol("SampleCmp", "Samples a texture and compares a single component against the specified comparison value.", parent,
                                valueType, m => new[]
                                {
                                    new ParameterSymbol("samplerState", "A sampler-comparison state, which is the sampler state plus a comparison state (a comparison function and a comparison filter)", m, SamplerComparisonState),
                                    new ParameterSymbol("location", "The texture coordinates.", m, locationType),
                                    new ParameterSymbol("compareValue", "A floating-point value to use as a comparison value.", m, Float),
                                    new ParameterSymbol("offset", "A texture coordinate offset, which can be used for any texture-object type; the offset is applied to the location before sampling. Use an offset only at an integer miplevel; otherwise, you may get results that do not translate well to hardware.", m, offsetType)
                                });
                            yield return new FunctionSymbol("SampleCmpLevelZero", "Samples a texture on mipmap level 0 only and compares a single component against the specified comparison value.", parent,
                                valueType, m => new[]
                                {
                                    new ParameterSymbol("samplerState", "A sampler-comparison state, which is the sampler state plus a comparison state (a comparison function and a comparison filter)", m, SamplerComparisonState),
                                    new ParameterSymbol("location", "The texture coordinates.", m, locationType),
                                    new ParameterSymbol("compareValue", "A floating-point value to use as a comparison value.", m, Float),
                                    new ParameterSymbol("offset", "A texture coordinate offset, which can be used for any texture-object type; the offset is applied to the location before sampling. Use an offset only at an integer miplevel; otherwise, you may get results that do not translate well to hardware.", m, offsetType)
                                });
                            break;
                    }
                    break;
            }

            switch (textureType)
            {
                case PredefinedObjectType.Buffer:
                case PredefinedObjectType.Texture2DMS:
                case PredefinedObjectType.Texture2DMSArray:
                    break;
                default:
                    TypeSymbol ddType;
                    switch (textureType)
                    {
                        case PredefinedObjectType.Texture1D:
                        case PredefinedObjectType.Texture1DArray:
                            ddType = Float;
                            break;
                        case PredefinedObjectType.Texture2D:
                        case PredefinedObjectType.Texture2DArray:
                            ddType = Float2;
                            break;
                        case PredefinedObjectType.Texture3D:
                        case PredefinedObjectType.TextureCube:
                        case PredefinedObjectType.TextureCubeArray:
                            ddType = Float3;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    yield return new FunctionSymbol("SampleGrad", "Samples a texture using a gradient to influence the way the sample location is calculated.", parent,
                        valueType, m => new[]
                        {
                            new ParameterSymbol("samplerState", "A sampler state.", m, SamplerState),
                            new ParameterSymbol("location", "The texture coordinates.", m, locationType),
                            new ParameterSymbol("ddx", "The rate of change of the surface geometry in the x direction.", m, ddType),
                            new ParameterSymbol("ddy", "The rate of change of the surface geometry in the y direction.", m, ddType)
                        });
                    switch (textureType)
                    {
                        case PredefinedObjectType.TextureCube:
                        case PredefinedObjectType.TextureCubeArray:
                            break;
                        default:
                            yield return new FunctionSymbol("SampleGrad", "Samples a texture using a gradient to influence the way the sample location is calculated.", parent,
                                valueType, m => new[]
                                {
                                    new ParameterSymbol("samplerState", "A sampler state.", m, SamplerState),
                                    new ParameterSymbol("location", "The texture coordinates.", m, locationType),
                                    new ParameterSymbol("ddx", "The rate of change of the surface geometry in the x direction.", m, ddType),
                                    new ParameterSymbol("ddy", "The rate of change of the surface geometry in the y direction.", m, ddType),
                                    new ParameterSymbol("offset", "A texture coordinate offset, which can be used for any texture-object type; the offset is applied to the location before sampling. Use an offset only at an integer miplevel; otherwise, you may get results that do not translate well to hardware.", m, offsetType)
                                });
                            break;
                    }
                    break;
            }

            switch (textureType)
            {
                case PredefinedObjectType.Buffer:
                case PredefinedObjectType.Texture2DMS:
                case PredefinedObjectType.Texture2DMSArray:
                    break;
                default:
                    yield return new FunctionSymbol("SampleLevel", "Samples a texture using a mipmap-level offset.", parent,
                        valueType, m => new[]
                        {
                            new ParameterSymbol("samplerState", "A sampler state.", m, SamplerState),
                            new ParameterSymbol("location", "The texture coordinates.", m, locationType),
                            new ParameterSymbol("lod", "A number that specifies the mipmap level. If the value is ≤ 0, the zero'th (biggest map) is used. The fractional value (if supplied) is used to interpolate between two mipmap levels.", m, Float)
                        });
                    switch (textureType)
                    {
                        case PredefinedObjectType.TextureCube:
                        case PredefinedObjectType.TextureCubeArray:
                            break;
                        default:
                            yield return new FunctionSymbol("SampleLevel", "Samples a texture using a mipmap-level offset.", parent,
                                valueType, m => new[]
                                {
                                    new ParameterSymbol("samplerState", "A sampler state.", m, SamplerState),
                                    new ParameterSymbol("location", "The texture coordinates.", m, locationType),
                                    new ParameterSymbol("lod", "A number that specifies the mipmap level. If the value is ≤ 0, the zero'th (biggest map) is used. The fractional value (if supplied) is used to interpolate between two mipmap levels.", m, Float),
                                    new ParameterSymbol("offset", "A texture coordinate offset, which can be used for any texture-object type; the offset is applied to the location before sampling. Use an offset only at an integer miplevel; otherwise, you may get results that do not translate well to hardware.", m, offsetType)
                                });
                            break;
                    }
                    break;
            }

            switch (textureType)
            {
                case PredefinedObjectType.Buffer:
                    yield return new IndexerSymbol("[]", "", parent, Uint, valueType);
                    break;
                case PredefinedObjectType.Texture2D:
                    yield return new IndexerSymbol("[]", "", parent, Uint2, valueType);
                    break;
            }
        }

        private static TypeSymbol GetTextureIntLocationType(PredefinedObjectType textureType)
        {
            switch (textureType)
            {
                case PredefinedObjectType.Buffer:
                case PredefinedObjectType.RWBuffer:
                case PredefinedObjectType.RasterizerOrderedBuffer:
                    return Int;
                case PredefinedObjectType.Texture1D:
                case PredefinedObjectType.RWTexture1D:
                case PredefinedObjectType.RasterizerOrderedTexture1D:
                case PredefinedObjectType.Texture2DMS:
                    return Int2;
                case PredefinedObjectType.Texture1DArray:
                case PredefinedObjectType.RWTexture1DArray:
                case PredefinedObjectType.RasterizerOrderedTexture1DArray:
                case PredefinedObjectType.Texture2D:
                case PredefinedObjectType.RWTexture2D:
                case PredefinedObjectType.RasterizerOrderedTexture2D:
                case PredefinedObjectType.Texture2DMSArray:
                    return Int3;
                case PredefinedObjectType.Texture2DArray:
                case PredefinedObjectType.RWTexture2DArray:
                case PredefinedObjectType.RasterizerOrderedTexture2DArray:
                case PredefinedObjectType.Texture3D:
                case PredefinedObjectType.RWTexture3D:
                case PredefinedObjectType.RasterizerOrderedTexture3D:
                    return Int4;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static TypeSymbol GetTextureIndexType(PredefinedObjectType textureType)
        {
            switch (textureType)
            {
                case PredefinedObjectType.Buffer:
                case PredefinedObjectType.RWBuffer:
                case PredefinedObjectType.RasterizerOrderedBuffer:
                case PredefinedObjectType.Texture1D:
                case PredefinedObjectType.RWTexture1D:
                case PredefinedObjectType.RasterizerOrderedTexture1D:
                    return Int;
                case PredefinedObjectType.Texture1DArray:
                case PredefinedObjectType.RWTexture1DArray:
                case PredefinedObjectType.RasterizerOrderedTexture1DArray:
                case PredefinedObjectType.Texture2D:
                case PredefinedObjectType.RWTexture2D:
                case PredefinedObjectType.RasterizerOrderedTexture2D:
                    return Int2;
                case PredefinedObjectType.Texture2DArray:
                case PredefinedObjectType.RWTexture2DArray:
                case PredefinedObjectType.RasterizerOrderedTexture2DArray:
                case PredefinedObjectType.Texture3D:
                case PredefinedObjectType.RWTexture3D:
                case PredefinedObjectType.RasterizerOrderedTexture3D:
                    return Int3;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static IEnumerable<FunctionSymbol> CreateTextureGatherComponentMethods(Symbol parent, ScalarType scalarType, string componentName, TypeSymbol locationType, TypeSymbol offsetType)
        {
            var componentNameLower = componentName.ToLower();

            if (offsetType != null)
            {
                yield return new FunctionSymbol($"Gather{componentName}", $"Samples a texture and returns the {componentNameLower} component.", parent,
                    GetVectorType(scalarType, 4), m => new[]
                    {
                        new ParameterSymbol("samplerState", "A sampler state.", m, SamplerState),
                        new ParameterSymbol("location", "The texture coordinates.", m, locationType),
                        new ParameterSymbol("offset", "An offset that is applied to the texture coordinate before sampling.", m, offsetType)
                    });
                yield return new FunctionSymbol($"Gather{componentName}", $"Samples a texture and returns the {componentNameLower} component.", parent,
                    GetVectorType(scalarType, 4), m => new[]
                    {
                        new ParameterSymbol("samplerState", "A sampler state.", m, SamplerState),
                        new ParameterSymbol("location", "The texture coordinates.", m, locationType),
                        new ParameterSymbol("offset1", "The first offset component applied to the texture coordinates before sampling.", m, offsetType),
                        new ParameterSymbol("offset2", "The second offset component applied to the texture coordinates before sampling.", m, offsetType),
                        new ParameterSymbol("offset3", "The third offset component applied to the texture coordinates before sampling.", m, offsetType),
                        new ParameterSymbol("offset4", "The fourth offset component applied to the texture coordinates before sampling.", m, offsetType)
                    });
                yield return new FunctionSymbol($"Gather{componentName}", $"Samples a texture and returns the {componentNameLower} component along with status about the operation.", parent,
                    GetVectorType(scalarType, 4), m => new[]
                    {
                        new ParameterSymbol("samplerState", "A sampler state.", m, SamplerState),
                        new ParameterSymbol("location", "The texture coordinates.", m, locationType),
                        new ParameterSymbol("offset", "An offset that is applied to the texture coordinate before sampling.", m, offsetType),
                        new ParameterSymbol("status", "The status of the operation.", m, Uint, ParameterDirection.Out)
                    });
                yield return new FunctionSymbol($"Gather{componentName}", $"Samples a texture and returns the {componentNameLower} component along with status about the operation.", parent,
                    GetVectorType(scalarType, 4), m => new[]
                    {
                        new ParameterSymbol("samplerState", "A sampler state.", m, SamplerState),
                        new ParameterSymbol("location", "The texture coordinates.", m, locationType),
                        new ParameterSymbol("offset1", "The first offset component applied to the texture coordinates before sampling.", m, offsetType),
                        new ParameterSymbol("offset2", "The second offset component applied to the texture coordinates before sampling.", m, offsetType),
                        new ParameterSymbol("offset3", "The third offset component applied to the texture coordinates before sampling.", m, offsetType),
                        new ParameterSymbol("offset4", "The fourth offset component applied to the texture coordinates before sampling.", m, offsetType),
                        new ParameterSymbol("status", "The status of the operation.", m, Uint, ParameterDirection.Out)
                    });
            }
            else
            {
                yield return new FunctionSymbol($"Gather{componentName}", $"Samples a texture and returns the {componentNameLower} component.", parent,
                    GetVectorType(scalarType, 4), m => new[]
                    {
                        new ParameterSymbol("samplerState", "A sampler state.", m, SamplerState),
                        new ParameterSymbol("location", "The texture coordinates.", m, locationType),
                        new ParameterSymbol("status", "The status of the operation.", m, Uint, ParameterDirection.Out)
                    });
            }

            if (offsetType != null)
            {
                yield return new FunctionSymbol($"GatherCmp{componentName}", $"Samples a texture, tests the samples against a compare value, and returns the {componentNameLower} component.", parent,
                    GetVectorType(scalarType, 4), m => new[]
                    {
                        new ParameterSymbol("samplerState", "A sampler state.", m, SamplerComparisonState),
                        new ParameterSymbol("location", "The texture coordinates.", m, locationType),
                        new ParameterSymbol("compareValue", "A value to compare each against each sampled value.", m, Float),
                        new ParameterSymbol("offset", "An offset that is applied to the texture coordinate before sampling.", m, offsetType)
                    });
                yield return new FunctionSymbol($"GatherCmp{componentName}", $"Samples a texture, tests the samples against a compare value, and returns the {componentNameLower} component.", parent,
                    GetVectorType(scalarType, 4), m => new[]
                    {
                        new ParameterSymbol("samplerState", "A sampler state.", m, SamplerComparisonState),
                        new ParameterSymbol("location", "The texture coordinates.", m, locationType),
                        new ParameterSymbol("compareValue", "A value to compare each against each sampled value.", m, Float),
                        new ParameterSymbol("offset1", "The first offset component applied to the texture coordinates before sampling.", m, offsetType),
                        new ParameterSymbol("offset2", "The second offset component applied to the texture coordinates before sampling.", m, offsetType),
                        new ParameterSymbol("offset3", "The third offset component applied to the texture coordinates before sampling.", m, offsetType),
                        new ParameterSymbol("offset4", "The fourth offset component applied to the texture coordinates before sampling.", m, offsetType)
                    });
                yield return new FunctionSymbol($"GatherCmp{componentName}", $"Samples a texture, tests the samples against a compare value, and returns the {componentNameLower} component along with status about the operation.", parent,
                    GetVectorType(scalarType, 4), m => new[]
                    {
                        new ParameterSymbol("samplerState", "A sampler state.", m, SamplerComparisonState),
                        new ParameterSymbol("location", "The texture coordinates.", m, locationType),
                        new ParameterSymbol("compareValue", "A value to compare each against each sampled value.", m, Float),
                        new ParameterSymbol("offset", "An offset that is applied to the texture coordinate before sampling.", m, offsetType),
                        new ParameterSymbol("status", "The status of the operation.", m, Uint, ParameterDirection.Out)
                    });
                yield return new FunctionSymbol($"GatherCmp{componentName}", $"Samples a texture, tests the samples against a compare value, and returns the {componentNameLower} component along with status about the operation.", parent,
                    GetVectorType(scalarType, 4), m => new[]
                    {
                        new ParameterSymbol("samplerState", "A sampler state.", m, SamplerComparisonState),
                        new ParameterSymbol("location", "The texture coordinates.", m, locationType),
                        new ParameterSymbol("compareValue", "A value to compare each against each sampled value.", m, Float),
                        new ParameterSymbol("offset1", "The first offset component applied to the texture coordinates before sampling.", m, offsetType),
                        new ParameterSymbol("offset2", "The second offset component applied to the texture coordinates before sampling.", m, offsetType),
                        new ParameterSymbol("offset3", "The third offset component applied to the texture coordinates before sampling.", m, offsetType),
                        new ParameterSymbol("offset4", "The fourth offset component applied to the texture coordinates before sampling.", m, offsetType),
                        new ParameterSymbol("status", "The status of the operation.", m, Uint, ParameterDirection.Out)
                    });
            }
            else
            {
                yield return new FunctionSymbol($"GatherCmp{componentName}", $"Samples a texture, tests the samples against a compare value, and returns the {componentNameLower} component.", parent,
                    GetVectorType(scalarType, 4), m => new[]
                    {
                        new ParameterSymbol("samplerState", "A sampler state.", m, SamplerComparisonState),
                        new ParameterSymbol("location", "The texture coordinates.", m, locationType),
                        new ParameterSymbol("compareValue", "A value to compare each against each sampled value.", m, Float),
                        new ParameterSymbol("status", "The status of the operation.", m, Uint, ParameterDirection.Out)
                    });
            }
        }

        private static FunctionSymbol CreateTextureGetDimensionsWithMipLevelMethod(TypeSymbol parent, PredefinedObjectType textureType, TypeSymbol parameterType)
        {
            return new FunctionSymbol("GetDimensions", "Gets texture size information.", parent,
                Void, m =>
                {
                    var result = new List<ParameterSymbol>();
                    result.Add(new ParameterSymbol("mipLevel", "A zero-based index that identifies the mipmap level.", m, Uint));
                    result.Add(new ParameterSymbol("width", "The texture width, in texels.", m, parameterType, ParameterDirection.Out));
                    switch (textureType)
                    {
                        case PredefinedObjectType.Texture2D:
                        case PredefinedObjectType.Texture2DArray:
                        case PredefinedObjectType.Texture3D:
                        case PredefinedObjectType.TextureCube:
                        case PredefinedObjectType.TextureCubeArray:
                            result.Add(new ParameterSymbol("height", "The texture height, in texels.", m, parameterType, ParameterDirection.Out));
                            break;
                    }
                    switch (textureType)
                    {
                        case PredefinedObjectType.Texture3D:
                            result.Add(new ParameterSymbol("depth", "The texture depth, in texels.", m, parameterType, ParameterDirection.Out));
                            break;
                    }
                    switch (textureType)
                    {
                        case PredefinedObjectType.Texture1DArray:
                        case PredefinedObjectType.Texture2DArray:
                        case PredefinedObjectType.TextureCubeArray:
                            result.Add(new ParameterSymbol("elements", "The number of elements in an array.", m, parameterType, ParameterDirection.Out));
                            break;
                    }
                    result.Add(new ParameterSymbol("numberOfLevels", "The number of mipmap levels.", m, parameterType, ParameterDirection.Out));
                    return result;
                });
        }

        private static FunctionSymbol CreateTextureGetDimensionsMethod(TypeSymbol parent, PredefinedObjectType textureType, TypeSymbol parameterType)
        {
            return new FunctionSymbol("GetDimensions", "Gets texture size information.", parent,
                Void, m =>
                {
                    var result = new List<ParameterSymbol>();
                    result.Add(new ParameterSymbol("width", "The texture width, in texels.", m, parameterType, ParameterDirection.Out));
                    switch (textureType)
                    {
                        case PredefinedObjectType.Texture2D:
                        case PredefinedObjectType.Texture2DArray:
                        case PredefinedObjectType.Texture3D:
                        case PredefinedObjectType.TextureCube:
                        case PredefinedObjectType.TextureCubeArray:
                            result.Add(new ParameterSymbol("height", "The texture height, in texels.", m, parameterType, ParameterDirection.Out));
                            break;
                    }
                    switch (textureType)
                    {
                        case PredefinedObjectType.Texture3D:
                            result.Add(new ParameterSymbol("depth", "The texture depth, in texels.", m, parameterType, ParameterDirection.Out));
                            break;
                    }
                    switch (textureType)
                    {
                        case PredefinedObjectType.Texture1DArray:
                        case PredefinedObjectType.Texture2DArray:
                        case PredefinedObjectType.TextureCubeArray:
                            result.Add(new ParameterSymbol("elements", "The number of elements in an array.", m, parameterType, ParameterDirection.Out));
                            break;
                    }
                    return result;
                });
        }

        private static IntrinsicObjectTypeSymbol CreatePredefinedObjectType(string name, string documentation, PredefinedObjectType predefinedObjectType, Func<TypeSymbol, IEnumerable<Symbol>> membersCallback)
        {
            var result = new IntrinsicObjectTypeSymbol(name, documentation, predefinedObjectType);
            result.AddMembers(membersCallback(result));
            return result;
        }

        public static IntrinsicObjectTypeSymbol CreateAppendStructuredBufferType(TypeSymbol valueType)
        {
            return CreatePredefinedObjectType("AppendStructuredBuffer",
                "Output buffer that appears as a stream the shader may append to. Only structured buffers can take T types that are structures.",
                PredefinedObjectType.AppendStructuredBuffer,
                t => new[]
                {
                    new FunctionSymbol("Append", "Appends a value to the end of the buffer.", t, Void,
                        m => new[]
                        {
                            new ParameterSymbol("value", "The input value.", m, valueType)
                        }),
                    new FunctionSymbol("GetDimensions", "Gets the resource dimensions.", t, Void,
                        m => new[]
                        {
                            new ParameterSymbol("numStructs", "The number of structures.", m, Uint, ParameterDirection.Out),
                            new ParameterSymbol("stride", "The number of bytes in each element.", m, Uint, ParameterDirection.Out)
                        })
                });
        }

        public static readonly IntrinsicObjectTypeSymbol ByteAddressBuffer;
        public static readonly IntrinsicObjectTypeSymbol RWByteAddressBuffer;
        public static readonly IntrinsicObjectTypeSymbol RasterizerOrderedByteAddressBuffer;

        private static FunctionSymbol[] CreateRWByteAddressBufferMethods(TypeSymbol t)
        {
            return CreateByteAddressBufferMethods(t)
                .Union(new[]
                {
                    new FunctionSymbol("InterlockedAdd", "Adds the value, atomically.", t, Void,
                        m => new[]
                        {
                            new ParameterSymbol("dest", "The destination address.", m, Uint),
                            new ParameterSymbol("value", "The input value.", m, Uint),
                            new ParameterSymbol("originalValue", "The original value.", m, Uint, ParameterDirection.Out)
                        }),
                    new FunctionSymbol("InterlockedAnd", "Ands the value, atomically.", t, Void,
                        m => new[]
                        {
                            new ParameterSymbol("dest", "The destination address.", m, Uint),
                            new ParameterSymbol("value", "The input value.", m, Uint),
                            new ParameterSymbol("originalValue", "The original value.", m, Uint, ParameterDirection.Out)
                        }),
                    new FunctionSymbol("InterlockedCompareExchange", "Compares the input to the comparison value and exchanges the result, atomically.", t, Void,
                        m => new[]
                        {
                            new ParameterSymbol("dest", "The destination address.", m, Uint),
                            new ParameterSymbol("compareValue", "The comparison value.", m, Uint),
                            new ParameterSymbol("value", "The input value.", m, Uint),
                            new ParameterSymbol("originalValue", "The original value.", m, Uint, ParameterDirection.Out)
                        }),
                    new FunctionSymbol("InterlockedCompareStore", "Compares the input to the comparison value, atomically.", t, Void,
                        m => new[]
                        {
                            new ParameterSymbol("dest", "The destination address.", m, Uint),
                            new ParameterSymbol("compareValue", "The comparison value.", m, Uint),
                            new ParameterSymbol("value", "The input value.", m, Uint)
                        }),
                    new FunctionSymbol("InterlockedExchange", "Exchanges a value, atomically.", t, Void,
                        m => new[]
                        {
                            new ParameterSymbol("dest", "The destination address.", m, Uint),
                            new ParameterSymbol("value", "The input value.", m, Uint),
                            new ParameterSymbol("originalValue", "The original value.", m, Uint, ParameterDirection.Out)
                        }),
                    new FunctionSymbol("InterlockedMax", "Finds the maximum value, atomically.", t, Void,
                        m => new[]
                        {
                            new ParameterSymbol("dest", "The destination address.", m, Uint),
                            new ParameterSymbol("value", "The input value.", m, Uint),
                            new ParameterSymbol("originalValue", "The original value.", m, Uint, ParameterDirection.Out)
                        }),
                    new FunctionSymbol("InterlockedMin", "Finds the minimum value, atomically.", t, Void,
                        m => new[]
                        {
                            new ParameterSymbol("dest", "The destination address.", m, Uint),
                            new ParameterSymbol("value", "The input value.", m, Uint),
                            new ParameterSymbol("originalValue", "The original value.", m, Uint, ParameterDirection.Out)
                        }),
                    new FunctionSymbol("InterlockedOr", "Performs an atomic OR on the value.", t, Void,
                        m => new[]
                        {
                            new ParameterSymbol("dest", "The destination address.", m, Uint),
                            new ParameterSymbol("value", "The input value.", m, Uint),
                            new ParameterSymbol("originalValue", "The original value.", m, Uint, ParameterDirection.Out)
                        }),
                    new FunctionSymbol("InterlockedXor", "Performs an atomic XOR on the value.", t, Void,
                        m => new[]
                        {
                            new ParameterSymbol("dest", "The destination address.", m, Uint),
                            new ParameterSymbol("value", "The input value.", m, Uint),
                            new ParameterSymbol("originalValue", "The original value.", m, Uint, ParameterDirection.Out)
                        }),

                    new FunctionSymbol("Store", "Sets one value.", t, Void,
                        m => new[]
                        {
                            new ParameterSymbol("address", "The input address in bytes, which must be a multiple of 4.", m, Uint),
                            new ParameterSymbol("value", "One input value.", m, Uint)
                        }),
                    new FunctionSymbol("Store2", "Sets two values.", t, Void,
                        m => new[]
                        {
                            new ParameterSymbol("address", "The input address in bytes, which must be a multiple of 4.", m, Uint),
                            new ParameterSymbol("value", "Two input values.", m, Uint2)
                        }),
                    new FunctionSymbol("Store3", "Sets three values.", t, Void,
                        m => new[]
                        {
                            new ParameterSymbol("address", "The input address in bytes, which must be a multiple of 4.", m, Uint),
                            new ParameterSymbol("value", "Three input values.", m, Uint3)
                        }),
                    new FunctionSymbol("Store4", "Sets four values.", t, Void,
                        m => new[]
                        {
                            new ParameterSymbol("address", "The input address in bytes, which must be a multiple of 4.", m, Uint),
                            new ParameterSymbol("value", "Four input values.", m, Uint4)
                        }),
                })
                .ToArray();
        }

        private static FunctionSymbol[] CreateByteAddressBufferMethods(TypeSymbol t)
        {
            return new[]
            {
                new FunctionSymbol("GetDimensions", "Gets the resource dimensions.", t, Void,
                    m => new[]
                    {
                        new ParameterSymbol("dim", "The length, in bytes, of the buffer.", m, Uint, ParameterDirection.Out)
                    }),
                new FunctionSymbol("Load", "Gets one value and the status of the operation from a read-only buffer indexed in bytes.", t, Uint,
                    m => new[]
                    {
                        new ParameterSymbol("location", "The input address in bytes, which must be a multiple of 4.", m, Int),
                        new ParameterSymbol("status", "The status of the operation.", m, Uint, ParameterDirection.Out)
                    }),
                new FunctionSymbol("Load", "Gets one value from a read-only buffer indexed in bytes.", t, Uint,
                    m => new[]
                    {
                        new ParameterSymbol("location", "The input address in bytes, which must be a multiple of 4.", m, Int)
                    }),
                new FunctionSymbol("Load2", "Gets two values and the status of the operation from a read-only buffer indexed in bytes.", t, Uint2,
                    m => new[]
                    {
                        new ParameterSymbol("location", "The input address in bytes, which must be a multiple of 4.", m, Int),
                        new ParameterSymbol("status", "The status of the operation.", m, Uint, ParameterDirection.Out)
                    }),
                new FunctionSymbol("Load2", "Gets two values from a read-only buffer indexed in bytes.", t, Uint2,
                    m => new[]
                    {
                        new ParameterSymbol("location", "The input address in bytes, which must be a multiple of 4.", m, Int)
                    }),
                new FunctionSymbol("Load3", "Gets three values and the status of the operation from a read-only buffer indexed in bytes.", t, Uint3,
                    m => new[]
                    {
                        new ParameterSymbol("location", "The input address in bytes, which must be a multiple of 4.", m, Int),
                        new ParameterSymbol("status", "The status of the operation.", m, Uint, ParameterDirection.Out)
                    }),
                new FunctionSymbol("Load3", "Gets three values from a read-only buffer indexed in bytes.", t, Uint3,
                    m => new[]
                    {
                        new ParameterSymbol("location", "The input address in bytes, which must be a multiple of 4.", m, Int)
                    }),
                new FunctionSymbol("Load4", "Gets four values and the status of the operation from a read-only buffer indexed in bytes.", t, Uint4,
                    m => new[]
                    {
                        new ParameterSymbol("location", "The input address in bytes, which must be a multiple of 4.", m, Int),
                        new ParameterSymbol("status", "The status of the operation.", m, Uint, ParameterDirection.Out)
                    }),
                new FunctionSymbol("Load4", "Gets four values from a read-only buffer indexed in bytes.", t, Uint4,
                    m => new[]
                    {
                        new ParameterSymbol("location", "The input address in bytes, which must be a multiple of 4.", m, Int)
                    })
            };
        }

        public static IntrinsicObjectTypeSymbol CreateConsumeStructuredBufferType(TypeSymbol valueType)
        {
            return CreatePredefinedObjectType("ConsumeStructuredBuffer",
                "An input buffer that appears as a stream the shader may pull values from. Only structured buffers can take T types that are structures.",
                PredefinedObjectType.ConsumeStructuredBuffer,
                t => new[]
                {
                    new FunctionSymbol("Consume", "Removes a value from the end of the buffer.", t, valueType,
                        m => Enumerable.Empty<ParameterSymbol>()),
                    new FunctionSymbol("GetDimensions", "Gets the resource dimensions.", t, Void,
                        m => new[]
                        {
                            new ParameterSymbol("numStructs", "The number of structures.", m, Uint, ParameterDirection.Out),
                            new ParameterSymbol("stride", "The number of bytes in each element.", m, Uint, ParameterDirection.Out)
                        })
                });
        }

        public static IntrinsicObjectTypeSymbol CreateStructuredBufferType(TypeSymbol valueType)
        {
            return CreatePredefinedObjectType("StructuredBuffer",
                "A read-only buffer, which can take a T type that is a structure.",
                PredefinedObjectType.StructuredBuffer,
                t => CreateStructuredBufferMethods(t, valueType)
                    .Union(new Symbol[]
                    {
                        new IndexerSymbol("[]", "Returns a read-only resource variable of a StructuredBuffer.", t, Uint, valueType)
                    }));
        }

        public static IntrinsicObjectTypeSymbol CreateRWStructuredBufferType(TypeSymbol valueType)
        {
            return CreatePredefinedObjectType("RWStructuredBuffer",
                "A read/write buffer, which can take a T type that is a structure.",
                PredefinedObjectType.RWStructuredBuffer,
                t => CreateRWStructuredBufferMethods(t, valueType));
        }

        public static IntrinsicObjectTypeSymbol CreateRasterizerOrderedStructuredBufferType(TypeSymbol valueType)
        {
            return CreatePredefinedObjectType("RasterizerOrderedStructuredBuffer",
                "A rasterizer ordered read/write buffer, which can take a T type that is a structure.",
                PredefinedObjectType.RasterizerOrderedStructuredBuffer,
                t => CreateRWStructuredBufferMethods(t, valueType));
        }

        private static Symbol[] CreateRWStructuredBufferMethods(TypeSymbol t, TypeSymbol valueType)
        {
            return CreateStructuredBufferMethods(t, valueType)
                .Union(new Symbol[]
                {
                    new IndexerSymbol("[]", "Returns a resource variable.", t, Uint, valueType, false),
                    new FunctionSymbol("DecrementCounter", "Decrements the object's hidden counter.", t, Uint),
                    new FunctionSymbol("IncrementCounter", "Increments the object's hidden counter.", t, Uint)
                })
                .ToArray();
        }

        private static Symbol[] CreateStructuredBufferMethods(TypeSymbol t, TypeSymbol valueType)
        {
            return new Symbol[]
            {
                new FunctionSymbol("GetDimensions", "Gets the resource dimensions.", t, Void,
                    m => new[]
                    {
                        new ParameterSymbol("numStructs", "The number of structures.", m, Uint, ParameterDirection.Out),
                        new ParameterSymbol("stride", "The number of bytes in each element.", m, Uint, ParameterDirection.Out)
                    }),
                new FunctionSymbol("Load", "Reads buffer data and returns status about the operation.", t, valueType,
                    m => new[]
                    {
                        new ParameterSymbol("location", "The location of the buffer.", m, Int),
                        new ParameterSymbol("status", "The status of the operation.", m, Uint, ParameterDirection.Out)
                    }),
                new FunctionSymbol("Load", "Reads buffer data.", t, valueType,
                    m => new[]
                    {
                        new ParameterSymbol("location", "The location of the buffer.", m, Int)
                    }),
            };
        }

        public static IntrinsicObjectTypeSymbol CreateInputPatchType(TypeSymbol valueType)
        {
            return CreatePredefinedObjectType("InputPatch",
                "Represents an array of control points that are available to the hull shader as inputs.",
                PredefinedObjectType.InputPatch,
                t => new Symbol[]
                {
                    new IndexerSymbol("[]", "Returns the nth control point in the patch.", t, Uint, valueType),
                    new FieldSymbol("Length", "The number of control points.", t, Uint)
                });
        }

        public static IntrinsicObjectTypeSymbol CreateOutputPatchType(TypeSymbol valueType)
        {
            return CreatePredefinedObjectType("OutputPatch",
                "Represents an array of output control points that are available to the hull shader's patch-constant function as well as the domain shader.",
                PredefinedObjectType.OutputPatch,
                t => new Symbol[]
                {
                    new IndexerSymbol("[]", "Returns the nth control point in the patch.", t, Uint, valueType)
                });
        }

        public static IntrinsicObjectTypeSymbol CreateStreamOutputType(PredefinedObjectType type, TypeSymbol valueType)
        {
            return CreatePredefinedObjectType(type.ToString(),
                "A stream-output object is a templated object that streams data out of the geometry-shader stage.",
                type,
                t => new Symbol[]
                {
                    new FunctionSymbol("Append", "Append geometry-shader-output data to an existing stream.", t, valueType,
                        m => new[]
                        {
                            new ParameterSymbol("data", "The geometry-shader-output data to be appended.", m, valueType)
                        }),
                    new FunctionSymbol("RestartStrip", "Ends the current primitive strip and starts a new strip. If the current strip does not have enough vertices emitted to fill the primitive topology, the incomplete primitive at the end will be discarded.", t, Void)
                });
        }

        public static IntrinsicObjectTypeSymbol CreateConstantBufferType(TypeSymbol valueType)
        {
            var fields = (valueType.Kind == SymbolKind.Struct)
                ? ((StructSymbol)valueType).Members
                : ImmutableArray<Symbol>.Empty;
            return CreatePredefinedObjectType("ConstantBuffer", "",
                PredefinedObjectType.ConstantBuffer,
                t => fields);
        }
    }
}