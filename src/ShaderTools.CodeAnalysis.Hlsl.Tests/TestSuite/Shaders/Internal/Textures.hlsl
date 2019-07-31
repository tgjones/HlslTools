namespace Outer
{
    void foo();

	struct VertexShaderInput
	{
		float4 pos_ : POSITION;
		float2 tex_ : TEXCOORD;
	};

	namespace Nested
	{
		struct VertexShaderInput
		{
			float4 pos : POSITION;
			float2 tex : TEXCOORD;
		};

		class MyClass
		{
            int TestFunc();
        };
	}

	static const int bar = 4;
}

#define FOO 1

float myFoo = FOO;

void Outer::foo() { }

int Outer::Nested::MyClass::TestFunc()
{
	return Outer::bar;
}

struct Animal
{
	float3 size;
};

struct Cat : Animal
{
	float purriness;
};

void UseStructInheritance()
{
	Cat cat = (Cat) 0;
	cat.size = float3(1, 2, 1);
	cat.purriness = 0.8;
}

struct PixelShaderInput
{
	float4 pos : SV_POSITION;
	float2 tex : TEXCOORD;
};

cbuffer MyCBuffer
{
    float CBufferVariable;
};

// This is a line comment.

float Scalar1;
int Scalar2;

float2 Vector1;
int4 Vector2;
vector<bool, 3> Vector3;

matrix Matrix1;
float1x2 Matrix2;
int2x3 Matrix3;
bool4x2 Matrix4;
matrix<uint, 3, 2> Matrix5;

float4x4 WorldViewProjection;

Texture2D Picture;
Texture2D<bool> PictureTyped;
SamplerState PictureSampler;
SamplerComparisonState PictureSamplerComparison;

snorm float4 fourComponentSNormFloat;
unorm float4 fourComponentUNormFloat;

technique MyTechnique
{
    
}

/* This is a asdfsssdfsd  sdfsd  sss

sdfds
sdfsds multiline comment. */

/* This is a single-line block comment */

float test() {
	int    a;
	return 1.0;
}

Texture2DMS<float4, 32> PictureMS;

Outer::Nested::VertexShaderInput VsInput;

PixelShaderInput VS(Outer::Nested::VertexShaderInput input)
{
	PixelShaderInput output = (PixelShaderInput) 0;
	
	output.pos = mul(input.pos, WorldViewProjection);
	output.tex = input.tex;
	
	return output;
}

float4 PS(PixelShaderInput input);

float4 PS(PixelShaderInput input) : SV_Target
{
	Outer::foo();

	float lod = Picture.CalculateLevelOfDetail(PictureSampler, input.tex);
	float lodUnclamped = Picture.CalculateLevelOfDetailUnclamped(PictureSampler, input.tex);
	float4 gathered = Picture.Gather(PictureSampler, input.tex, int2(0, 1));
	
	int width, height, numLevels;
	Picture.GetDimensions(1, width, height, numLevels);

	float2 samplePos = PictureMS.GetSamplePosition(0);

	float4 loaded = PictureMS.Load(int2(25, 10), 1, int2(0, 1));

	float4 sampled = Picture.Sample(PictureSampler, input.tex);
	float4 sampleBias = Picture.SampleBias(PictureSampler, input.tex, 0.5);
	float4 sampleCmp = Picture.SampleCmp(PictureSamplerComparison, input.tex, 0.4);
	float4 sampleCmpLevelZero = Picture.SampleCmpLevelZero(PictureSamplerComparison, input.tex, 0.6);
	float4 sampleGrad = Picture.SampleGrad(PictureSampler, input.tex, 0.1, 0.2);
	float4 sampleLevel = Picture.SampleLevel(PictureSampler, input.tex, 1.5);

    [unroll]
    return float4(lod, lodUnclamped, width, height)
		+ gathered
		+ float4(samplePos, 0, 0)
		+ sampled
		+ sampleBias
		+ sampleCmp
		+ sampleCmpLevelZero
		+ sampleGrad
		+ sampleLevel;
}
