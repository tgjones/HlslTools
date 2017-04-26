//--------------------------------------------------------------------------------------
// File: 10BitScanout10.fx
//
// The effect file for the 10BitScanout10 sample.  
// 
// Copyright (c) Microsoft Corporation. All rights reserved.
//--------------------------------------------------------------------------------------


//--------------------------------------------------------------------------------------
// Global variables
//--------------------------------------------------------------------------------------
cbuffer cb0
{
    float4 g_vColor = float4(1,1,0.8,1);				// Object color
    float3 g_vLightDir = float3(-0.707,0.707,0);        // Light's direction in world space
    float2 g_vScreenRez = float2( 800.0f, 600.0f ); 
    float  g_colorRange = 0.25f;   
};

cbuffer cb1
{
    float4x4 g_mWorld;                  // World matrix for object
    float4x4 g_mWorldViewProjection;    // World * View * Projection matrix
};

BlendState NoBlending
{
    AlphaToCoverageEnable = FALSE;
    BlendEnable[0] = FALSE;
};

//--------------------------------------------------------------------------------------
// Vertex shader output structure
//--------------------------------------------------------------------------------------
struct VS_INPUT
{
    float3 Position   : POSITION;
    float3 Normal	  : NORMAL;
};

struct PS_INPUT
{
    float4 Position   : SV_Position;   // vertex position 
    float3 Normal     : NORMAL;     // vertex diffuse color (note that COLOR0 is clamped from 0..1)
};


//--------------------------------------------------------------------------------------
// This shader computes standard transform and lighting
//--------------------------------------------------------------------------------------
PS_INPUT RenderVS( VS_INPUT input )
{
    PS_INPUT output;
    
    output.Position = mul( float4(input.Position,1), g_mWorldViewProjection);   
    output.Normal = mul(input.Normal, (float3x3)g_mWorld);
    
    return output;    
}

//--------------------------------------------------------------------------------------
// This shader outputs the pixel's color by modulating the texture's
// color with diffuse material color
//--------------------------------------------------------------------------------------
float4 RenderPS( PS_INPUT input ) : SV_Target
{ 
    return g_vColor * saturate( dot( normalize(input.Normal), g_vLightDir ) );
}

//-----------------------------------------------------------------------------
// Name: QuadVS
// Type: Vertex Shader
// Desc: 
//-----------------------------------------------------------------------------
struct QuadVS_Input
{
    float4 Pos : POSITION;
    float2 Tex : TEXCOORD0;
};

struct QuadVS_Output
{
    float4 Pos : SV_POSITION;              // Transformed position
    float2 Tex : TEXCOORD0;
};

QuadVS_Output RenderQuadVS( QuadVS_Input Input )    
{
    QuadVS_Output Output;
    Output.Pos = Input.Pos;
    Output.Tex = Input.Tex;
    return Output;
}

SamplerState PointSampler
{
    Filter = MIN_MAG_MIP_POINT;
    AddressU = Clamp;
    AddressV = Clamp;
};

SamplerState LinearSampler
{
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = Clamp;
    AddressV = Clamp;
};

//#define UV_INTERPOLATION 1

#if defined( UV_INTERPOLATION )
float4 RenderQuadPS( QuadVS_Output Input ) : SV_TARGET
{
	float4 color = (float4)((Input.Tex.x - 0.5f) * g_colorRange);
	color.w = 1.0f;
	
    return color; 
}
#else // SCREEN SPACE INTERPOLATION
float4 RenderQuadPS( QuadVS_Output Input ) : SV_TARGET
{
	float4 color = (float4)(((Input.Pos.x - 0.5f) / g_vScreenRez.x) * g_colorRange);
	color.w = 1.0f;
	
    return color; 
}
#endif

//--------------------------------------------------------------------------------------
// Renders scene 
//--------------------------------------------------------------------------------------
technique10 Render
{
    pass P0
    {       
        SetVertexShader( CompileShader( vs_4_0, RenderVS() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, RenderPS() ) );
        
        SetBlendState( NoBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
    }
}

technique10 RenderQuad
{
	pass P0
	{
        SetVertexShader( CompileShader( vs_4_0, RenderQuadVS( ) ) ); 
        SetGeometryShader( NULL );        
        SetPixelShader( CompileShader( ps_4_0, RenderQuadPS( ) ) ); 
        SetBlendState( NoBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        
    }
}