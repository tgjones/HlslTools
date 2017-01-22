//--------------------------------------------------------------------------------------
// File: SimpleSample.fx
//
// The effect file for the SimpleSample sample.  
// 
// Copyright (c) Microsoft Corporation. All rights reserved.
//--------------------------------------------------------------------------------------


//--------------------------------------------------------------------------------------
// Global variables
//--------------------------------------------------------------------------------------
cbuffer cb0
{
    float3 g_vLightDir = float3(-0.707,0.707,0);                 // Light's direction in world space
    float4 g_vColor = float4(1,1,0.8,1);				// Object color
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
