//--------------------------------------------------------------------------------------
// Exercise02.fx
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
//					Output the silhouette edge as a LineStream.  The maximum number of
//					vertices that this shader can output is 6.
//-----------------------------------------------------------------------------------------
[maxvertexcount(6)]
void GSScene( triangleadj GSSceneIn input[6], inout LineStream<PSSceneIn> OutputStream )
{	
	//-----------------------------------------------------------------------------------------
	// o/__   <-- BreakdancinBob NOTE:	This shader determines which edges are silhouette edges 
	// |  (\			by computing the normal for the face of the triangle, and then computing 
	//					the normals for the adjacent triangles.  
	//
	//					The current triangle is specified by input[0], input[2], and input[4]
	//					The three adjacent triangles are defined as follows
	//							Adjacent1:  input[0], input[1], input[2]
	//							Adjacent2:  input[2], input[3], input[4]
	//							Adjacent3:  input[4], input[5], input[0]
	//
	//					If the current triangle is facing the camera, but an adjacent triangle
	//					is not, then the edje they share is a silhouette edge.  These edges
	//					are added to the LineStream OutputStream and sent to the pixel shader.
	//-----------------------------------------------------------------------------------------
	
	// Get the normal of the current triangle
	float3 TriNormal = GetNormal( input[0].Pos.xyz, input[2].Pos.xyz, input[4].Pos.xyz );
	

	//-----------------------------------------------------------------------------------------
	// o/__   <-- BreakdancinBob TODO:  Change the test below to determine whether the TriNormal
	// |  (\			 is facing the light.  The light direction is stored in g_ViewSpaceLightDir.
	//					 You can use the dot intrinsic to determine if two vectors are facing
	//					 the same direction.  The dot product of two vectors is the length of
	//					 the projection of one vector onto the other.  If this length is positive,
	//					 the two vectors are less than 90 degrees apart.  If the length is negative,
	//					 the two vectors are greater than 90 degrees apart.
	//					
	//					 The dot syntax is:
	//						float result = dot( vector, vector );
	//-----------------------------------------------------------------------------------------
	if( dot( TriNormal, g_ViewSpaceLightDir ) > 0 ) 	// Test to make sure the normal is facing the camera (or the light)
	{
		PSSceneIn output;
		
		// Iterate through the adjacent vertices
		for( uint i=0; i<6; i+=2 )
		{
			uint iNextTri = (i+2)%6;
			// Get the normal for the adjacent triangle
			float3 AdjTriNormal = GetNormal( input[i].Pos.xyz, input[i+1].Pos.xyz, input[ iNextTri ].Pos.xyz );
			
			//-----------------------------------------------------------------------------------------
			// o/__   <-- BreakdancinBob TODO:  Change the test below to determine whether the AdjTriNormal
			// |  (\			 is NOT facing the light.  The light direction is stored in g_ViewSpaceLightDir.
			//					 You can use the dot intrinsic to determine if two vectors are facing
			//					 the same direction.  Refer to the dot syntax above.
			//-----------------------------------------------------------------------------------------
			if ( dot( AdjTriNormal, g_ViewSpaceLightDir ) <= 0 ) // Test to make sure the normal is NOT facing the camera (or the light)
			{
			
				// append the first endpoint of the edge shared by the two triangles
				output.Pos = mul( input[i].Pos, g_mProj );
				output.Norm = input[i].Norm;
				output.Tex = input[i].Tex;
				OutputStream.Append( output );
				
				// append the second endpoint of the edge shared by the two triangles
				output.Pos = mul( input[ iNextTri ].Pos, g_mProj );
				output.Norm = input[ iNextTri ].Norm;
				output.Tex = input[ iNextTri ].Tex;
				OutputStream.Append( output );
				
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

