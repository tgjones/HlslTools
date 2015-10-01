//--------------------------------------------------------------------------------------
// Exercise03.fx
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
};

//-----------------------------------------------------------------------------------------
// State Structures
//-----------------------------------------------------------------------------------------
BlendState NoBlending
{
    AlphaToCoverageEnable = FALSE;
    BlendEnable[0] = FALSE;
};

BlendState SrcBlending
{
    AlphaToCoverageEnable = FALSE;
    BlendEnable[0] = TRUE;
    SrcBlend = SRC_ALPHA;
    DestBlend = INV_SRC_ALPHA;
    BlendOp = ADD;
};

//-----------------------------------------------------------------------------------------
// VertexShader: VSScene
//-----------------------------------------------------------------------------------------
GSSceneIn VSScene(VSSceneIn input)
{
	GSSceneIn output;
	
	// Transform the position into view space
	output.Pos = mul( float4(input.Pos,1.0), g_mWorldView );
	
	// Transform the Normal
	output.Norm = mul( input.Norm, (float3x3)g_mWorldView );
	
	// Pass the texcoord through
	output.Tex = input.Tex;
	
	return output;
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
// GeometryShader: GSScene
//					Extrude volumes off of the silhouettes we've already found.
//					Maximum number of vertices that can be output from this geometry shader
//					is 12.
//-----------------------------------------------------------------------------------------
[maxvertexcount(12)]

//-----------------------------------------------------------------------------------------
// o/__   <-- BreakdancinBob TODO:	Change the LineStream<PSSceneIn> below to 
// |  (\			TriangleStream<PSSceneIn> in the argument list.  This tells the GS to
//					output triangles instead of lines.
//-----------------------------------------------------------------------------------------
void GSScene( triangleadj GSSceneIn input[6], inout TriangleStream<PSSceneIn> OutputStream )
{	
	// Get the normal of the current triangle
	float3 TriNormal = GetNormal( input[0].Pos.xyz, input[2].Pos.xyz, input[4].Pos.xyz );
	
	// Ensure that the first triangle is facing the light
	if( dot( TriNormal, g_ViewSpaceLightDir ) > 0 )
	{
		PSSceneIn output = (PSSceneIn)0;
		
		// Iterate through the adjacent triangles
		for( uint i=0; i<6; i+=2 )
		{
			uint iNextTri = (i+2)%6;
			
			// Get the normal for the adjacent triangle
			float3 AdjTriNormal = GetNormal( input[i].Pos.xyz, input[i+1].Pos.xyz, input[ iNextTri ].Pos.xyz );
			
			// Do a test to ensure the adjacenty triangle is NOT facing the light
			if ( dot( AdjTriNormal, g_ViewSpaceLightDir ) <= 0 )
			{
				// extrude AWAY from the light -3 units
				float extrudeAmt = -3.0;
				
				// V0 - append the first endpoint of the edge shared by the two triangles
				float4 V0 = input[i].Pos;
				output.Pos = mul( V0, g_mProj );
				OutputStream.Append( output );
				
				//-----------------------------------------------------------------------------------------
				// o/__   <-- BreakdancinBob TODO:	Find the position of V1.  V0 is already given as one
				// |  (\			one of the endpoints of the silhouette edge.  V1 is V0 pushed away from
				//					the light along the direction of the light.  Refer to the diagram on
				//					the slides for more information.
				//
				//			  BreakdancinBob HINT:  V0, and V1 are float4.  g_ViewSpaceLightDir is float3.
				//					You can use a float4 constructor to get g_ViewSpaceLightDir to float4.
				//					Ex.  float4(g_ViewSpaceLightDir,0)
				//-----------------------------------------------------------------------------------------
				// V1 - V1 is just V0 pushed away from the light by extrudeAmt
				float4 V1 = V0 + extrudeAmt*float4(g_ViewSpaceLightDir,0);
				output.Pos = mul( V1, g_mProj );	// uncomment these lines when you fix V1
				OutputStream.Append( output );	// uncomment these lines when you fix V1
				
				// V2 - append the second endpoint of the edge shared by the two triangles
				float4 V2 = input[ iNextTri ].Pos;
				output.Pos = mul( V2, g_mProj );
				OutputStream.Append( output );
				
				//-----------------------------------------------------------------------------------------
				// o/__   <-- BreakdancinBob TODO:	Find the position of V3.  V2 is already given as one
				// |  (\			one of the endpoints of the silhouette edge.  V3 is V2 pushed away from
				//					the light along the direction of the light.  Refer to the diagram on
				//					the slides for more information.
				//
				//			  BreakdancinBob HINT:  V0, and V1 are float4.  g_ViewSpaceLightDir is float3.
				//					You can use a float4 constructor to get g_ViewSpaceLightDir to float4.
				//					Ex.  float4(g_ViewSpaceLightDir,0)
				//-----------------------------------------------------------------------------------------
				// V3 - V3 is just V2 pushed away from the light by extrudeAmt
				float4 V3 = V2 + extrudeAmt*float4(g_ViewSpaceLightDir,0);
				output.Pos = mul( V3, g_mProj );	// uncomment these lines when you fix V3
				OutputStream.Append( output );	// uncomment these lines when you fix V3
				
				// Restart the triangle strip
				OutputStream.RestartStrip();
			}
		}
	}
}

//-----------------------------------------------------------------------------------------
// PixelShader: PSScene
//-----------------------------------------------------------------------------------------
float4 PSScene(PSSceneIn input) : SV_Target
{	
	//return a constant color
	return float4(1,0,0,1);
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
        
        SetBlendState( NoBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
    }  
}






//-----------------------------------------------------------------------------------------
// VertexShader: VSScenePiece
//-----------------------------------------------------------------------------------------
PSSceneIn VSScenePiece(VSSceneIn input)
{
	PSSceneIn output;
	
	// Transform the position into clip space
	output.Pos = mul( float4(input.Pos,1.0), g_mWorldViewProj );
	
	// Transform the Normal
	output.Norm = mul( input.Norm, (float3x3)g_mWorldView );
	
	// Pass the texcoord through
	output.Tex = input.Tex;
	
	return output;
}

//-----------------------------------------------------------------------------------------
// PixelShader: PSScenePiece
//-----------------------------------------------------------------------------------------
float4 PSScenePiece(PSSceneIn input) : SV_Target
{	
	//calculate the lighting
	float lighting = saturate( dot( normalize( input.Norm ), g_ViewSpaceLightDir ) );
	float4 color = tex2D( g_samLinear, input.Tex )*lighting;
	color.a = 0.5;
	return color;
}

//-----------------------------------------------------------------------------------------
// Technique10: RenderPiece
//-----------------------------------------------------------------------------------------
technique10 RenderPiece
{
    pass p0
    {
		// Set VS, GS, and PS
        SetVertexShader( CompileShader( vs_4_0, VSScenePiece() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PSScenePiece() ) );
        
        SetBlendState( SrcBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
    }  
}

