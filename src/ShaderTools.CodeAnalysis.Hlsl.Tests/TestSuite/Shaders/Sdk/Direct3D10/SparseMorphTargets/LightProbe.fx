//-----------------------------------------------------------------------------
// File: lightprobe.fx
//
// Desc: Rendering a cubic light probe LightProbe
// 
// Copyright (c) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

struct LightProbeVS_Input
{
    float4 Pos : POSITION;
};

struct LightProbeVS_Output
{
    float4 Pos : SV_Position;
    float3 Tex : TEXCOORD0;
};

//-----------------------------------------------------------------------------
// Global variables
//-----------------------------------------------------------------------------
cbuffer cbLightProbe
{
	matrix g_mInvWorldViewProjection;
	float g_fAlpha = 1.0f;
	float g_fScale = 1.0f;
};

TextureCube g_txEnvironmentTexture;

SamplerState g_samPointCube
{
    Filter = MIN_MAG_MIP_POINT;
    AddressU = Clamp;
    AddressV = Clamp;
};

SamplerState g_samLinearCube
{
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = Clamp;
    AddressV = Clamp;
};

#if USE_POINT_CUBE_SAMPLING == 1
#define g_samCubeFilter g_samLinearCube
#else
#define g_samCubeFilter g_samPointCube
#endif

BlendState NoBlending
{
    AlphaToCoverageEnable = FALSE;
    BlendEnable[0] = FALSE;
    BlendEnable[1] = FALSE;
    BlendEnable[2] = FALSE;
};

DepthStencilState DisableDepth
{
    DepthEnable = FALSE;
    DepthWriteMask = ZERO;
};

RasterizerState DisableCulling
{
	CullMode = NONE;
};

//-----------------------------------------------------------------------------
// LightProbe stuff
//-----------------------------------------------------------------------------

LightProbeVS_Output LightProbeVS( LightProbeVS_Input Input )
{
    LightProbeVS_Output Output;
    
    Output.Pos = Input.Pos;
    Output.Tex = normalize( mul( Input.Pos, g_mInvWorldViewProjection) );
    
    return Output;
}

float4 LightProbePS( LightProbeVS_Output Input ) : SV_Target
{
    float4 color = g_txEnvironmentTexture.Sample( g_samCubeFilter, Input.Tex )*g_fScale;
    color.a = g_fAlpha;
    return color;
}

technique10 LightProbe
{
    pass p0
    {
		SetVertexShader( CompileShader( vs_4_0, LightProbeVS() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, LightProbePS() ) );
        
        SetDepthStencilState( DisableDepth, 0 );
        SetBlendState( NoBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetRasterizerState( DisableCulling );
    }
}




