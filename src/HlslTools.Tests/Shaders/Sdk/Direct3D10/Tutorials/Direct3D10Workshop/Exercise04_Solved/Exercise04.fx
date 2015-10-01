//--------------------------------------------------------------------------------------
// Exercise04.fx
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
	float  GrowAmt : GROWAMT;
};

struct GSSceneIn
{
	float4 Pos	: POS;
	float3 Norm : NORMAL;
	float2 Tex	: TEXCOORD0;
	float  GrowAmt : GROWAMT;
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
	matrix g_mProj;
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

DepthStencilState DisableDepth
{
    DepthEnable = FALSE;
    DepthWriteMask = ZERO;
};

DepthStencilState EnableDepth
{
    DepthEnable = TRUE;
    DepthWriteMask = ALL;
};

RasterizerState NoCulling
{
	CullMode = NONE;
};

//-----------------------------------------------------------------------------------------
// VertexShader: VSScene
//-----------------------------------------------------------------------------------------
GSSceneIn VSScene(VSSceneIn input)
{
	GSSceneIn output;
	
	// transform the point into view space
	output.Pos = mul( float4(input.Pos,1.0), g_mWorldView );
	
	// Transform the Normal
	output.Norm = mul( input.Norm, (float3x3)g_mWorldView );
	
	// Pass the texcoord through
	output.Tex = input.Tex;
	
	// Pass the Grow Amount through
	output.GrowAmt = input.GrowAmt;
	
	return output;
}

//-----------------------------------------------------------------------------------------
// GetNormal - this helper function gets the normal of the triangle created by the 3
//				passed in vertices.
//-----------------------------------------------------------------------------------------
float3 GetNormal( GSSceneIn triPts[3] )
{
	float3 AB = triPts[1].Pos - triPts[0].Pos;
	float3 AC = triPts[2].Pos - triPts[0].Pos;
	return normalize( cross(AB,AC) );
}

//-----------------------------------------------------------------------------------------
// Helper for the GS
//
// triPts[3] contains the input triangle vertices.  growAmt is the amount by which we
// extrude the triangle.
//-----------------------------------------------------------------------------------------
void ExtrudeBranch( GSSceneIn triPts[3], float growAmt, inout TriangleStream<PSSceneIn> OutputStream )
{
	//-----------------------------------------------------------------------------------------
	// o/__   <-- BreakdancinBob TODO: Find the normal for the triangle.  You can do this 
	// |  (\			yourself or use the GetNormal helper function above.
	//-----------------------------------------------------------------------------------------
	float3 TriNormal = GetNormal( triPts );
	
	PSSceneIn output = (PSSceneIn)0;
	
	// Loop over all 3 triangle points
	[unroll] for( int i=0; i<3; i++ )
	{
		//-----------------------------------------------------------------------------------------
		// o/__   <-- BreakdancinBob TODO: Extrude the triangle points along the triangle normal.
		// |  (\			
		//					The original input position is in triPts[i].Pos.  ExtrudePos should 
		//					be the input position moved growAmt (passed in) in the direction of
		//					the tirangle normal (TriNormal).
		//-----------------------------------------------------------------------------------------
		float3 extrudePos = triPts[i].Pos + TriNormal*growAmt;
		
		//-----------------------------------------------------------------------------------------
		// o/__   <-- BreakdancinBob TODO: After you extrude the point along the normal, don't 
		// |  (\			forget to multiply by the projection matrix (g_mProj) or it won't show 
		//					up properly on the screen.  Uncomment the line below to do this.
		//-----------------------------------------------------------------------------------------
		output.Pos = mul( float4( extrudePos, 1 ), g_mProj );
		output.Norm = triPts[i].Norm;
		output.Tex = triPts[i].Tex;
		
		//Append this triangle point to the output stream
		OutputStream.Append( output );
	}
	OutputStream.RestartStrip();
}

//-----------------------------------------------------------------------------------------
// GeometryShader: GSGrowBranches
//					Grow a branch off of any triangles that have a non-zero GrowAmounts.
//					Maximimum number of vertices that can be output from this geometry
//					shader is 6.
//-----------------------------------------------------------------------------------------
[maxvertexcount(6)]
//-----------------------------------------------------------------------------------------
// o/__   <-- BreakdancinBob Note:	We will not use adjacency for the rest of the exercises.  
// |  (\			Therefore the triangles passed into this GS are triangle (not triangleadj).  
//					Only three vertices are input for normal triangles.
//-----------------------------------------------------------------------------------------
void GSScene( triangle GSSceneIn input[3], inout TriangleStream<PSSceneIn> OutputStream )
{
	PSSceneIn output;
	for( uint i=0; i<3; i++ )
	{
		output.Pos = mul( input[i].Pos, g_mProj );
		output.Norm = input[i].Norm;
		output.Tex = input[i].Tex;
		OutputStream.Append( output );
	}
	OutputStream.RestartStrip();
				
	if( input[0].GrowAmt > 0 && input[1].GrowAmt > 0 && input[2].GrowAmt > 0 )
	{
		// Extrude a branch by input[0].GrowAmt
		ExtrudeBranch( input, input[0].GrowAmt, OutputStream );
	}
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
// Technique: RenderTextured  
//-----------------------------------------------------------------------------------------
technique10 RenderTextured
{
    pass p0
    {
		// Set VS, GS, and PS
        SetVertexShader( CompileShader( vs_4_0, VSScene() ) );
        SetGeometryShader( CompileShader( gs_4_0, GSScene() ) );
        SetPixelShader( CompileShader( ps_4_0, PSScene() ) );
        
        SetDepthStencilState( EnableDepth, 0 );
        
        //disable culling so we can see the thin extruded triangles
        SetRasterizerState( NoCulling );
    }  
}

