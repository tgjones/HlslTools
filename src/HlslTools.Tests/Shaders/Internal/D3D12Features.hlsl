#define MyRS1 "RootFlags( ALLOW_INPUT_ASSEMBLER_INPUT_LAYOUT | " \
                         "DENY_VERTEX_SHADER_ROOT_ACCESS), " \
              "CBV(b0, space = 1), " \
              "SRV(t0), " \
              "UAV(u0), " \
              "DescriptorTable( CBV(b1), " \
                               "SRV(t1, numDescriptors = 8), " \
                               "UAV(u1, numDescriptors = unbounded)), " \
              "DescriptorTable(Sampler(s0, space=1, numDescriptors = 4)), " \
              "RootConstants(num32BitConstants=3, b10), " \
              "StaticSampler(s1)," \
              "StaticSampler(s2, " \
                             "addressU = TEXTURE_ADDRESS_CLAMP, " \
                             "filter = FILTER_MIN_MAG_MIP_LINEAR )"

Texture2D<float4> tex0          : register(t5, space0);
Texture2D<float4> tex1[][5][3]  : register(t10, space0);
Texture2D<float4> tex2[8]       : register(t0, space1);
SamplerState samp0              : register(s5, space0);

[RootSignature(MyRS1)]
float4 main(float4 coord : COORD) : SV_TARGET
{
	float4 r = coord;
	r += tex0.Sample(samp0, r.xy);
	r += tex2[r.x].Sample(samp0, r.xy);
	r += tex1[r.x][r.y][r.z].Sample(samp0, r.xy);
	return r;
}

struct MyStruct
{
	float3 v;
};

RasterizerOrderedBuffer<float4> b1;
RasterizerOrderedByteAddressBuffer b2;
RasterizerOrderedStructuredBuffer<MyStruct> b3;
RasterizerOrderedTexture1D b4;
RasterizerOrderedTexture1DArray b5;
RasterizerOrderedTexture2D b6;
RasterizerOrderedTexture2DArray b7;
RasterizerOrderedTexture3D b8;