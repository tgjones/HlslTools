//--------------------------------------------------------------------------------------
// File: DrawPredicated.fx
//
// The effect file for the DrawPredicated sample.
//
// Copyright (c) Microsoft Corporation. All rights reserved.
//--------------------------------------------------------------------------------------
struct VSSceneIn
{
	float3 pos	: POSITION;			//position
	float3 norm : NORMAL;			//normal
	float2 tex	: TEXTURE0;			//texture coordinate
};

struct PSSceneIn
{
	float4 pos : SV_Position;
	float2 tex : TEXTURE0;
};

cbuffer cb0
{
	float4x4 g_mWorldViewProj;
};

Texture2D g_txDiffuse;
SamplerState g_samLinear
{
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = Clamp;
    AddressV = Clamp;
};

BlendState OccTestBlendState
{
    AlphaToCoverageEnable = FALSE;
    SrcBlend = SRC_ALPHA;
    DestBlend = ONE;
    BlendOp = ADD;
    SrcBlendAlpha = ZERO;
    DestBlendAlpha = ZERO;
    BlendOpAlpha = ADD;
    BlendEnable[0] = FALSE;
    RenderTargetWriteMask[0] = 0x0;
};

BlendState AlphaBlendState
{
    AlphaToCoverageEnable = FALSE;
    BlendEnable[0] = TRUE;
    SrcBlend = SRC_ALPHA;
    DestBlend = ONE;
    BlendOp = ADD;
    SrcBlendAlpha = ZERO;
    DestBlendAlpha = ZERO;
    BlendOpAlpha = ADD;
    RenderTargetWriteMask[0] = 0x0F;
};

RasterizerState DisableCulling
{
	CullMode = NONE;
};

RasterizerState EnableCulling
{
	CullMode = BACK;
};

DepthStencilState DisableDepthTestWrite
{
    DepthEnable = FALSE;
    DepthWriteMask = ZERO;
};

DepthStencilState DisableDepthWrite
{
    DepthEnable = TRUE;
    DepthWriteMask = ZERO;
};

DepthStencilState EnableDepthTestWrite
{
    DepthEnable = TRUE;
    DepthWriteMask = ALL;
};

BlendState NoBlending
{
    AlphaToCoverageEnable = FALSE;
    BlendEnable[0] = FALSE;
};

PSSceneIn VSScenemain(VSSceneIn input)
{
	PSSceneIn output;
	
	//
	// Pass the point through
	//
	output.pos = mul( float4(input.pos,1.0), g_mWorldViewProj );
	output.tex = input.tex;
	
	return output;
}

float4 PSScenemain(PSSceneIn input) : SV_Target
{	
	return g_txDiffuse.Sample( g_samLinear, input.tex );
}

float4 PSOccluder(PSSceneIn input) : SV_Target
{	
	return float4(1,0,0,0.5);
}

technique10 RenderTextured
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, VSScenemain() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PSScenemain() ) );
        
        SetBlendState( NoBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( EnableDepthTestWrite, 0 );
        SetRasterizerState( EnableCulling );
    }  
}

technique10 RenderOnTop
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, VSScenemain() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PSOccluder() ) );
        
        SetBlendState( AlphaBlendState, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( DisableDepthTestWrite, 0 );
        SetRasterizerState( EnableCulling );
    }  
}

technique10 RenderOccluder
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, VSScenemain() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PSOccluder() ) );
        
        SetBlendState( OccTestBlendState, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( DisableDepthWrite, 0 );
        SetRasterizerState( DisableCulling );
    }  
}

