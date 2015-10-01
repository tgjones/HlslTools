//--------------------------------------------------------------------------------------
// Exercise06.fx
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
	
	//-----------------------------------------------------------------------------------------
	// o/__   <-- BreakdancinBob NOTE: This is a vertex ID created automatically by the input assembler.  It 
	// |  (\			is a system variable denoted by SV.  System variables have special
	//					meaning in Shader Model 4.0
	//-----------------------------------------------------------------------------------------
	uint   vertID : SV_VertexID;
};

struct GSSceneIn
{
	float3 Pos	: POS;
	float3 Norm : NORMAL;
	float2 Tex	: TEXCOORD0;
	float  GrowAmt : GROWAMT;
	uint  vertID : ID;
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

cbuffer cbUserDefined
{
	float LengthModifier;
	float SpreadModifier;
	float ShrinkModifier;
	float ExtinctionRate;
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
// o/__   <-- New buffer to hold random data.
// |  (\    
//-----------------------------------------------------------------------------------------
Buffer<float4> g_RandomBuffer;

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

//-----------------------------------------------------------------------------------------
// VertexShader: VSScene
//-----------------------------------------------------------------------------------------
PSSceneIn VSScene(VSSceneIn input)
{
	PSSceneIn output;
	
	// Transform the Point
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
// VertexShader: VSPassThrough
//-----------------------------------------------------------------------------------------
GSSceneIn VSPassThrough(VSSceneIn input)
{
	GSSceneIn output;
	output.Pos = input.Pos;
	output.Norm = input.Norm;
	output.Tex	= input.Tex;
	output.GrowAmt = input.GrowAmt;
	output.vertID = input.vertID;
	return output;
}


//-----------------------------------------------------------------------------------------
// o/__   <-- BreakdancinBob TODO: Implement this function in such a way that it takes in 
// |  (\			the vertexID and returns a unique random direction from the random 
//					buffer which is stored in g_RandomBuffer.  Use the Load method.
//
//					The load syntax is:
//						data = BufferName.Load( offset );
//-----------------------------------------------------------------------------------------
//-----------------------------------------------------------------------------------------
// RandomDir: Sample a random direction from our random texture
//-----------------------------------------------------------------------------------------
float3 RandomDir(uint ID)
{
	//-----------------------------------------------------------------------------------------
	// o/__   <-- BreakdancinBob HINT: ID will come in the range of [0..MaxVertices].  The 
	// |  (\			buffer size will also be in the [0..MaxVertices] range.  To get 
	//					different random effects try modifying the ID and then loading data
	//					from the buffer.
	//-----------------------------------------------------------------------------------------
	return float3(1,1,1);
}

//-----------------------------------------------------------------------------------------
// GetNormal
//-----------------------------------------------------------------------------------------
float3 GetNormal( float3 A, float3 B, float3 C )
{
	float3 AB = B - A;
	float3 AC = C - A;
	return normalize( cross(AB,AC) );
}

//-----------------------------------------------------------------------------------------
// Helper for the GS
//-----------------------------------------------------------------------------------------
void ExtrudeBranch( GSSceneIn triPts[3], float growAmt, inout TriangleStream<GSSceneIn> OutputStream )
{
	// Find the triangle normal
	float3 TriNormal = GetNormal( triPts[0].Pos, triPts[1].Pos, triPts[2].Pos );
	float3 TriCenter = (triPts[0].Pos + triPts[1].Pos + triPts[2].Pos)/3.0;
	
	GSSceneIn outputVert = (GSSceneIn)0;
	GSSceneIn topVerts[3];
	// Create the Edges of the extruded prism
	[unroll] for(int i=3; i>=0; i--)
	{
		uint ivert = i%3;
		
		// find the normal pointing away from the column
		float3 LocalNormal = triPts[ivert].Pos - TriCenter;
		float normLen = length( LocalNormal );
		LocalNormal /= normLen;
		
		outputVert.Pos = triPts[ivert].Pos;
		outputVert.Norm = LocalNormal;
		outputVert.Tex = float2(i*0.3333,0);
		outputVert.GrowAmt = 0.0;
		OutputStream.Append( outputVert );
		
		outputVert.Pos += TriNormal*growAmt;
		outputVert.Pos -= LocalNormal*normLen*ShrinkModifier;
		outputVert.Tex.y = 1.0;
		topVerts[ivert] = outputVert;
		OutputStream.Append( outputVert );
	}
	
	OutputStream.RestartStrip();
	
	// Add the end cap
	float3 midPt;
	midPt = (topVerts[0].Pos + topVerts[1].Pos + topVerts[2].Pos )/3.0;
	midPt += TriNormal*growAmt*SpreadModifier;
	
	//-----------------------------------------------------------------------------------------
	// o/__   <-- BreakdancinBob NOTE: Here we are using the RandomDir function to change the 
	// |  (\			geometry of the end of a branch, and therefore, the geometries of 
	//					subsequent branches.
	//-----------------------------------------------------------------------------------------
	midPt += normalize( RandomDir( triPts[0].vertID ) )*0.02;
	
	//-----------------------------------------------------------------------------------------
	// o/__   <-- BreakdancinBob NOTE: Here we are using the RandomDir function determine if 
	// |  (\			we need to grow new branches off of our current branch.  Note, we
	//					add 100 to the vertID so that we don't get the same value as the 
	//					previous call.
	//-----------------------------------------------------------------------------------------
	float3 random3 = RandomDir( triPts[0].vertID + 100 );
	if( abs(random3.x) < ExtinctionRate )
		growAmt = 0;
	
	[unroll] for(int i=0; i<3; i++)
	{
		TriNormal = GetNormal( topVerts[i].Pos, topVerts[(i+1)%3].Pos, midPt );
		topVerts[i].Norm = TriNormal;
		topVerts[i].GrowAmt = growAmt * LengthModifier;
		topVerts[(i+1)%3].Norm = TriNormal;
		topVerts[(i+1)%3].GrowAmt = growAmt * LengthModifier;
		outputVert.Pos = midPt;
		outputVert.Norm = TriNormal;
		outputVert.Tex = float2(0,topVerts[i].Tex.y);
		outputVert.GrowAmt = growAmt * LengthModifier;
		
		OutputStream.Append( topVerts[i] );
		OutputStream.Append( topVerts[(i+1)%3] );
		OutputStream.Append( outputVert );
		OutputStream.RestartStrip();
	}
	
}

//-----------------------------------------------------------------------------------------
// GeometryShader: GSGrowBranches
//					Grow a branch off of any triangles that have a non-zero GrowAmounts.
//					Maximimum number of vertices that can be output from this geometry
//					shader is 17.
//-----------------------------------------------------------------------------------------
[maxvertexcount(17)]
void GSGrowBranches( triangle GSSceneIn input[3], inout TriangleStream<GSSceneIn> OutputStream )
{	
	if( input[0].GrowAmt == 0.0 || input[1].GrowAmt == 0.0 || input[2].GrowAmt == 0.0 )
	{
		OutputStream.Append( input[0] );
		OutputStream.Append( input[1] );
		OutputStream.Append( input[2] );
		OutputStream.RestartStrip();
	}
	else
	{
		ExtrudeBranch( input, input[0].GrowAmt, OutputStream );
	}
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
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PSScene() ) );
        
        SetDepthStencilState( EnableDepth, 0 );
    }  
}

//-----------------------------------------------------------------------------------------
// Technique: GrowBranches 
//-----------------------------------------------------------------------------------------
GeometryShader gsStreamOut = ConstructGSWithSO( CompileShader( gs_4_0, GSGrowBranches() ), "POS.xyz; NORMAL.xyz; TEXCOORD0.xy; GROWAMT.x" );
technique10 GrowBranches
{
    pass p0
    {
		// Set VS, GS, and PS
        SetVertexShader( CompileShader( vs_4_0, VSPassThrough() ) );
        SetGeometryShader( gsStreamOut );
        SetPixelShader( NULL );
        
        SetDepthStencilState( DisableDepth, 0 );
    }  
}

