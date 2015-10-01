//-----------------------------------------------------------------------------
// File: SkyBox.fx
//
// Desc: 
// 
// Copyright (c) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------


//-----------------------------------------------------------------------------
// Global variables
//-----------------------------------------------------------------------------
float4x4 g_mInvWorldViewProjection;

texture g_EnvironmentTexture;

sampler EnvironmentSampler = sampler_state
{ 
    Texture = (g_EnvironmentTexture);
    MipFilter = POINT;
    MinFilter = POINT;
    MagFilter = POINT;
};


//-----------------------------------------------------------------------------
// Skybox stuff
//-----------------------------------------------------------------------------
struct SkyboxVS_Input
{
    float4 Pos : POSITION;
};

struct SkyboxVS_Output
{
    float4 Pos : POSITION;
    float3 Tex : TEXCOORD0;
};

SkyboxVS_Output SkyboxVS( SkyboxVS_Input Input )
{
    SkyboxVS_Output Output;
    
    Output.Pos = Input.Pos;
    Output.Tex = normalize( mul(Input.Pos, g_mInvWorldViewProjection) );
    
    return Output;
}

float4 SkyboxPS( SkyboxVS_Output Input ) : COLOR
{
    float4 color = texCUBE( EnvironmentSampler, Input.Tex );
    return color;
}

technique Skybox
{
    pass p0
    {
        VertexShader = compile vs_2_0 SkyboxVS();
        PixelShader = compile ps_2_0 SkyboxPS();
    }
}




