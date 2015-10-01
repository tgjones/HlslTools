//--------------------------------------------------------------------------------------
// Exercise00.fx
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
	Filter = MIN_MAG_MIP_LINEAR;
	AddressU = WRAP;
	AddressV = WRAP;
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
PSSceneIn VSScene(VSSceneIn input)
{
	PSSceneIn output;
	
	//-----------------------------------------------------------------------------------------
	// o/__   <-- BreakdancinBob TODO: You should get a compile error on the line below.  This
	// |  (\			is because VSSceneIn does not contain a member called pos.  It does
	//					contain a member called Pos (uppercase P).  Fixed this error and
	//					recompile.
	//-----------------------------------------------------------------------------------------
	output.Pos = mul( float4(input.Pos,1.0), g_mWorldViewProj );
	
	// Transform the Normal
	output.Norm = mul( input.Norm, (float3x3)g_mWorldView );
	
	// Pass the texcoord through
	output.Tex = input.Tex;

	return output;
}

//-----------------------------------------------------------------------------------------
// PixelShader: PSSceneMain
//-----------------------------------------------------------------------------------------
float4 PSScene(PSSceneIn input) : SV_Target
{	
	//calculate the lighting
	float lighting = saturate( dot( normalize( input.Norm ), g_ViewSpaceLightDir ) );
	return tex2D( g_samLinear, input.Tex )*lighting;
}

//-----------------------------------------------------------------------------------------
// o/__   <-- BreakdancinBob NOTE: For 10 FX files we use technique10 to denote Direct3D 10 
// |  (\			level techniques. 
//-----------------------------------------------------------------------------------------
technique10 RenderTextured
{
    pass p0
    {
		// Set VS, GS, and PS
        SetVertexShader( CompileShader( vs_4_0, VSScene() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PSScene() ) );
        
        // Set our render states
        // This is actually the default state and will not be set in future exercises
        SetBlendState( NoBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
    }  
}

