using System;
using System.Collections.Generic;
using System.Linq;
using HlslTools.Syntax;

namespace HlslTools.Symbols
{
    public static class IntrinsicTypes
    {
        public static readonly IntrinsicScalarTypeSymbol Void;
        public static readonly IntrinsicScalarTypeSymbol String;
        public static readonly IntrinsicScalarTypeSymbol Bool;
        public static readonly IntrinsicScalarTypeSymbol Int;
        public static readonly IntrinsicScalarTypeSymbol Uint;
        public static readonly IntrinsicScalarTypeSymbol Half;
        public static readonly IntrinsicScalarTypeSymbol Float;
        public static readonly IntrinsicScalarTypeSymbol Double;

        public static readonly TypeSymbol Bool1;
        public static readonly TypeSymbol Bool2;
        public static readonly TypeSymbol Bool3;
        public static readonly TypeSymbol Bool4;

        public static readonly TypeSymbol Int1;
        public static readonly TypeSymbol Int2;
        public static readonly TypeSymbol Int3;
        public static readonly TypeSymbol Int4;

        public static readonly TypeSymbol Uint1;
        public static readonly TypeSymbol Uint2;
        public static readonly TypeSymbol Uint3;
        public static readonly TypeSymbol Uint4;

        public static readonly TypeSymbol Half1;
        public static readonly TypeSymbol Half2;
        public static readonly TypeSymbol Half3;
        public static readonly TypeSymbol Half4;

        public static readonly TypeSymbol Float1;
        public static readonly TypeSymbol Float2;
        public static readonly TypeSymbol Float3;
        public static readonly TypeSymbol Float4;

        public static readonly TypeSymbol Double1;
        public static readonly TypeSymbol Double2;
        public static readonly TypeSymbol Double3;
        public static readonly TypeSymbol Double4;

        public static readonly TypeSymbol Bool1x1;
        public static readonly TypeSymbol Bool1x2;
        public static readonly TypeSymbol Bool1x3;
        public static readonly TypeSymbol Bool1x4;
        public static readonly TypeSymbol Bool2x1;
        public static readonly TypeSymbol Bool2x2;
        public static readonly TypeSymbol Bool2x3;
        public static readonly TypeSymbol Bool2x4;
        public static readonly TypeSymbol Bool3x1;
        public static readonly TypeSymbol Bool3x2;
        public static readonly TypeSymbol Bool3x3;
        public static readonly TypeSymbol Bool3x4;
        public static readonly TypeSymbol Bool4x1;
        public static readonly TypeSymbol Bool4x2;
        public static readonly TypeSymbol Bool4x3;
        public static readonly TypeSymbol Bool4x4;

        public static readonly TypeSymbol Int1x1;
        public static readonly TypeSymbol Int1x2;
        public static readonly TypeSymbol Int1x3;
        public static readonly TypeSymbol Int1x4;
        public static readonly TypeSymbol Int2x1;
        public static readonly TypeSymbol Int2x2;
        public static readonly TypeSymbol Int2x3;
        public static readonly TypeSymbol Int2x4;
        public static readonly TypeSymbol Int3x1;
        public static readonly TypeSymbol Int3x2;
        public static readonly TypeSymbol Int3x3;
        public static readonly TypeSymbol Int3x4;
        public static readonly TypeSymbol Int4x1;
        public static readonly TypeSymbol Int4x2;
        public static readonly TypeSymbol Int4x3;
        public static readonly TypeSymbol Int4x4;

        public static readonly TypeSymbol Uint1x1;
        public static readonly TypeSymbol Uint1x2;
        public static readonly TypeSymbol Uint1x3;
        public static readonly TypeSymbol Uint1x4;
        public static readonly TypeSymbol Uint2x1;
        public static readonly TypeSymbol Uint2x2;
        public static readonly TypeSymbol Uint2x3;
        public static readonly TypeSymbol Uint2x4;
        public static readonly TypeSymbol Uint3x1;
        public static readonly TypeSymbol Uint3x2;
        public static readonly TypeSymbol Uint3x3;
        public static readonly TypeSymbol Uint3x4;
        public static readonly TypeSymbol Uint4x1;
        public static readonly TypeSymbol Uint4x2;
        public static readonly TypeSymbol Uint4x3;
        public static readonly TypeSymbol Uint4x4;

        public static readonly TypeSymbol Half1x1;
        public static readonly TypeSymbol Half1x2;
        public static readonly TypeSymbol Half1x3;
        public static readonly TypeSymbol Half1x4;
        public static readonly TypeSymbol Half2x1;
        public static readonly TypeSymbol Half2x2;
        public static readonly TypeSymbol Half2x3;
        public static readonly TypeSymbol Half2x4;
        public static readonly TypeSymbol Half3x1;
        public static readonly TypeSymbol Half3x2;
        public static readonly TypeSymbol Half3x3;
        public static readonly TypeSymbol Half3x4;
        public static readonly TypeSymbol Half4x1;
        public static readonly TypeSymbol Half4x2;
        public static readonly TypeSymbol Half4x3;
        public static readonly TypeSymbol Half4x4;

        public static readonly TypeSymbol Float1x1;
        public static readonly TypeSymbol Float1x2;
        public static readonly TypeSymbol Float1x3;
        public static readonly TypeSymbol Float1x4;
        public static readonly TypeSymbol Float2x1;
        public static readonly TypeSymbol Float2x2;
        public static readonly TypeSymbol Float2x3;
        public static readonly TypeSymbol Float2x4;
        public static readonly TypeSymbol Float3x1;
        public static readonly TypeSymbol Float3x2;
        public static readonly TypeSymbol Float3x3;
        public static readonly TypeSymbol Float3x4;
        public static readonly TypeSymbol Float4x1;
        public static readonly TypeSymbol Float4x2;
        public static readonly TypeSymbol Float4x3;
        public static readonly TypeSymbol Float4x4;

        public static readonly TypeSymbol Double1x1;
        public static readonly TypeSymbol Double1x2;
        public static readonly TypeSymbol Double1x3;
        public static readonly TypeSymbol Double1x4;
        public static readonly TypeSymbol Double2x1;
        public static readonly TypeSymbol Double2x2;
        public static readonly TypeSymbol Double2x3;
        public static readonly TypeSymbol Double2x4;
        public static readonly TypeSymbol Double3x1;
        public static readonly TypeSymbol Double3x2;
        public static readonly TypeSymbol Double3x3;
        public static readonly TypeSymbol Double3x4;
        public static readonly TypeSymbol Double4x1;
        public static readonly TypeSymbol Double4x2;
        public static readonly TypeSymbol Double4x3;
        public static readonly TypeSymbol Double4x4;

        public static readonly TypeSymbol[] AllScalarTypes;

        public static readonly TypeSymbol[] AllBoolVectorTypes;
        public static readonly TypeSymbol[] AllIntVectorTypes;
        public static readonly TypeSymbol[] AllUintVectorTypes;
        public static readonly TypeSymbol[] AllFloatVectorTypes;
        public static readonly TypeSymbol[] AllDoubleVectorTypes;
        public static readonly TypeSymbol[] AllVectorTypes;

        public static readonly TypeSymbol[] AllBoolMatrixTypes;
        public static readonly TypeSymbol[] AllIntMatrixTypes;
        public static readonly TypeSymbol[] AllUintMatrixTypes;
        public static readonly TypeSymbol[] AllFloatMatrixTypes;
        public static readonly TypeSymbol[] AllDoubleMatrixTypes;
        public static readonly TypeSymbol[] AllMatrixTypes;

        public static readonly TypeSymbol[] AllBoolTypes;
        public static readonly TypeSymbol[] AllIntTypes;
        public static readonly TypeSymbol[] AllUintTypes;
        public static readonly TypeSymbol[] AllFloatTypes;
        public static readonly TypeSymbol[] AllDoubleTypes;
        public static readonly TypeSymbol[] AllNumericTypes;

        public static readonly TypeSymbol Sampler1D;
        public static readonly TypeSymbol Sampler2D;
        public static readonly TypeSymbol Sampler3D;
        public static readonly TypeSymbol SamplerCube;
        public static readonly TypeSymbol SamplerState;
        public static readonly TypeSymbol SamplerComparisonState;
        public static readonly TypeSymbol LegacyTexture;

        public static readonly TypeSymbol BlendState;
        public static readonly TypeSymbol DepthStencilState;
        public static readonly TypeSymbol RasterizerState;

        public static readonly TypeSymbol[] AllIntrinsicTypes;

        static IntrinsicTypes()
        {
            // Scalar types.
            Void = new IntrinsicScalarTypeSymbol("void", "Represents a void value.", ScalarType.Void);
            String = new IntrinsicScalarTypeSymbol("string", "Represents a string value.", ScalarType.String);
            Bool = new IntrinsicScalarTypeSymbol("bool", "Represents a boolean value.", ScalarType.Bool, t => CreateVectorTypeFields(1, Bool, Bool1, Bool2, Bool3, Bool4));
            Int = new IntrinsicScalarTypeSymbol("int", "Represents a 32-bit signed integer value.", ScalarType.Int, t => CreateVectorTypeFields(1, Int, Int1, Int2, Int3, Int4));
            Uint = new IntrinsicScalarTypeSymbol("uint", "Represents a 32-bit unsigned integer value.", ScalarType.Uint, t => CreateVectorTypeFields(1, Uint, Uint1, Uint2, Uint3, Uint4));
            Half = new IntrinsicScalarTypeSymbol("half", "Represents a 16-bit floating point value.", ScalarType.Half, t => CreateVectorTypeFields(1, Half, Half1, Half2, Half3, Half4));
            Float = new IntrinsicScalarTypeSymbol("float", "Represents a 32-bit floating point value.", ScalarType.Float, t => CreateVectorTypeFields(1, Float, Float1, Float2, Float3, Float4));
            Double = new IntrinsicScalarTypeSymbol("double", "Represents a 64-bit floating point value.", ScalarType.Double, t => CreateVectorTypeFields(1, Double, Double1, Double2, Double3, Double4));

            // Vector types.
            Bool1 = new IntrinsicVectorTypeSymbol("bool1", "Represents a vector containing 1 boolean component.",  ScalarType.Bool, 1, t => CreateVectorTypeFields(1, Bool1, Bool, Bool2, Bool3, Bool4));
            Bool2 = new IntrinsicVectorTypeSymbol("bool2", "Represents a vector containing 2 boolean components.", ScalarType.Bool, 2, t => CreateVectorTypeFields(2, Bool2, Bool, Bool2, Bool3, Bool4));
            Bool3 = new IntrinsicVectorTypeSymbol("bool3", "Represents a vector containing 3 boolean components.", ScalarType.Bool, 3, t => CreateVectorTypeFields(3, Bool3, Bool, Bool2, Bool3, Bool4));
            Bool4 = new IntrinsicVectorTypeSymbol("bool4", "Represents a vector containing 4 boolean components.", ScalarType.Bool, 4, t => CreateVectorTypeFields(4, Bool4, Bool, Bool2, Bool3, Bool4));
            Int1 = new IntrinsicVectorTypeSymbol("int1", "Represents a vector containing 1 signed integer component.",  ScalarType.Int, 1, t => CreateVectorTypeFields(1, Int1, Int, Int2, Int3, Int4));
            Int2 = new IntrinsicVectorTypeSymbol("int2", "Represents a vector containing 2 signed integer components.", ScalarType.Int, 2, t => CreateVectorTypeFields(2, Int2, Int, Int2, Int3, Int4));
            Int3 = new IntrinsicVectorTypeSymbol("int3", "Represents a vector containing 3 signed integer components.", ScalarType.Int, 3, t => CreateVectorTypeFields(3, Int3, Int, Int2, Int3, Int4));
            Int4 = new IntrinsicVectorTypeSymbol("int4", "Represents a vector containing 4 signed integer components.", ScalarType.Int, 4, t => CreateVectorTypeFields(4, Int4, Int, Int2, Int3, Int4));
            Uint1 = new IntrinsicVectorTypeSymbol("uint1", "Represents a vector containing 1 unsigned integer component.",  ScalarType.Uint, 1, t => CreateVectorTypeFields(1, Uint1, Uint, Uint2, Uint3, Uint4));
            Uint2 = new IntrinsicVectorTypeSymbol("uint2", "Represents a vector containing 2 unsigned integer components.", ScalarType.Uint, 2, t => CreateVectorTypeFields(2, Uint2, Uint, Uint2, Uint3, Uint4));
            Uint3 = new IntrinsicVectorTypeSymbol("uint3", "Represents a vector containing 3 unsigned integer components.", ScalarType.Uint, 3, t => CreateVectorTypeFields(3, Uint3, Uint, Uint2, Uint3, Uint4));
            Uint4 = new IntrinsicVectorTypeSymbol("uint4", "Represents a vector containing 4 unsigned integer components.", ScalarType.Uint, 4, t => CreateVectorTypeFields(4, Uint4, Uint, Uint2, Uint3, Uint4));
            Half1 = new IntrinsicVectorTypeSymbol("half1", "Represents a vector containing 1 floating point component.",  ScalarType.Half, 1, t => CreateVectorTypeFields(1, Half1, Half, Half2, Half3, Half4));
            Half2 = new IntrinsicVectorTypeSymbol("half2", "Represents a vector containing 2 floating point components.", ScalarType.Half, 2, t => CreateVectorTypeFields(2, Half2, Half, Half2, Half3, Half4));
            Half3 = new IntrinsicVectorTypeSymbol("half3", "Represents a vector containing 3 floating point components.", ScalarType.Half, 3, t => CreateVectorTypeFields(3, Half3, Half, Half2, Half3, Half4));
            Half4 = new IntrinsicVectorTypeSymbol("half4", "Represents a vector containing 4 floating point components.", ScalarType.Half, 4, t => CreateVectorTypeFields(4, Half4, Half, Half2, Half3, Half4));
            Float1 = new IntrinsicVectorTypeSymbol("float1", "Represents a vector containing 1 floating point component.",  ScalarType.Float, 1, t => CreateVectorTypeFields(1, Float1, Float, Float2, Float3, Float4));
            Float2 = new IntrinsicVectorTypeSymbol("float2", "Represents a vector containing 2 floating point components.", ScalarType.Float, 2, t => CreateVectorTypeFields(2, Float2, Float, Float2, Float3, Float4));
            Float3 = new IntrinsicVectorTypeSymbol("float3", "Represents a vector containing 3 floating point components.", ScalarType.Float, 3, t => CreateVectorTypeFields(3, Float3, Float, Float2, Float3, Float4));
            Float4 = new IntrinsicVectorTypeSymbol("float4", "Represents a vector containing 4 floating point components.", ScalarType.Float, 4, t => CreateVectorTypeFields(4, Float4, Float, Float2, Float3, Float4));
            Double1 = new IntrinsicVectorTypeSymbol("double1", "Represents a vector containing 1 floating point component.",  ScalarType.Double, 1, t => CreateVectorTypeFields(1, Double1, Double, Double2, Double3, Double4));
            Double2 = new IntrinsicVectorTypeSymbol("double2", "Represents a vector containing 2 floating point components.", ScalarType.Double, 2, t => CreateVectorTypeFields(2, Double2, Double, Double2, Double3, Double4));
            Double3 = new IntrinsicVectorTypeSymbol("double3", "Represents a vector containing 3 floating point components.", ScalarType.Double, 3, t => CreateVectorTypeFields(3, Double3, Double, Double2, Double3, Double4));
            Double4 = new IntrinsicVectorTypeSymbol("double4", "Represents a vector containing 4 floating point components.", ScalarType.Double, 4, t => CreateVectorTypeFields(4, Double4, Double, Double2, Double3, Double4));

            // Matrix types.
            Bool1x1 = new IntrinsicMatrixTypeSymbol("bool1x1", "Represents a matrix containing 1 row and 1 column.",   ScalarType.Bool, 1, 1, t => CreateMatrixTypeMembers(1, 1, Bool1x1, Bool));
            Bool1x2 = new IntrinsicMatrixTypeSymbol("bool1x2", "Represents a matrix containing 1 row and 2 columns.",  ScalarType.Bool, 1, 2, t => CreateMatrixTypeMembers(1, 2, Bool1x2, Bool));
            Bool1x3 = new IntrinsicMatrixTypeSymbol("bool1x3", "Represents a matrix containing 1 row and 3 columns.",  ScalarType.Bool, 1, 3, t => CreateMatrixTypeMembers(1, 3, Bool1x3, Bool));
            Bool1x4 = new IntrinsicMatrixTypeSymbol("bool1x4", "Represents a matrix containing 1 row and 4 columns.",  ScalarType.Bool, 1, 4, t => CreateMatrixTypeMembers(1, 4, Bool1x4, Bool));
            Bool2x1 = new IntrinsicMatrixTypeSymbol("bool2x1", "Represents a matrix containing 2 rows and 1 column.",  ScalarType.Bool, 2, 1, t => CreateMatrixTypeMembers(2, 1, Bool2x1, Bool));
            Bool2x2 = new IntrinsicMatrixTypeSymbol("bool2x2", "Represents a matrix containing 2 rows and 2 columns.", ScalarType.Bool, 2, 2, t => CreateMatrixTypeMembers(2, 2, Bool2x2, Bool));
            Bool2x3 = new IntrinsicMatrixTypeSymbol("bool2x3", "Represents a matrix containing 2 rows and 3 columns.", ScalarType.Bool, 2, 3, t => CreateMatrixTypeMembers(2, 3, Bool2x3, Bool));
            Bool2x4 = new IntrinsicMatrixTypeSymbol("bool2x4", "Represents a matrix containing 2 rows and 4 columns.", ScalarType.Bool, 2, 4, t => CreateMatrixTypeMembers(2, 4, Bool2x4, Bool));
            Bool3x1 = new IntrinsicMatrixTypeSymbol("bool3x1", "Represents a matrix containing 3 rows and 1 column.",  ScalarType.Bool, 3, 1, t => CreateMatrixTypeMembers(3, 1, Bool3x1, Bool));
            Bool3x2 = new IntrinsicMatrixTypeSymbol("bool3x2", "Represents a matrix containing 3 rows and 2 columns.", ScalarType.Bool, 3, 2, t => CreateMatrixTypeMembers(3, 2, Bool3x2, Bool));
            Bool3x3 = new IntrinsicMatrixTypeSymbol("bool3x3", "Represents a matrix containing 3 rows and 3 columns.", ScalarType.Bool, 3, 3, t => CreateMatrixTypeMembers(3, 3, Bool3x3, Bool));
            Bool3x4 = new IntrinsicMatrixTypeSymbol("bool3x4", "Represents a matrix containing 3 rows and 4 columns.", ScalarType.Bool, 3, 4, t => CreateMatrixTypeMembers(3, 4, Bool3x4, Bool));
            Bool4x1 = new IntrinsicMatrixTypeSymbol("bool4x1", "Represents a matrix containing 4 rows and 1 column.",  ScalarType.Bool, 4, 1, t => CreateMatrixTypeMembers(4, 1, Bool4x1, Bool));
            Bool4x2 = new IntrinsicMatrixTypeSymbol("bool4x2", "Represents a matrix containing 4 rows and 2 columns.", ScalarType.Bool, 4, 2, t => CreateMatrixTypeMembers(4, 2, Bool4x2, Bool));
            Bool4x3 = new IntrinsicMatrixTypeSymbol("bool4x3", "Represents a matrix containing 4 rows and 3 columns.", ScalarType.Bool, 4, 3, t => CreateMatrixTypeMembers(4, 3, Bool4x3, Bool));
            Bool4x4 = new IntrinsicMatrixTypeSymbol("bool4x4", "Represents a matrix containing 4 rows and 4 columns.", ScalarType.Bool, 4, 4, t => CreateMatrixTypeMembers(4, 4, Bool4x4, Bool));
            Int1x1 = new IntrinsicMatrixTypeSymbol("int1x1", "Represents a matrix containing 1 row and 1 column.",   ScalarType.Int, 1, 1, t => CreateMatrixTypeMembers(1, 1, Int1x1, Int));
            Int1x2 = new IntrinsicMatrixTypeSymbol("int1x2", "Represents a matrix containing 1 row and 2 columns.",  ScalarType.Int, 1, 2, t => CreateMatrixTypeMembers(1, 2, Int1x2, Int));
            Int1x3 = new IntrinsicMatrixTypeSymbol("int1x3", "Represents a matrix containing 1 row and 3 columns.",  ScalarType.Int, 1, 3, t => CreateMatrixTypeMembers(1, 3, Int1x3, Int));
            Int1x4 = new IntrinsicMatrixTypeSymbol("int1x4", "Represents a matrix containing 1 row and 4 columns.",  ScalarType.Int, 1, 4, t => CreateMatrixTypeMembers(1, 4, Int1x4, Int));
            Int2x1 = new IntrinsicMatrixTypeSymbol("int2x1", "Represents a matrix containing 2 rows and 1 column.",  ScalarType.Int, 2, 1, t => CreateMatrixTypeMembers(2, 1, Int2x1, Int));
            Int2x2 = new IntrinsicMatrixTypeSymbol("int2x2", "Represents a matrix containing 2 rows and 2 columns.", ScalarType.Int, 2, 2, t => CreateMatrixTypeMembers(2, 2, Int2x2, Int));
            Int2x3 = new IntrinsicMatrixTypeSymbol("int2x3", "Represents a matrix containing 2 rows and 3 columns.", ScalarType.Int, 2, 3, t => CreateMatrixTypeMembers(2, 3, Int2x3, Int));
            Int2x4 = new IntrinsicMatrixTypeSymbol("int2x4", "Represents a matrix containing 2 rows and 4 columns.", ScalarType.Int, 2, 4, t => CreateMatrixTypeMembers(2, 4, Int2x4, Int));
            Int3x1 = new IntrinsicMatrixTypeSymbol("int3x1", "Represents a matrix containing 3 rows and 1 column.",  ScalarType.Int, 3, 1, t => CreateMatrixTypeMembers(3, 1, Int3x1, Int));
            Int3x2 = new IntrinsicMatrixTypeSymbol("int3x2", "Represents a matrix containing 3 rows and 2 columns.", ScalarType.Int, 3, 2, t => CreateMatrixTypeMembers(3, 2, Int3x2, Int));
            Int3x3 = new IntrinsicMatrixTypeSymbol("int3x3", "Represents a matrix containing 3 rows and 3 columns.", ScalarType.Int, 3, 3, t => CreateMatrixTypeMembers(3, 3, Int3x3, Int));
            Int3x4 = new IntrinsicMatrixTypeSymbol("int3x4", "Represents a matrix containing 3 rows and 4 columns.", ScalarType.Int, 3, 4, t => CreateMatrixTypeMembers(3, 4, Int3x4, Int));
            Int4x1 = new IntrinsicMatrixTypeSymbol("int4x1", "Represents a matrix containing 4 rows and 1 column.",  ScalarType.Int, 4, 1, t => CreateMatrixTypeMembers(4, 1, Int4x1, Int));
            Int4x2 = new IntrinsicMatrixTypeSymbol("int4x2", "Represents a matrix containing 4 rows and 2 columns.", ScalarType.Int, 4, 2, t => CreateMatrixTypeMembers(4, 2, Int4x2, Int));
            Int4x3 = new IntrinsicMatrixTypeSymbol("int4x3", "Represents a matrix containing 4 rows and 3 columns.", ScalarType.Int, 4, 3, t => CreateMatrixTypeMembers(4, 3, Int4x3, Int));
            Int4x4 = new IntrinsicMatrixTypeSymbol("int4x4", "Represents a matrix containing 4 rows and 4 columns.", ScalarType.Int, 4, 4, t => CreateMatrixTypeMembers(4, 4, Int4x4, Int));
            Uint1x1 = new IntrinsicMatrixTypeSymbol("uint1x1", "Represents a matrix containing 1 row and 1 column.",   ScalarType.Uint, 1, 1, t => CreateMatrixTypeMembers(1, 1, Uint1x1, Uint));
            Uint1x2 = new IntrinsicMatrixTypeSymbol("uint1x2", "Represents a matrix containing 1 row and 2 columns.",  ScalarType.Uint, 1, 2, t => CreateMatrixTypeMembers(1, 2, Uint1x2, Uint));
            Uint1x3 = new IntrinsicMatrixTypeSymbol("uint1x3", "Represents a matrix containing 1 row and 3 columns.",  ScalarType.Uint, 1, 3, t => CreateMatrixTypeMembers(1, 3, Uint1x3, Uint));
            Uint1x4 = new IntrinsicMatrixTypeSymbol("uint1x4", "Represents a matrix containing 1 row and 4 columns.",  ScalarType.Uint, 1, 4, t => CreateMatrixTypeMembers(1, 4, Uint1x4, Uint));
            Uint2x1 = new IntrinsicMatrixTypeSymbol("uint2x1", "Represents a matrix containing 2 rows and 1 column.",  ScalarType.Uint, 2, 1, t => CreateMatrixTypeMembers(2, 1, Uint2x1, Uint));
            Uint2x2 = new IntrinsicMatrixTypeSymbol("uint2x2", "Represents a matrix containing 2 rows and 2 columns.", ScalarType.Uint, 2, 2, t => CreateMatrixTypeMembers(2, 2, Uint2x2, Uint));
            Uint2x3 = new IntrinsicMatrixTypeSymbol("uint2x3", "Represents a matrix containing 2 rows and 3 columns.", ScalarType.Uint, 2, 3, t => CreateMatrixTypeMembers(2, 3, Uint2x3, Uint));
            Uint2x4 = new IntrinsicMatrixTypeSymbol("uint2x4", "Represents a matrix containing 2 rows and 4 columns.", ScalarType.Uint, 2, 4, t => CreateMatrixTypeMembers(2, 4, Uint2x4, Uint));
            Uint3x1 = new IntrinsicMatrixTypeSymbol("uint3x1", "Represents a matrix containing 3 rows and 1 column.",  ScalarType.Uint, 3, 1, t => CreateMatrixTypeMembers(3, 1, Uint3x1, Uint));
            Uint3x2 = new IntrinsicMatrixTypeSymbol("uint3x2", "Represents a matrix containing 3 rows and 2 columns.", ScalarType.Uint, 3, 2, t => CreateMatrixTypeMembers(3, 2, Uint3x2, Uint));
            Uint3x3 = new IntrinsicMatrixTypeSymbol("uint3x3", "Represents a matrix containing 3 rows and 3 columns.", ScalarType.Uint, 3, 3, t => CreateMatrixTypeMembers(3, 3, Uint3x3, Uint));
            Uint3x4 = new IntrinsicMatrixTypeSymbol("uint3x4", "Represents a matrix containing 3 rows and 4 columns.", ScalarType.Uint, 3, 4, t => CreateMatrixTypeMembers(3, 4, Uint3x4, Uint));
            Uint4x1 = new IntrinsicMatrixTypeSymbol("uint4x1", "Represents a matrix containing 4 rows and 1 column.",  ScalarType.Uint, 4, 1, t => CreateMatrixTypeMembers(4, 1, Uint4x1, Uint));
            Uint4x2 = new IntrinsicMatrixTypeSymbol("uint4x2", "Represents a matrix containing 4 rows and 2 columns.", ScalarType.Uint, 4, 2, t => CreateMatrixTypeMembers(4, 2, Uint4x2, Uint));
            Uint4x3 = new IntrinsicMatrixTypeSymbol("uint4x3", "Represents a matrix containing 4 rows and 3 columns.", ScalarType.Uint, 4, 3, t => CreateMatrixTypeMembers(4, 3, Uint4x3, Uint));
            Uint4x4 = new IntrinsicMatrixTypeSymbol("uint4x4", "Represents a matrix containing 4 rows and 4 columns.", ScalarType.Uint, 4, 4, t => CreateMatrixTypeMembers(4, 4, Uint4x4, Uint));
            Half1x1 = new IntrinsicMatrixTypeSymbol("half1x1", "Represents a matrix containing 1 row and 1 column.",   ScalarType.Half, 1, 1, t => CreateMatrixTypeMembers(1, 1, Half1x1, Half));
            Half1x2 = new IntrinsicMatrixTypeSymbol("half1x2", "Represents a matrix containing 1 row and 2 columns.",  ScalarType.Half, 1, 2, t => CreateMatrixTypeMembers(1, 2, Half1x2, Half));
            Half1x3 = new IntrinsicMatrixTypeSymbol("half1x3", "Represents a matrix containing 1 row and 3 columns.",  ScalarType.Half, 1, 3, t => CreateMatrixTypeMembers(1, 3, Half1x3, Half));
            Half1x4 = new IntrinsicMatrixTypeSymbol("half1x4", "Represents a matrix containing 1 row and 4 columns.",  ScalarType.Half, 1, 4, t => CreateMatrixTypeMembers(1, 4, Half1x4, Half));
            Half2x1 = new IntrinsicMatrixTypeSymbol("half2x1", "Represents a matrix containing 2 rows and 1 column.",  ScalarType.Half, 2, 1, t => CreateMatrixTypeMembers(2, 1, Half2x1, Half));
            Half2x2 = new IntrinsicMatrixTypeSymbol("half2x2", "Represents a matrix containing 2 rows and 2 columns.", ScalarType.Half, 2, 2, t => CreateMatrixTypeMembers(2, 2, Half2x2, Half));
            Half2x3 = new IntrinsicMatrixTypeSymbol("half2x3", "Represents a matrix containing 2 rows and 3 columns.", ScalarType.Half, 2, 3, t => CreateMatrixTypeMembers(2, 3, Half2x3, Half));
            Half2x4 = new IntrinsicMatrixTypeSymbol("half2x4", "Represents a matrix containing 2 rows and 4 columns.", ScalarType.Half, 2, 4, t => CreateMatrixTypeMembers(2, 4, Half2x4, Half));
            Half3x1 = new IntrinsicMatrixTypeSymbol("half3x1", "Represents a matrix containing 3 rows and 1 column.",  ScalarType.Half, 3, 1, t => CreateMatrixTypeMembers(3, 1, Half3x1, Half));
            Half3x2 = new IntrinsicMatrixTypeSymbol("half3x2", "Represents a matrix containing 3 rows and 2 columns.", ScalarType.Half, 3, 2, t => CreateMatrixTypeMembers(3, 2, Half3x2, Half));
            Half3x3 = new IntrinsicMatrixTypeSymbol("half3x3", "Represents a matrix containing 3 rows and 3 columns.", ScalarType.Half, 3, 3, t => CreateMatrixTypeMembers(3, 3, Half3x3, Half));
            Half3x4 = new IntrinsicMatrixTypeSymbol("half3x4", "Represents a matrix containing 3 rows and 4 columns.", ScalarType.Half, 3, 4, t => CreateMatrixTypeMembers(3, 4, Half3x4, Half));
            Half4x1 = new IntrinsicMatrixTypeSymbol("half4x1", "Represents a matrix containing 4 rows and 1 column.",  ScalarType.Half, 4, 1, t => CreateMatrixTypeMembers(4, 1, Half4x1, Half));
            Half4x2 = new IntrinsicMatrixTypeSymbol("half4x2", "Represents a matrix containing 4 rows and 2 columns.", ScalarType.Half, 4, 2, t => CreateMatrixTypeMembers(4, 2, Half4x2, Half));
            Half4x3 = new IntrinsicMatrixTypeSymbol("half4x3", "Represents a matrix containing 4 rows and 3 columns.", ScalarType.Half, 4, 3, t => CreateMatrixTypeMembers(4, 3, Half4x3, Half));
            Half4x4 = new IntrinsicMatrixTypeSymbol("half4x4", "Represents a matrix containing 4 rows and 4 columns.", ScalarType.Half, 4, 4, t => CreateMatrixTypeMembers(4, 4, Half4x4, Half));
            Float1x1 = new IntrinsicMatrixTypeSymbol("float1x1", "Represents a matrix containing 1 row and 1 column.",   ScalarType.Float, 1, 1, t => CreateMatrixTypeMembers(1, 1, Float1x1, Float));
            Float1x2 = new IntrinsicMatrixTypeSymbol("float1x2", "Represents a matrix containing 1 row and 2 columns.",  ScalarType.Float, 1, 2, t => CreateMatrixTypeMembers(1, 2, Float1x2, Float));
            Float1x3 = new IntrinsicMatrixTypeSymbol("float1x3", "Represents a matrix containing 1 row and 3 columns.",  ScalarType.Float, 1, 3, t => CreateMatrixTypeMembers(1, 3, Float1x3, Float));
            Float1x4 = new IntrinsicMatrixTypeSymbol("float1x4", "Represents a matrix containing 1 row and 4 columns.",  ScalarType.Float, 1, 4, t => CreateMatrixTypeMembers(1, 4, Float1x4, Float));
            Float2x1 = new IntrinsicMatrixTypeSymbol("float2x1", "Represents a matrix containing 2 rows and 1 column.",  ScalarType.Float, 2, 1, t => CreateMatrixTypeMembers(2, 1, Float2x1, Float));
            Float2x2 = new IntrinsicMatrixTypeSymbol("float2x2", "Represents a matrix containing 2 rows and 2 columns.", ScalarType.Float, 2, 2, t => CreateMatrixTypeMembers(2, 2, Float2x2, Float));
            Float2x3 = new IntrinsicMatrixTypeSymbol("float2x3", "Represents a matrix containing 2 rows and 3 columns.", ScalarType.Float, 2, 3, t => CreateMatrixTypeMembers(2, 3, Float2x3, Float));
            Float2x4 = new IntrinsicMatrixTypeSymbol("float2x4", "Represents a matrix containing 2 rows and 4 columns.", ScalarType.Float, 2, 4, t => CreateMatrixTypeMembers(2, 4, Float2x4, Float));
            Float3x1 = new IntrinsicMatrixTypeSymbol("float3x1", "Represents a matrix containing 3 rows and 1 column.",  ScalarType.Float, 3, 1, t => CreateMatrixTypeMembers(3, 1, Float3x1, Float));
            Float3x2 = new IntrinsicMatrixTypeSymbol("float3x2", "Represents a matrix containing 3 rows and 2 columns.", ScalarType.Float, 3, 2, t => CreateMatrixTypeMembers(3, 2, Float3x2, Float));
            Float3x3 = new IntrinsicMatrixTypeSymbol("float3x3", "Represents a matrix containing 3 rows and 3 columns.", ScalarType.Float, 3, 3, t => CreateMatrixTypeMembers(3, 3, Float3x3, Float));
            Float3x4 = new IntrinsicMatrixTypeSymbol("float3x4", "Represents a matrix containing 3 rows and 4 columns.", ScalarType.Float, 3, 4, t => CreateMatrixTypeMembers(3, 4, Float3x4, Float));
            Float4x1 = new IntrinsicMatrixTypeSymbol("float4x1", "Represents a matrix containing 4 rows and 1 column.",  ScalarType.Float, 4, 1, t => CreateMatrixTypeMembers(4, 1, Float4x1, Float));
            Float4x2 = new IntrinsicMatrixTypeSymbol("float4x2", "Represents a matrix containing 4 rows and 2 columns.", ScalarType.Float, 4, 2, t => CreateMatrixTypeMembers(4, 2, Float4x2, Float));
            Float4x3 = new IntrinsicMatrixTypeSymbol("float4x3", "Represents a matrix containing 4 rows and 3 columns.", ScalarType.Float, 4, 3, t => CreateMatrixTypeMembers(4, 3, Float4x3, Float));
            Float4x4 = new IntrinsicMatrixTypeSymbol("float4x4", "Represents a matrix containing 4 rows and 4 columns.", ScalarType.Float, 4, 4, t => CreateMatrixTypeMembers(4, 4, Float4x4, Float));
            Double1x1 = new IntrinsicMatrixTypeSymbol("double1x1", "Represents a matrix containing 1 row and 1 column.",   ScalarType.Double, 1, 1, t => CreateMatrixTypeMembers(1, 1, Double1x1, Double));
            Double1x2 = new IntrinsicMatrixTypeSymbol("double1x2", "Represents a matrix containing 1 row and 2 columns.",  ScalarType.Double, 1, 2, t => CreateMatrixTypeMembers(1, 2, Double1x2, Double));
            Double1x3 = new IntrinsicMatrixTypeSymbol("double1x3", "Represents a matrix containing 1 row and 3 columns.",  ScalarType.Double, 1, 3, t => CreateMatrixTypeMembers(1, 3, Double1x3, Double));
            Double1x4 = new IntrinsicMatrixTypeSymbol("double1x4", "Represents a matrix containing 1 row and 4 columns.",  ScalarType.Double, 1, 4, t => CreateMatrixTypeMembers(1, 4, Double1x4, Double));
            Double2x1 = new IntrinsicMatrixTypeSymbol("double2x1", "Represents a matrix containing 2 rows and 1 column.",  ScalarType.Double, 2, 1, t => CreateMatrixTypeMembers(2, 1, Double2x1, Double));
            Double2x2 = new IntrinsicMatrixTypeSymbol("double2x2", "Represents a matrix containing 2 rows and 2 columns.", ScalarType.Double, 2, 2, t => CreateMatrixTypeMembers(2, 2, Double2x2, Double));
            Double2x3 = new IntrinsicMatrixTypeSymbol("double2x3", "Represents a matrix containing 2 rows and 3 columns.", ScalarType.Double, 2, 3, t => CreateMatrixTypeMembers(2, 3, Double2x3, Double));
            Double2x4 = new IntrinsicMatrixTypeSymbol("double2x4", "Represents a matrix containing 2 rows and 4 columns.", ScalarType.Double, 2, 4, t => CreateMatrixTypeMembers(2, 4, Double2x4, Double));
            Double3x1 = new IntrinsicMatrixTypeSymbol("double3x1", "Represents a matrix containing 3 rows and 1 column.",  ScalarType.Double, 3, 1, t => CreateMatrixTypeMembers(3, 1, Double3x1, Double));
            Double3x2 = new IntrinsicMatrixTypeSymbol("double3x2", "Represents a matrix containing 3 rows and 2 columns.", ScalarType.Double, 3, 2, t => CreateMatrixTypeMembers(3, 2, Double3x2, Double));
            Double3x3 = new IntrinsicMatrixTypeSymbol("double3x3", "Represents a matrix containing 3 rows and 3 columns.", ScalarType.Double, 3, 3, t => CreateMatrixTypeMembers(3, 3, Double3x3, Double));
            Double3x4 = new IntrinsicMatrixTypeSymbol("double3x4", "Represents a matrix containing 3 rows and 4 columns.", ScalarType.Double, 3, 4, t => CreateMatrixTypeMembers(3, 4, Double3x4, Double));
            Double4x1 = new IntrinsicMatrixTypeSymbol("double4x1", "Represents a matrix containing 4 rows and 1 column.",  ScalarType.Double, 4, 1, t => CreateMatrixTypeMembers(4, 1, Double4x1, Double));
            Double4x2 = new IntrinsicMatrixTypeSymbol("double4x2", "Represents a matrix containing 4 rows and 2 columns.", ScalarType.Double, 4, 2, t => CreateMatrixTypeMembers(4, 2, Double4x2, Double));
            Double4x3 = new IntrinsicMatrixTypeSymbol("double4x3", "Represents a matrix containing 4 rows and 3 columns.", ScalarType.Double, 4, 3, t => CreateMatrixTypeMembers(4, 3, Double4x3, Double));
            Double4x4 = new IntrinsicMatrixTypeSymbol("double4x4", "Represents a matrix containing 4 rows and 4 columns.", ScalarType.Double, 4, 4, t => CreateMatrixTypeMembers(4, 4, Double4x4, Double));

            AllScalarTypes = new[]
            {
                Bool,
                Uint,
                Int,
                Half,
                Float,
                Double
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

            AllVectorTypes = new[]
            {
                Bool1,
                Bool2,
                Bool3,
                Bool4,
                Int1,
                Int2,
                Int3,
                Int4,
                Uint1,
                Uint2,
                Uint3,
                Uint4,
                Half1,
                Half2,
                Half3,
                Half4,
                Float1,
                Float2,
                Float3,
                Float4,
                Double1,
                Double2,
                Double3,
                Double4
            };

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

            AllMatrixTypes = new[]
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
                Bool4x4,
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
                Int4x4,
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
                Uint4x4,
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
                Half4x4,
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
                Float4x4,
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

            AllBoolTypes = new[] { Bool }
                .Union(AllBoolVectorTypes)
                .Union(AllBoolMatrixTypes)
                .ToArray();

            AllIntTypes = new[] { Int }
                .Union(AllIntVectorTypes)
                .Union(AllIntMatrixTypes)
                .ToArray();

            AllUintTypes = new[] { Uint }
                .Union(AllUintVectorTypes)
                .Union(AllUintMatrixTypes)
                .ToArray();

            AllFloatTypes = new[] { Float }
                .Union(AllFloatVectorTypes)
                .Union(AllFloatMatrixTypes)
                .ToArray();

            AllDoubleTypes = new[] { Double }
                .Union(AllDoubleVectorTypes)
                .Union(AllDoubleMatrixTypes)
                .ToArray();

            AllNumericTypes = AllScalarTypes
                .Union(AllVectorTypes)
                .Union(AllMatrixTypes)
                .ToArray();

            Sampler1D = new IntrinsicObjectTypeSymbol("sampler1D", "");
            Sampler2D = new IntrinsicObjectTypeSymbol("sampler2D", "");
            Sampler3D = new IntrinsicObjectTypeSymbol("sampler3D", "");
            SamplerCube = new IntrinsicObjectTypeSymbol("samplerCUBE", "");
            SamplerState = new IntrinsicObjectTypeSymbol("SamplerState", "");
            SamplerComparisonState = new IntrinsicObjectTypeSymbol("SamplerComparisonState", "");

            LegacyTexture = new IntrinsicObjectTypeSymbol("texture", "");

            BlendState = new IntrinsicObjectTypeSymbol("BlendState", "");
            DepthStencilState = new IntrinsicObjectTypeSymbol("DepthStencilState", "");
            RasterizerState = new IntrinsicObjectTypeSymbol("RasterizerState", "");

            AllIntrinsicTypes = AllNumericTypes
                .Union(new[] { Sampler1D, Sampler2D, Sampler3D, SamplerCube, SamplerState, SamplerComparisonState, LegacyTexture })
                .Union(new[] { BlendState, DepthStencilState, RasterizerState })
                .ToArray();
        }

        private static IEnumerable<FieldSymbol> CreateVectorTypeFields(int numComponents,
            TypeSymbol vectorType, TypeSymbol v1, TypeSymbol v2, TypeSymbol v3, TypeSymbol v4)
        {
            var componentNameSets = new[] { "xyzw", "rgba" }.Select(x => x.Substring(0, numComponents).ToCharArray()).ToList();
            var vectorTypes = new[] { v1, v2, v3, v4 };

            foreach (var componentNameSet in componentNameSets)
                for (var i = 0; i < 4; i++)
                    foreach (var namePermutation in GetComponentNamePermutations(componentNameSet, i + 1))
                        yield return new FieldSymbol(namePermutation, "", vectorType, vectorTypes[i]);
        }

        private static IEnumerable<string> GetComponentNamePermutations(char[] components, int num)
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
                default :
                    throw new ArgumentOutOfRangeException("num");
            }
        }

        private static IEnumerable<Symbol> CreateMatrixTypeMembers(int numRows, int numColumns, TypeSymbol matrixType, IntrinsicScalarTypeSymbol fieldType)
        {
            // TODO: Support composite fields like _m00_m01.

            yield return new IndexerSymbol("[]", "", matrixType, GetVectorType(fieldType.ScalarType, numColumns));

            for (var row = 0; row < numRows; row++)
                for (var col = 0; col < numColumns; col++)
                {
                    yield return new FieldSymbol($"_m{row}{col}", "", matrixType, fieldType);
                    yield return new FieldSymbol($"_{row + 1}{col + 1}", "", matrixType, fieldType);
                }
        }

        public static TypeSymbol GetVectorType(ScalarType scalarType, int numComponents)
        {
            return AllVectorTypes[(((int)scalarType - 1) * 4) + (numComponents - 1)];
        }

        public static TypeSymbol GetMatrixType(ScalarType scalarType, int numRows, int numCols)
        {
            return AllMatrixTypes[(((int)scalarType - 1) * 16) + ((numRows - 1) * 4) + (numCols - 1)];
        }

        public static IntrinsicTypeSymbol CreateTextureType(PredefinedObjectType textureType, TypeSymbol valueType, ScalarType scalarType)
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

            return new IntrinsicObjectTypeSymbol(name, documentation, t => CreateTextureMethods(textureType, t, valueType, scalarType));
        }

        private static IEnumerable<FunctionSymbol> CreateTextureMethods(PredefinedObjectType textureType, TypeSymbol parent, TypeSymbol valueType, ScalarType scalarType)
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
                    if (textureType == PredefinedObjectType.Texture2D || textureType == PredefinedObjectType.Texture2DArray)
                    {
                        yield return new FunctionSymbol("Gather", "Gets the four samples (red component only) that would be used for bilinear interpolation when sampling a texture.", parent,
                            GetVectorType(scalarType, 4), m => new[]
                            {
                                new ParameterSymbol("samplerState", "A sampler state.", m, SamplerState),
                                new ParameterSymbol("location", "The texture coordinates.", m, locationType),
                                new ParameterSymbol("offset", "An optional texture coordinate offset, which can be used for any texture-object types. The offset is applied to the location before sampling.", m, offsetType)
                            });
                    }
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
                    TypeSymbol intLocationType;
                    switch (textureType)
                    {
                        case PredefinedObjectType.Buffer:
                            intLocationType = Int;
                            break;
                        case PredefinedObjectType.Texture1D:
                        case PredefinedObjectType.Texture2DMS:
                            intLocationType = Int2;
                            break;
                        case PredefinedObjectType.Texture1DArray:
                        case PredefinedObjectType.Texture2D:
                        case PredefinedObjectType.Texture2DMSArray:
                            intLocationType = Int3;
                            break;
                        case PredefinedObjectType.Texture2DArray:
                        case PredefinedObjectType.Texture3D:
                            intLocationType = Int4;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    yield return new FunctionSymbol("Load", "Reads texel data without any filtering or sampling.", parent,
                        valueType, m => new[]
                        {
                            new ParameterSymbol("location", "The texture coordinates; the last component specifies the mipmap level. This method uses a 0-based coordinate system and not a 0.0-1.0 UV system. ", m, intLocationType)
                        });
                    switch (textureType)
                    {
                        case PredefinedObjectType.Texture2DMS:
                        case PredefinedObjectType.Texture2DMSArray:
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

        public static TypeSymbol CreateAppendStructuredBufferType(TypeSymbol valueType)
        {
            return new IntrinsicObjectTypeSymbol("AppendStructuredBuffer",
                "Output buffer that appears as a stream the shader may append to. Only structured buffers can take T types that are structures.",
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

        private static TypeSymbol _byteAddressBuffer;

        public static TypeSymbol ByteAddressBuffer => _byteAddressBuffer ?? (_byteAddressBuffer = CreateByteAddressBufferType());

        private static TypeSymbol CreateByteAddressBufferType()
        {
            return new IntrinsicObjectTypeSymbol("ByteAddressBuffer",
                "A read-only buffer that is indexed in bytes.",
                t => new[]
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
                        }),
                });
        }

        public static TypeSymbol CreateConsumeStructuredBufferType(TypeSymbol valueType)
        {
            return new IntrinsicObjectTypeSymbol("ConsumeStructuredBuffer",
                "An input buffer that appears as a stream the shader may pull values from. Only structured buffers can take T types that are structures.",
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

        public static TypeSymbol CreateStructuredBufferType(TypeSymbol valueType)
        {
            return new IntrinsicObjectTypeSymbol("StructuredBuffer",
                "A read-only buffer, which can take a T type that is a structure.",
                t => new[]
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
                });
        }

        public static TypeSymbol CreateInputPatchType(TypeSymbol valueType)
        {
            return new IntrinsicObjectTypeSymbol("InputPatch",
                "Represents an array of control points that are available to the hull shader as inputs.",
                t => new Symbol[]
                {
                    new IndexerSymbol("[]", "Returns the nth control point in the patch.", t, valueType),
                    new FieldSymbol("Length", "The number of control points.", t, Uint)
                });
        }

        public static TypeSymbol CreateOutputPatchType(TypeSymbol valueType)
        {
            return new IntrinsicObjectTypeSymbol("OutputPatch",
                "Represents an array of output control points that are available to the hull shader's patch-constant function as well as the domain shader.",
                t => new Symbol[]
                {
                    new IndexerSymbol("[]", "Returns the nth control point in the patch.", t, valueType)
                });
        }
    }
}