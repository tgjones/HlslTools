//--------------------------------------------------------------------------------------
// File: DDSWithoutD3DX.fx
//
// The effect file for the DDSWithoutD3DX sample.  
// 
// Copyright (c) Microsoft Corporation. All rights reserved.
//--------------------------------------------------------------------------------------


//--------------------------------------------------------------------------------------
// Global variables
//--------------------------------------------------------------------------------------
float3   g_vLightDir = float3(0,0.707,-0.707);  // Light's direction in world space
float4x4 g_mWorld;                  // World matrix for object
float4x4 g_mWorldViewProjection;    // World * View * Projection matrix

//-----------------------------------------------------------------------------------------
// Textures and Samplers
//-----------------------------------------------------------------------------------------
texture2D g_txDiffuse;
sampler2D DiffuseSampler = sampler_state
{
    Texture = (g_txDiffuse);
#ifndef D3D10
    MinFilter = Linear;
    MagFilter = Linear;
#else // D3D10
    Filter = MIN_MAG_MIP_LINEAR;
#endif
    AddressU = WRAP;
    AddressV = WRAP;
};

//--------------------------------------------------------------------------------------
// shader input/output structure
//--------------------------------------------------------------------------------------
struct VS_INPUT
{
    float4 Position   : POSITION;   // vertex position 
    float3 Normal     : NORMAL;		// this normal comes in per-vertex
    float2 TextureUV  : TEXCOORD0;  // vertex texture coords 
};

struct VS_OUTPUT
{
    float4 Position   : POSITION;   // vertex position 
    float4 Diffuse    : COLOR0;     // vertex diffuse color (note that COLOR0 is clamped from 0..1)
    float2 TextureUV  : TEXCOORD0;  // vertex texture coords 
};

//--------------------------------------------------------------------------------------
// This shader computes standard transform and lighting
//--------------------------------------------------------------------------------------
VS_OUTPUT RenderSceneVS( VS_INPUT input )
{
    VS_OUTPUT Output;
    float3 vNormalWorldSpace;
    
    // Transform the position from object space to homogeneous projection space
    Output.Position = mul( input.Position, g_mWorldViewProjection );
    
    // Transform the normal from object space to world space    
    vNormalWorldSpace = normalize(mul(input.Normal, (float3x3)g_mWorld)); // normal (world space)

    // Calc diffuse color    
    Output.Diffuse.rgb = max(0.3,dot(vNormalWorldSpace, g_vLightDir)).rrr;
    Output.Diffuse.a = 1.0f; 
    
    // Just copy the texture coordinate through
    Output.TextureUV = input.TextureUV; 
    
    return Output;    
}

//--------------------------------------------------------------------------------------
// This shader outputs the pixel's color by modulating the texture's
// color with diffuse material color
//--------------------------------------------------------------------------------------
float4 RenderScenePS( VS_OUTPUT In ) : COLOR0
{ 
    // Lookup mesh texture and modulate it with diffuse
    return tex2D( DiffuseSampler, In.TextureUV) * In.Diffuse;
}


//--------------------------------------------------------------------------------------
// Renders scene for Direct3D 9
//--------------------------------------------------------------------------------------
technique RenderScene
{
    pass P0
    {   
        VertexShader = compile vs_2_0 RenderSceneVS();
        PixelShader  = compile ps_2_0 RenderScenePS();     
    }
}

//--------------------------------------------------------------------------------------
// RendersScene Single Index for Direct3D 10
//--------------------------------------------------------------------------------------
technique10 RenderScene10
{
    pass P0
    {       
        SetVertexShader( CompileShader( vs_4_0, RenderSceneVS() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, RenderScenePS() ) );
    }
}
