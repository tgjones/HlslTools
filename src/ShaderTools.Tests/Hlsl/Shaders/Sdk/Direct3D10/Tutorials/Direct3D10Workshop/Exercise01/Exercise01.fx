//--------------------------------------------------------------------------------------
// Exercise01.fx
// Direct3D 10 Shader Model 4.0 Workshop
// Copyright (c) Microsoft Corporation. All rights reserved.
//--------------------------------------------------------------------------------------

//-----------------------------------------------------------------------------------------
// o/__   <-- Breakdancin' Bob will guide you through the exercise
// |  (\    
//-----------------------------------------------------------------------------------------

//-----------------------------------------------------------------------------------------
// Input and Output Structures
//-----------------------------------------------------------------------------------------
struct VSSceneIn
{
	float3 Pos	: POS;
	float3 Norm : NORMAL;
	float2 Tex	: TEXCOORD0;
};

//-----------------------------------------------------------------------------------------
// o/__   <-- Note:	New structure for the GeometryShader input
// |  (\    
//-----------------------------------------------------------------------------------------
struct GSSceneIn
{
	float4 Pos	: POS;
	float3 Norm : NORMAL;
	float2 Tex	: TEXCOORD0;
};

struct PSSceneIn
{
	float4 Pos  : SV_Position;		// SV_Position is a (S)ystem (V)ariable that denotes transformed position
	float3 Norm : TEXCOORD0;		// World transformed normal
	float2 Tex  : TEXCOORD1;
};

//-----------------------------------------------------------------------------------------
// Constant Buffers (where we store variables by frequency of update)
//-----------------------------------------------------------------------------------------
cbuffer cbEveryFrame
{
	matrix g_mWorldViewProj;
	matrix g_mWorldView;
	matrix g_mWorld;
	float3 g_ViewSpaceLightDir;
};

//-----------------------------------------------------------------------------------------
// Textures and Samplers
//-----------------------------------------------------------------------------------------
Texture2D g_txDiffuse;
sampler2D g_samLinear = sampler_state
{
	texture=g_txDiffuse;
};

//-----------------------------------------------------------------------------------------
// State Structures
//-----------------------------------------------------------------------------------------
BlendState NoBlending
{
    AlphaToCoverageEnable = FALSE;
    BlendEnable[0] = FALSE;
};

//-----------------------------------------------------------------------------------------
// VertexShader: VSScene
//-----------------------------------------------------------------------------------------
GSSceneIn VSScene(VSSceneIn input)
{
	GSSceneIn output;
	
	// Transform the Point
	output.Pos = mul( float4(input.Pos,1.0), g_mWorldViewProj );
	
	// Transform the Normal
	output.Norm = mul( input.Norm, (float3x3)g_mWorldView );
	
	// Pass the texcoord through
	output.Tex = input.Tex;
	
	return output;
}

//-----------------------------------------------------------------------------------------
// GeometryShader: GSScene
//					Output only the even vertices.  The odd vertices are edge adjacent
//					vertices that will be used in the next exercise.
//
//					[maxvertexcount(3)] specifies that the maximum number of vertices
//					that can be output from this geometry shader is 3.
//-----------------------------------------------------------------------------------------
[maxvertexcount(3)]
void GSScene( triangleadj GSSceneIn input[6], inout TriangleStream<PSSceneIn> OutputStream )
{	
	//-----------------------------------------------------------------------------------------
	// o/__   <-- BreakdancinBob Note:	We are taking in 6 input vertices for one triangle.  
	// |  (\			Only the Even Vertices are needed.  The odd ones represent edge-adjacent 
	//					vertices  which we'll use in later exercises.
	//-----------------------------------------------------------------------------------------

	PSSceneIn output = (PSSceneIn)0;

	for( uint i=0; i<6; i+=2 )
	{
		output.Pos = input[i].Pos;
		
		//-----------------------------------------------------------------------------------------
		// o/__   <-- BreakdancinBob TODO:	We're just outputting the position now.  Output the 
		// |  (\			normal and texture coordinates as well.  If it doesn't get appended to 
		//					the stream, it doesn't make it to the pixel shader.
		//-----------------------------------------------------------------------------------------
		//output.Norm = ?
		//output.Tex = ?
		
		OutputStream.Append( output );
	}
	
	OutputStream.RestartStrip();
}

//-----------------------------------------------------------------------------------------
// PixelShader: PSScene
//-----------------------------------------------------------------------------------------
float4 PSScene(PSSceneIn input) : SV_Target
{	
	//calculate the lighting
	float lighting = saturate( dot( normalize( input.Norm ), g_ViewSpaceLightDir ) );
	return tex2D( g_samLinear, input.Tex )*lighting;
}

//-----------------------------------------------------------------------------------------
// Technique10: RenderTextured
//-----------------------------------------------------------------------------------------
technique10 RenderTextured
{
    pass p0
    {
		// Set VS, GS, and PS
        SetVertexShader( CompileShader( vs_4_0, VSScene() ) );
        SetGeometryShader( CompileShader( gs_4_0, GSScene() ) );
        SetPixelShader( CompileShader( ps_4_0, PSScene() ) );
    }  
}

