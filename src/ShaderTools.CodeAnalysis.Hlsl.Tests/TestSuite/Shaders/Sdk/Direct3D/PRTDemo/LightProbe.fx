//-----------------------------------------------------------------------------
// File: lightprobe.fx
//
// Desc: Rendering a cubic light probe LightProbe
// 
// Copyright (c) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------


//-----------------------------------------------------------------------------
// Global variables
//-----------------------------------------------------------------------------
float4x4 g_mInvWorldViewProjection;
float g_fAlpha;
float g_fScale;

texture g_EnvironmentTexture;

sampler EnvironmentSampler = sampler_state
{ 
    Texture = (g_EnvironmentTexture);
    MipFilter = LINEAR;
    MinFilter = LINEAR;
    MagFilter = POINT;
};


//-----------------------------------------------------------------------------
// LightProbe stuff
//-----------------------------------------------------------------------------
struct LightProbeVS_Input
{
    float4 Pos : POSITION;
};

struct LightProbeVS_Output
{
    float4 Pos : POSITION;
    float3 Tex : TEXCOORD0;
};

LightProbeVS_Output LightProbeVS( LightProbeVS_Input Input )
{
    LightProbeVS_Output Output;
    
    Output.Pos = Input.Pos;
    Output.Tex = normalize( mul(Input.Pos, g_mInvWorldViewProjection) );
    
    return Output;
}

float4 LightProbePS( LightProbeVS_Output Input ) : COLOR
{
    float4 color = texCUBE( EnvironmentSampler, Input.Tex )*g_fScale;
    color.a = g_fAlpha;
    return color;
}

technique LightProbe
{
    pass p0
    {
        VertexShader = compile vs_2_0 LightProbeVS();
        PixelShader = compile ps_2_0 LightProbePS();
    }
}




