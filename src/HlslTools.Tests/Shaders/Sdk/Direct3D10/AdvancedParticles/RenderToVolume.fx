//--------------------------------------------------------------------------------------
// File: RenderToVolume.fx
//
// Copyright (c) Microsoft Corporation. All rights reserved.
//--------------------------------------------------------------------------------------
#include "common.fxh"

struct VSSceneIn
{
    float3 Pos              : POSITION;
    float3 Norm             : NORMAL;
    float2 Tex				: TEXCOORD0;
    uint   InstanceID		: SV_INSTANCEID;
};

struct GSSceneIn
{
    float4 Pos				: SV_POSITION;
    float4 wPos				: WORLDPOS;
    uint   InstanceID		: INSTANCEID;
};

struct GSSceneOut
{
	float4 PlaneEq          : NORMAL;
	float2 PlaneDist		: CLIPDISTANCE;
	float4 Pos				: SV_POSITION;
    uint   RTIndex			: SV_RENDERTARGETARRAYINDEX;
};

struct PSSceneIn
{
	float4 PlaneEq          : NORMAL;
	float2 PlaneDist		: CLIPDISTANCE;
};

struct GSVelocityIn
{
	float4 Pos				: POSITION;
	float2 PlaneDist		: PLANEDIST;
	float3 Velocity			: VELOCITY;
	uint   InstanceID		: INSTANCEID;
};

struct GSVelocityOut
{
	float3 Velocity			: VELOCITY;
	float2 PlaneDist		: CLIPDISTANCE;
	float4 Pos				: SV_POSITION;
	uint   RTIndex			: SV_RENDERTARGETARRAYINDEX;
};

struct PSVelocityIn
{
	float3 Velocity			: VELOCITY;
	float2 PlaneDist		: CLIPDISTANCE;
};

cbuffer cb0
{
    float4x4 g_mWorldViewProj;
    float4x4 g_mViewProj;
    float4x4 g_mWorld;
    float4x4 g_mWorldPrev;
    float    g_fElapsedTime;
    float    g_fPlaneStart;
    float	 g_fPlaneStep;
};

cbuffer cbPerSlice
{
	float4  g_vFarClipPlane;
	float4	g_vNearClipPlane;
};

BlendState NoBlending
{
    AlphaToCoverageEnable = FALSE;
    BlendEnable[0] = FALSE;
};

DepthStencilState EnableDepth
{
    DepthEnable = TRUE;
    DepthWriteMask = ALL;
};

DepthStencilState DisableDepth
{
    DepthEnable = FALSE;
    DepthWriteMask = 0;
};

RasterizerState CullNone
{
	CullMode = NONE;
};

RasterizerState CullBack
{
	CullMode = BACK;
};

//
// Vertex shader for drawing the scene
//
GSSceneIn VSScene(VSSceneIn input)
{
    GSSceneIn output = (GSSceneIn)0;
    
    output.Pos = mul( float4(input.Pos,1), g_mWorldViewProj );
    output.wPos = mul( float4(input.Pos,1), g_mWorld );
	output.InstanceID = input.InstanceID;
	
    return output;
}

//
// Vertex shader for drawing the animated scene
//
GSSceneIn VSAnim( VSAnimIn input )
{
    GSSceneIn output = (GSSceneIn)0;
    
    SkinnedInfo vSkinned = SkinVert( input );
    output.Pos = mul( vSkinned.Pos, g_mViewProj );
    output.wPos = vSkinned.Pos;
    output.InstanceID = input.InstanceID;
    
    return output;
}

[maxvertexcount(3)]
void GSScene( triangle GSSceneIn input[3], inout TriangleStream<GSSceneOut> TriStream )
{
	GSSceneOut output;
	
	// calculate the face normal
	float3 faceEdgeA = input[1].wPos - input[0].wPos;
	float3 faceEdgeB = input[2].wPos - input[0].wPos;
	float3 faceNormal = normalize( cross( faceEdgeA, faceEdgeB ) );
	
	// find the plane equation
	float4 planeEq;
	planeEq.xyz = faceNormal;
	planeEq.w = -dot( input[0].wPos, faceNormal );
	
	// create the clip planes
	float4 vFarClip = float4( 0,1,0, -(g_fPlaneStart + g_fPlaneStep*input[0].InstanceID) );
	float4 vNearClip = float4( 0,-1,0, (g_fPlaneStart + g_fPlaneStep*(input[0].InstanceID+1)) );
	
	// send us to the right render target
	output.RTIndex = input[0].InstanceID;
	
	// set the plane equation
	output.PlaneEq = planeEq;
	
	// output the triangle
	[unroll] for( int i=0; i<3; i++ )
	{
		// pass the position through
		output.Pos = input[i].Pos;
	
		// calculate clip distances
		output.PlaneDist.x = dot( input[i].wPos, vFarClip );
		output.PlaneDist.y = dot( input[i].wPos, vNearClip );
		
		TriStream.Append( output );
	}
}

//
// Pixel shader for drawing the scene
//
float4 PSScene( PSSceneIn input ) : SV_Target
{   
	if( input.PlaneDist.x < 0 ) { discard; }
	if( input.PlaneDist.y < 0 ) { discard; }
	return input.PlaneEq;
}

//
// Vertex shader for drawing velocity
//
GSVelocityIn VSVelocity( VSSceneIn input )
{
    GSVelocityIn output = (GSVelocityIn)0;
    
    output.Pos = mul( float4(input.Pos,1), g_mWorldViewProj );
    
    // calculate world-space velocity
    float4 wPos = mul( float4(input.Pos,1), g_mWorld );
    float4 wPosPrev = mul( float4(input.Pos,1), g_mWorldPrev );
	output.Velocity = ( wPos - wPosPrev ) / g_fElapsedTime;
	
	// create the clip planes
	float4 vFarClip = float4( 0,1,0, -(g_fPlaneStart + g_fPlaneStep*input.InstanceID) );
	float4 vNearClip = float4( 0,-1,0, (g_fPlaneStart + g_fPlaneStep*(input.InstanceID+1)) );
	
	// pass instance id through
	output.InstanceID = input.InstanceID;
	
    // calculate clip distances
	output.PlaneDist.x = dot( wPos, vFarClip );
	output.PlaneDist.y = dot( wPos, vNearClip );
		
    return output;
}

//
// Vertex shader for drawing animated velocity
//
GSVelocityIn VSAnimVelocity( VSAnimIn input )
{
    GSVelocityIn output = (GSVelocityIn)0;
    
	SkinnedInfo vSkinned = SkinVert( input );
	SkinnedInfo vSkinnedPrev = SkinVertPrev( input );
	
    output.Pos = mul( vSkinned.Pos, g_mViewProj );
    
    // calculate world-space velocity
    float4 wPos = vSkinned.Pos;
    float4 wPosPrev = vSkinnedPrev.Pos;
	output.Velocity = ( wPos - wPosPrev ) / g_fElapsedTime;
	
	// create the clip planes
	float4 vFarClip = float4( 0,1,0, -(g_fPlaneStart + g_fPlaneStep*input.InstanceID) );
	float4 vNearClip = float4( 0,-1,0, (g_fPlaneStart + g_fPlaneStep*(input.InstanceID+1)) );
	
    // pass instance id through
	output.InstanceID = input.InstanceID;
	
    // calculate clip distances
	output.PlaneDist.x = dot( wPos, vFarClip );
	output.PlaneDist.y = dot( wPos, vNearClip );
		
    return output;
}

//
// Geometry shader for drawing velocity
//
[maxvertexcount(3)]
void GSVelocity( triangle GSVelocityIn input[3], inout TriangleStream<GSVelocityOut> TriStream )
{
	GSVelocityOut output;
	
	// output the triangle
	for( int i=0; i<3; i++ )
	{
		// pass the position through
		output.Pos = input[i].Pos;
		output.PlaneDist = input[i].PlaneDist;
		output.Velocity = input[i].Velocity;
		output.RTIndex = input[i].InstanceID;
		
		TriStream.Append( output );
	}
}

//
// Pixel shader for drawing the velocity
//
float4 PSVelocity(PSVelocityIn input) : SV_Target
{   
	if( input.PlaneDist.x < 0 ) { discard; }
	if( input.PlaneDist.y < 0 ) { discard; }
	return float4(input.Velocity,1);
}

//
// RenderScene
//
technique10 RenderScene
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, VSScene() ) );
        SetGeometryShader( CompileShader( gs_4_0, GSScene() ) );
        SetPixelShader( CompileShader( ps_4_0, PSScene() ) );
        
        SetBlendState( NoBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( DisableDepth, 0 );
        SetRasterizerState( CullBack );
    }  
}


//
// RenderScene
//
technique10 RenderAnimScene
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, VSAnim() ) );
        SetGeometryShader( CompileShader( gs_4_0, GSScene() ) );
        SetPixelShader( CompileShader( ps_4_0, PSScene() ) );
        
        SetBlendState( NoBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( DisableDepth, 0 );
        SetRasterizerState( CullNone );
    }  
}


//
// RenderVelocity
//
technique10 RenderVelocity
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, VSVelocity() ) );
        SetGeometryShader( CompileShader( gs_4_0, GSVelocity() ) );
        SetPixelShader( CompileShader( ps_4_0, PSVelocity() ) );
        
        SetBlendState( NoBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( DisableDepth, 0 );
        SetRasterizerState( CullNone );
    }  
}


//
// RenderAnimVelocity
//
technique10 RenderAnimVelocity
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, VSAnimVelocity() ) );
        SetGeometryShader( CompileShader( gs_4_0, GSVelocity() ) );
        SetPixelShader( CompileShader( ps_4_0, PSVelocity() ) );
        
        SetBlendState( NoBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( DisableDepth, 0 );
        SetRasterizerState( CullNone );
    }  
}