using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Cryptography;
using System.Xml.Linq;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Symbols;

namespace ShaderTools.CodeAnalysis.Hlsl.Symbols
{
	internal class IntrinsicBaseType {

		public IntrinsicScalarTypeSymbol baseSymbol;
		public IntrinsicVectorTypeSymbol[] vectorSymbols;
		public IntrinsicMatrixTypeSymbol[] matrixSymbols;

		public IntrinsicMatrixTypeSymbol MatrixAt(int i, int j) {
			return matrixSymbols[i + j * 4];
		}

		public IntrinsicNumericTypeSymbol[] All { 
			get {
				return 
					new IntrinsicNumericTypeSymbol[] { baseSymbol }
					.Union(vectorSymbols)
					.Union(matrixSymbols)
					.ToArray();
			} 
		}
	}

	//ScalarType except excluding types that can't have vectors/matrices made out of them
	enum EIntrinsicBaseType {

		Bool,

		Int,
		Uint,
		Int64_t,
		Uint64_t,

		Half,
		Float,
		Double,

		Min16Float,
		Min10Float,

		Min16Int,
		Min12Int,

		Min16Uint,

		Count
	}

	internal struct IntrinsicTypeDesc {
		public string name, desc;
		public ScalarType type;
	}

	internal static class IntrinsicTypes
    {
        public static readonly IntrinsicScalarTypeSymbol Void;
        public static readonly IntrinsicScalarTypeSymbol String;

		public static readonly List<IntrinsicBaseType> BaseTypes;

		public static readonly List<IntrinsicScalarTypeSymbol> AllScalarTypes;
        public static readonly List<IntrinsicVectorTypeSymbol> AllVectorTypes;
        public static readonly List<IntrinsicMatrixTypeSymbol> AllMatrixTypes;

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

        // DXR Structs

        public static readonly IntrinsicObjectTypeSymbol BuiltInTriangleIntersectionAttributes;
        public static readonly IntrinsicObjectTypeSymbol RayDesc;
        public static readonly IntrinsicObjectTypeSymbol RaytracingAccelerationStructure;

        public static readonly TypeSymbol[] AllTypes;

		//Helper to make vectors, matrices and base types

		private static void MakeTypes(List<IntrinsicTypeDesc> types) {

			foreach (IntrinsicTypeDesc type in types) {

				IntrinsicBaseType baseType = new IntrinsicBaseType() {
					baseSymbol = new IntrinsicScalarTypeSymbol(type.name, $"Represents a {type.desc} value.", type.type)
				};

				AllScalarTypes.Add(baseType.baseSymbol);
				BaseTypes.Add(baseType);

				baseType.vectorSymbols = new IntrinsicVectorTypeSymbol[4];

				for (int i = 1; i <= 4; ++i) {

					string s = i == 1 ? "" : "s";

					baseType.vectorSymbols[i - 1] = new IntrinsicVectorTypeSymbol(
						$"{type.name}{i}", $"Represents a vector containing {i} {type.desc} component{s}.",
						type.type,
						i
					);

					AllVectorTypes.Add(baseType.vectorSymbols[i - 1]);
				}
			}
			
			int k = 0;

			foreach (IntrinsicTypeDesc type in types) {

				IntrinsicBaseType baseType = BaseTypes[k];

				baseType.baseSymbol.AddMembers(
					CreateScalarTypeFields(
						1,
						baseType.baseSymbol,
						baseType.vectorSymbols[0],
						baseType.vectorSymbols[1],
						baseType.vectorSymbols[2],
						baseType.vectorSymbols[3]
					)
				);

				for (int i = 0; i < 4; ++i)
					baseType.vectorSymbols[i].AddMembers(
						CreateVectorTypeFields(
							i + 1,
							baseType.vectorSymbols[i],
							baseType.baseSymbol,
							baseType.vectorSymbols[0],
							baseType.vectorSymbols[1],
							baseType.vectorSymbols[2],
							baseType.vectorSymbols[3]
						)
					);

				baseType.matrixSymbols = new IntrinsicMatrixTypeSymbol[4 * 4];

				for (int j = 1; j <= 4; ++j) {
					for (int i = 1; i <= 4; ++i) {

						string rows = i == 1 ? "" : "s";
						string cols = j == 1 ? "" : "s";
						int id = (i - 1) + ((j - 1) * 4);

						baseType.matrixSymbols[id] = new IntrinsicMatrixTypeSymbol(
							$"{type.name}{i}x{j}", $"Represents a matrix containing {i} row{rows} and {j} column{cols} of {type.desc} components.",
							type.type,
							i, j
						);

						baseType.matrixSymbols[id].AddMembers(CreateMatrixTypeMembers(
							baseType.matrixSymbols[id],
							baseType.vectorSymbols[0],
							baseType.vectorSymbols[1],
							baseType.vectorSymbols[2],
							baseType.vectorSymbols[3]
						));

					}
				}
				
				for (int j = 0; j < 4; ++j)
					for (int i = 0; i < 4; ++i)
						AllMatrixTypes.Add(baseType.matrixSymbols[j + i * 4]);

				++k;
			}
		}

		//Properties to ensure single references are still working

		public static IntrinsicScalarTypeSymbol Bool { get { return BaseTypes[(int)EIntrinsicBaseType.Bool].baseSymbol; } }

		public static IntrinsicScalarTypeSymbol Int { get { return BaseTypes[(int)EIntrinsicBaseType.Int].baseSymbol; } }
		public static IntrinsicScalarTypeSymbol Uint { get { return BaseTypes[(int)EIntrinsicBaseType.Uint].baseSymbol; } }
		public static IntrinsicScalarTypeSymbol Int64_t { get { return BaseTypes[(int)EIntrinsicBaseType.Int64_t].baseSymbol; } }
		public static IntrinsicScalarTypeSymbol Uint64_t { get { return BaseTypes[(int)EIntrinsicBaseType.Uint64_t].baseSymbol; } }

		public static IntrinsicScalarTypeSymbol Half { get { return BaseTypes[(int)EIntrinsicBaseType.Half].baseSymbol; } }
		public static IntrinsicScalarTypeSymbol Float { get { return BaseTypes[(int)EIntrinsicBaseType.Float].baseSymbol; } }
		public static IntrinsicScalarTypeSymbol Double { get { return BaseTypes[(int)EIntrinsicBaseType.Double].baseSymbol; } }

		public static IntrinsicScalarTypeSymbol Min16Float { get { return BaseTypes[(int)EIntrinsicBaseType.Min16Float].baseSymbol; } }
		public static IntrinsicScalarTypeSymbol Min10Float { get { return BaseTypes[(int)EIntrinsicBaseType.Min10Float].baseSymbol; } }

		public static IntrinsicScalarTypeSymbol Min16Int { get { return BaseTypes[(int)EIntrinsicBaseType.Min16Int].baseSymbol; } }
		public static IntrinsicScalarTypeSymbol Min12Int { get { return BaseTypes[(int)EIntrinsicBaseType.Min12Int].baseSymbol; } }

		public static IntrinsicScalarTypeSymbol Min16Uint { get { return BaseTypes[(int)EIntrinsicBaseType.Min16Uint].baseSymbol; } }

		//Minimal vectors, otherwise you have to manually get them from BaseTypes.

		public static IntrinsicVectorTypeSymbol Int1 { get { return BaseTypes[(int)EIntrinsicBaseType.Int].vectorSymbols[0]; } }
		public static IntrinsicVectorTypeSymbol Uint1 { get { return BaseTypes[(int)EIntrinsicBaseType.Uint].vectorSymbols[0]; } }
		public static IntrinsicVectorTypeSymbol Float1 { get { return BaseTypes[(int)EIntrinsicBaseType.Float].vectorSymbols[0]; } }

		public static IntrinsicVectorTypeSymbol Int2 { get { return BaseTypes[(int)EIntrinsicBaseType.Int].vectorSymbols[1]; } }
		public static IntrinsicVectorTypeSymbol Uint2 { get { return BaseTypes[(int)EIntrinsicBaseType.Uint].vectorSymbols[1]; } }
		public static IntrinsicVectorTypeSymbol Float2 { get { return BaseTypes[(int)EIntrinsicBaseType.Float].vectorSymbols[1]; } }
		public static IntrinsicVectorTypeSymbol Double2 { get { return BaseTypes[(int)EIntrinsicBaseType.Double].vectorSymbols[1]; } }

		public static IntrinsicVectorTypeSymbol Int3 { get { return BaseTypes[(int)EIntrinsicBaseType.Int].vectorSymbols[2]; } }
		public static IntrinsicVectorTypeSymbol Uint3 { get { return BaseTypes[(int)EIntrinsicBaseType.Uint].vectorSymbols[2]; } }
		public static IntrinsicVectorTypeSymbol Float3 { get { return BaseTypes[(int)EIntrinsicBaseType.Float].vectorSymbols[2]; } }

		public static IntrinsicVectorTypeSymbol Int4 { get { return BaseTypes[(int)EIntrinsicBaseType.Int].vectorSymbols[3]; } }
		public static IntrinsicVectorTypeSymbol Uint4 { get { return BaseTypes[(int)EIntrinsicBaseType.Uint].vectorSymbols[3]; } }
		public static IntrinsicVectorTypeSymbol Float4 { get { return BaseTypes[(int)EIntrinsicBaseType.Float].vectorSymbols[3]; } }

		//Minimal matrices, otherwise you have to manually get them from BaseTypes.

		public static IntrinsicMatrixTypeSymbol Bool1x1 { get { return BaseTypes[(int)EIntrinsicBaseType.Bool].MatrixAt(0, 0); } }
		public static IntrinsicMatrixTypeSymbol Int1x1 { get { return BaseTypes[(int)EIntrinsicBaseType.Int].MatrixAt(0, 0); } }
		public static IntrinsicMatrixTypeSymbol Float1x1 { get { return BaseTypes[(int)EIntrinsicBaseType.Float].MatrixAt(0, 0); } }

		public static IntrinsicMatrixTypeSymbol Bool2x1 { get { return BaseTypes[(int)EIntrinsicBaseType.Bool].MatrixAt(1, 0); } }
		public static IntrinsicMatrixTypeSymbol Int2x1 { get { return BaseTypes[(int)EIntrinsicBaseType.Int].MatrixAt(1, 0); } }
		public static IntrinsicMatrixTypeSymbol Float2x1 { get { return BaseTypes[(int)EIntrinsicBaseType.Float].MatrixAt(1, 0); } }

		public static IntrinsicMatrixTypeSymbol Bool3x1 { get { return BaseTypes[(int)EIntrinsicBaseType.Bool].MatrixAt(2, 0); } }
		public static IntrinsicMatrixTypeSymbol Int3x1 { get { return BaseTypes[(int)EIntrinsicBaseType.Int].MatrixAt(2, 0); } }
		public static IntrinsicMatrixTypeSymbol Float3x1 { get { return BaseTypes[(int)EIntrinsicBaseType.Float].MatrixAt(2, 0); } }

		public static IntrinsicMatrixTypeSymbol Bool4x1 { get { return BaseTypes[(int)EIntrinsicBaseType.Bool].MatrixAt(3, 0); } }
		public static IntrinsicMatrixTypeSymbol Int4x1 { get { return BaseTypes[(int)EIntrinsicBaseType.Int].MatrixAt(3, 0); } }
		public static IntrinsicMatrixTypeSymbol Float4x1 { get { return BaseTypes[(int)EIntrinsicBaseType.Float].MatrixAt(3, 0); } }

		public static IntrinsicMatrixTypeSymbol Bool1x2 { get { return BaseTypes[(int)EIntrinsicBaseType.Bool].MatrixAt(0, 1); } }
		public static IntrinsicMatrixTypeSymbol Int1x2 { get { return BaseTypes[(int)EIntrinsicBaseType.Int].MatrixAt(0, 1); } }
		public static IntrinsicMatrixTypeSymbol Float1x2 { get { return BaseTypes[(int)EIntrinsicBaseType.Float].MatrixAt(0, 1); } }

		public static IntrinsicMatrixTypeSymbol Bool2x2 { get { return BaseTypes[(int)EIntrinsicBaseType.Bool].MatrixAt(1, 1); } }
		public static IntrinsicMatrixTypeSymbol Int2x2 { get { return BaseTypes[(int)EIntrinsicBaseType.Int].MatrixAt(1, 1); } }
		public static IntrinsicMatrixTypeSymbol Float2x2 { get { return BaseTypes[(int)EIntrinsicBaseType.Float].MatrixAt(1, 1); } }

		public static IntrinsicMatrixTypeSymbol Bool3x2 { get { return BaseTypes[(int)EIntrinsicBaseType.Bool].MatrixAt(2, 1); } }
		public static IntrinsicMatrixTypeSymbol Int3x2 { get { return BaseTypes[(int)EIntrinsicBaseType.Int].MatrixAt(2, 1); } }
		public static IntrinsicMatrixTypeSymbol Float3x2 { get { return BaseTypes[(int)EIntrinsicBaseType.Float].MatrixAt(2, 1); } }

		public static IntrinsicMatrixTypeSymbol Bool4x2 { get { return BaseTypes[(int)EIntrinsicBaseType.Bool].MatrixAt(3, 1); } }
		public static IntrinsicMatrixTypeSymbol Int4x2 { get { return BaseTypes[(int)EIntrinsicBaseType.Int].MatrixAt(3, 1); } }
		public static IntrinsicMatrixTypeSymbol Float4x2 { get { return BaseTypes[(int)EIntrinsicBaseType.Float].MatrixAt(3, 1); } }

		public static IntrinsicMatrixTypeSymbol Bool1x3 { get { return BaseTypes[(int)EIntrinsicBaseType.Bool].MatrixAt(0, 2); } }
		public static IntrinsicMatrixTypeSymbol Int1x3 { get { return BaseTypes[(int)EIntrinsicBaseType.Int].MatrixAt(0, 2); } }
		public static IntrinsicMatrixTypeSymbol Float1x3 { get { return BaseTypes[(int)EIntrinsicBaseType.Float].MatrixAt(0, 2); } }

		public static IntrinsicMatrixTypeSymbol Bool2x3 { get { return BaseTypes[(int)EIntrinsicBaseType.Bool].MatrixAt(1, 2); } }
		public static IntrinsicMatrixTypeSymbol Int2x3 { get { return BaseTypes[(int)EIntrinsicBaseType.Int].MatrixAt(1, 2); } }
		public static IntrinsicMatrixTypeSymbol Float2x3 { get { return BaseTypes[(int)EIntrinsicBaseType.Float].MatrixAt(1, 2); } }

		public static IntrinsicMatrixTypeSymbol Bool3x3 { get { return BaseTypes[(int)EIntrinsicBaseType.Bool].MatrixAt(2, 2); } }
		public static IntrinsicMatrixTypeSymbol Int3x3 { get { return BaseTypes[(int)EIntrinsicBaseType.Int].MatrixAt(2, 2); } }
		public static IntrinsicMatrixTypeSymbol Float3x3 { get { return BaseTypes[(int)EIntrinsicBaseType.Float].MatrixAt(2, 2); } }

		public static IntrinsicMatrixTypeSymbol Bool4x3 { get { return BaseTypes[(int)EIntrinsicBaseType.Bool].MatrixAt(3, 2); } }
		public static IntrinsicMatrixTypeSymbol Int4x3 { get { return BaseTypes[(int)EIntrinsicBaseType.Int].MatrixAt(3, 2); } }
		public static IntrinsicMatrixTypeSymbol Float4x3 { get { return BaseTypes[(int)EIntrinsicBaseType.Float].MatrixAt(3, 2); } }

		public static IntrinsicMatrixTypeSymbol Bool1x4 { get { return BaseTypes[(int)EIntrinsicBaseType.Bool].MatrixAt(0, 3); } }
		public static IntrinsicMatrixTypeSymbol Int1x4 { get { return BaseTypes[(int)EIntrinsicBaseType.Int].MatrixAt(0, 3); } }
		public static IntrinsicMatrixTypeSymbol Float1x4 { get { return BaseTypes[(int)EIntrinsicBaseType.Float].MatrixAt(0, 3); } }

		public static IntrinsicMatrixTypeSymbol Bool2x4 { get { return BaseTypes[(int)EIntrinsicBaseType.Bool].MatrixAt(1, 3); } }
		public static IntrinsicMatrixTypeSymbol Int2x4 { get { return BaseTypes[(int)EIntrinsicBaseType.Int].MatrixAt(1, 3); } }
		public static IntrinsicMatrixTypeSymbol Float2x4 { get { return BaseTypes[(int)EIntrinsicBaseType.Float].MatrixAt(1, 3); } }

		public static IntrinsicMatrixTypeSymbol Bool3x4 { get { return BaseTypes[(int)EIntrinsicBaseType.Bool].MatrixAt(2, 3); } }
		public static IntrinsicMatrixTypeSymbol Int3x4 { get { return BaseTypes[(int)EIntrinsicBaseType.Int].MatrixAt(2, 3); } }
		public static IntrinsicMatrixTypeSymbol Float3x4 { get { return BaseTypes[(int)EIntrinsicBaseType.Float].MatrixAt(2, 3); } }

		public static IntrinsicMatrixTypeSymbol Bool4x4 { get { return BaseTypes[(int)EIntrinsicBaseType.Bool].MatrixAt(3, 3); } }
		public static IntrinsicMatrixTypeSymbol Int4x4 { get { return BaseTypes[(int)EIntrinsicBaseType.Int].MatrixAt(3, 3); } }
		public static IntrinsicMatrixTypeSymbol Float4x4 { get { return BaseTypes[(int)EIntrinsicBaseType.Float].MatrixAt(3, 3); } }

		public static IntrinsicNumericTypeSymbol[] AllBoolTypes { get { return BaseTypes[(int)EIntrinsicBaseType.Bool].All; } }
		public static IntrinsicNumericTypeSymbol[] AllIntTypes { get { return BaseTypes[(int)EIntrinsicBaseType.Int].All; } }
		public static IntrinsicNumericTypeSymbol[] AllUintTypes { get { return BaseTypes[(int)EIntrinsicBaseType.Uint].All; } }
		public static IntrinsicNumericTypeSymbol[] AllFloatTypes { get { return BaseTypes[(int)EIntrinsicBaseType.Float].All; } }
		public static IntrinsicNumericTypeSymbol[] AllDoubleTypes { get { return BaseTypes[(int)EIntrinsicBaseType.Double].All; } }

		public static IntrinsicVectorTypeSymbol[] AllIntVectorTypes { get { return BaseTypes[(int)EIntrinsicBaseType.Int].vectorSymbols; } }
		public static IntrinsicVectorTypeSymbol[] AllUintVectorTypes { get { return BaseTypes[(int)EIntrinsicBaseType.Uint].vectorSymbols; } }
		public static IntrinsicVectorTypeSymbol[] AllFloatVectorTypes { get { return BaseTypes[(int)EIntrinsicBaseType.Float].vectorSymbols; } }
		public static IntrinsicVectorTypeSymbol[] AllDoubleVectorTypes { get { return BaseTypes[(int)EIntrinsicBaseType.Double].vectorSymbols; } }

		public static IntrinsicMatrixTypeSymbol[] AllBoolMatrixTypes { get { return BaseTypes[(int)EIntrinsicBaseType.Bool].matrixSymbols; } }
		public static IntrinsicMatrixTypeSymbol[] AllIntMatrixTypes { get { return BaseTypes[(int)EIntrinsicBaseType.Int].matrixSymbols; } }
		public static IntrinsicMatrixTypeSymbol[] AllUintMatrixTypes { get { return BaseTypes[(int)EIntrinsicBaseType.Uint].matrixSymbols; } }
		public static IntrinsicMatrixTypeSymbol[] AllFloatMatrixTypes { get { return BaseTypes[(int)EIntrinsicBaseType.Float].matrixSymbols; } }

		//Initializing all types

		static IntrinsicTypes()
        {
            // Scalar types.
            Void = new IntrinsicScalarTypeSymbol("void", "Represents a void value.", ScalarType.Void);
            String = new IntrinsicScalarTypeSymbol("string", "Represents a string value.", ScalarType.String);

			BaseTypes = new List<IntrinsicBaseType>();

			AllScalarTypes = new List<IntrinsicScalarTypeSymbol>();
			AllVectorTypes = new List<IntrinsicVectorTypeSymbol>();
			AllMatrixTypes = new List<IntrinsicMatrixTypeSymbol>();

			List<IntrinsicTypeDesc> typeDescs = new List<IntrinsicTypeDesc>();

			typeDescs.Add(new IntrinsicTypeDesc{ name = "bool", desc = "boolean", type = ScalarType.Bool });

			typeDescs.Add(new IntrinsicTypeDesc{ name = "int", desc = "32-bit signed integer", type = ScalarType.Int });
			typeDescs.Add(new IntrinsicTypeDesc{ name = "uint", desc = "32-bit unsigned integer", type = ScalarType.Uint });
			typeDescs.Add(new IntrinsicTypeDesc{ name = "int64_t", desc = "64-bit signed integer", type = ScalarType.Int64_t });
			typeDescs.Add(new IntrinsicTypeDesc{ name = "uint64_t", desc = "64-bit unsigned integer", type = ScalarType.Uint64_t });

			typeDescs.Add(new IntrinsicTypeDesc{ name = "half", desc = "16-bit floating point", type = ScalarType.Half });
			typeDescs.Add(new IntrinsicTypeDesc{ name = "float", desc = "32-bit floating point", type = ScalarType.Float });
			typeDescs.Add(new IntrinsicTypeDesc{ name = "double", desc = "64-bit floating point", type = ScalarType.Double });

			typeDescs.Add(new IntrinsicTypeDesc{ name = "min16float", desc = "minimum 16-bit floating point", type = ScalarType.Min16Float });
			typeDescs.Add(new IntrinsicTypeDesc{ name = "min10float", desc = "minimum 10-bit floating point", type = ScalarType.Min10Float });

			typeDescs.Add(new IntrinsicTypeDesc{ name = "min16int", desc = "minimum 16-bit signed integer", type = ScalarType.Min16Int });
			typeDescs.Add(new IntrinsicTypeDesc{ name = "min12int", desc = "minimum 12-bit signed integer", type = ScalarType.Min12Int });
			typeDescs.Add(new IntrinsicTypeDesc{ name = "min16uint", desc = "minimum 16-bit unsigned integer", type = ScalarType.Min16Uint });

			MakeTypes(typeDescs);

			IntrinsicNumericTypeSymbol[] ints =
				BaseTypes[(int)EIntrinsicBaseType.Int].All
				.Union(BaseTypes[(int)EIntrinsicBaseType.Uint].All)
				.Union(BaseTypes[(int)EIntrinsicBaseType.Uint64_t].All)
				.Union(BaseTypes[(int)EIntrinsicBaseType.Int64_t].All)
				.Union(BaseTypes[(int)EIntrinsicBaseType.Min16Int].All)
				.Union(BaseTypes[(int)EIntrinsicBaseType.Min12Int].All)
				.Union(BaseTypes[(int)EIntrinsicBaseType.Min16Uint].All)
				.ToArray();

			IntrinsicNumericTypeSymbol[] floats =
				BaseTypes[(int)EIntrinsicBaseType.Float].All
				.Union(BaseTypes[(int)EIntrinsicBaseType.Double].All)
				.Union(BaseTypes[(int)EIntrinsicBaseType.Half].All)
				.Union(BaseTypes[(int)EIntrinsicBaseType.Min16Float].All)
				.Union(BaseTypes[(int)EIntrinsicBaseType.Min10Float].All)
				.ToArray();

			AllIntegralTypes = ints.Union(BaseTypes[(int)EIntrinsicBaseType.Bool].All).ToArray();

			AllNumericNonBoolTypes = ints.Union(floats).ToArray();

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

            BuiltInTriangleIntersectionAttributes = new IntrinsicObjectTypeSymbol("BuiltInTriangleIntersectionAttributes", "Any hit and closest hit shaders invoked using fixed-function triangle intersection must use this structure for hit attributes.", PredefinedObjectType.BuiltInTriangleIntersectionAttributes);
            RayDesc = new IntrinsicObjectTypeSymbol("RayDesc", "Passed to the TraceRay function to define the origin, direction, and extents of the ray.", PredefinedObjectType.RayDesc);
            RaytracingAccelerationStructure = new IntrinsicObjectTypeSymbol("RaytracingAccelerationStructure", "A resource type that can be declared in HLSL and passed into TraceRay to indicate the top-level acceleration resource built using BuildRaytracingAccelerationStructure.", PredefinedObjectType.RaytracingAccelerationStructure);

            BuiltInTriangleIntersectionAttributes.AddMember(new FieldSymbol("barycentrics", "The Barycentric coordinates of the hit location", BuiltInTriangleIntersectionAttributes, Float2));

            RayDesc.AddMember(new FieldSymbol("Origin", "The origin of the ray.", RayDesc, Float3));
            RayDesc.AddMember(new FieldSymbol("TMin", "The minimum extent of the ray.", RayDesc, Float));
            RayDesc.AddMember(new FieldSymbol("Direction", "The direction of the ray.", RayDesc, Float3));
            RayDesc.AddMember(new FieldSymbol("TMax", "The maximum extent of the ray.", RayDesc, Float));

            AllTypes = AllNumericTypes
                .Cast<TypeSymbol>()
                .Union(new[] { Sampler, Sampler1D, Sampler2D, Sampler3D, SamplerCube, SamplerState, SamplerComparisonState, LegacyTexture })
                .Union(new[] { BlendState, DepthStencilState, RasterizerState })
                .Union(new[] { GeometryShader, PixelShader, VertexShader })
                .Union(new[] { ByteAddressBuffer, RWByteAddressBuffer, RasterizerOrderedByteAddressBuffer })
                .Union(new[] { BuiltInTriangleIntersectionAttributes, RayDesc, RaytracingAccelerationStructure })
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
                case PredefinedObjectType.TextureCube:
                case PredefinedObjectType.TextureCubeArray:
                    break;
                default:
                    var indexType = GetTextureIndexType(textureType);
                    yield return new IndexerSymbol("[]", "Returns a resource variable.", parent, indexType, valueType);
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
                    return Uint;
                case PredefinedObjectType.Texture1DArray:
                case PredefinedObjectType.RWTexture1DArray:
                case PredefinedObjectType.RasterizerOrderedTexture1DArray:
                case PredefinedObjectType.Texture2D:
                case PredefinedObjectType.Texture2DMS:
                case PredefinedObjectType.RWTexture2D:
                case PredefinedObjectType.RasterizerOrderedTexture2D:
                    return Uint2;
                case PredefinedObjectType.Texture2DArray:
                case PredefinedObjectType.Texture2DMSArray:
                case PredefinedObjectType.RWTexture2DArray:
                case PredefinedObjectType.RasterizerOrderedTexture2DArray:
                case PredefinedObjectType.Texture3D:
                case PredefinedObjectType.RWTexture3D:
                case PredefinedObjectType.RasterizerOrderedTexture3D:
                    return Uint3;
                default:
                    throw new ArgumentOutOfRangeException(nameof(textureType), textureType, $"{textureType} is not a supported texture type");
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
                        new ParameterSymbol("location", "The texture coordinates.", m, locationType)
                    });
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
                        case PredefinedObjectType.RWTexture2D:
                        case PredefinedObjectType.RWTexture2DArray:
                        case PredefinedObjectType.RWTexture3D:
                        case PredefinedObjectType.RasterizerOrderedTexture2D:
                        case PredefinedObjectType.RasterizerOrderedTexture2DArray:
                        case PredefinedObjectType.RasterizerOrderedTexture3D:
                            result.Add(new ParameterSymbol("height", "The texture height, in texels.", m, parameterType, ParameterDirection.Out));
                            break;
                    }
                    switch (textureType)
                    {
                        case PredefinedObjectType.Texture3D:
                        case PredefinedObjectType.RWTexture3D:
                        case PredefinedObjectType.RasterizerOrderedTexture3D:
                            result.Add(new ParameterSymbol("depth", "The texture depth, in texels.", m, parameterType, ParameterDirection.Out));
                            break;
                    }
                    switch (textureType)
                    {
                        case PredefinedObjectType.Texture1DArray:
                        case PredefinedObjectType.Texture2DArray:
                        case PredefinedObjectType.TextureCubeArray:
                        case PredefinedObjectType.RWTexture1DArray:
                        case PredefinedObjectType.RWTexture2DArray:
                        case PredefinedObjectType.RasterizerOrderedTexture1DArray:
                        case PredefinedObjectType.RasterizerOrderedTexture2DArray:
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