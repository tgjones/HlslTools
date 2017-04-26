//--------------------------------------------------------------------------------------
// File: Paint.fx
//
// Copyright (c) Microsoft Corporation. All rights reserved.
//--------------------------------------------------------------------------------------
#include "common.fxh"

struct VSSceneIn
{
    float3 Pos              : POSITION;
    float3 Norm             : NORMAL;
    float2 Tex				: TEXCOORD0;
};

struct VSQuadIn
{
    float3 Pos              : POSITION;
    float2 Tex				: TEXCOORD0;
};

struct PSUVIn
{
    float4 Pos				: SV_Position;
    float4 wPos             : WORLDPOS;
};

struct PSQuadIn
{
    float4 Pos				: SV_Position;
    float2 Tex              : TEXCOORD0;
};

cbuffer cb0
{
    float4x4 g_mWorld;
};

cbuffer cbPerFrame
{
	int g_NumParticles;
	int g_ParticleStart;
	int g_ParticleStep;
	float g_fParticleRadiusSq;
	float4 g_vParticleColor = float4(0,1,0,1);
}

Buffer<float4> g_ParticleBuffer;

Texture2D g_txDiffuse;
SamplerState g_samLinear
{
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = Clamp;
    AddressV = Clamp;
};

SamplerState g_samPoint
{
    Filter = MIN_MAG_MIP_POINT;
    AddressU = Clamp;
    AddressV = Clamp;
};

BlendState NoBlending
{
    AlphaToCoverageEnable = FALSE;
    BlendEnable[0] = FALSE;
};

BlendState PaintBlending
{
    AlphaToCoverageEnable = FALSE;
    BlendEnable[0] = TRUE;
    SrcBlend = SRC_ALPHA;
    DestBlend = INV_SRC_ALPHA;
    BlendOp = ADD;
    SrcBlendAlpha = SRC_ALPHA;
    DestBlendAlpha = INV_SRC_ALPHA;
    BlendOpAlpha = ADD;
    RenderTargetWriteMask[0] = 0x0F;
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

RasterizerState CullBack
{
	CullMode = BACK;
};

RasterizerState CullNone
{
	CullMode = NONE;
};

//
// Vertex shader for drawing the scene into UV space
//
PSUVIn VSRenderToUV(VSSceneIn input)
{
    PSUVIn output = (PSUVIn)0;
    
    float2 screenTex = input.Tex*float2(2,2) - float2(1,1);
    output.Pos = float4( screenTex, 0.5, 1 );
    output.wPos = mul( float4(input.Pos,1), g_mWorld );
    
    return output;
}

//
// Vertex shader for drawing the animated scene into UV space
//
PSUVIn VSRenderAnimToUV(VSAnimIn input)
{
    PSUVIn output = (PSUVIn)0;
    
    SkinnedInfo vSkinned = SkinVert( input );
    
    float2 screenTex = input.Tex*2 - float2(1,1);
    output.Pos = float4( screenTex, 0.5, 1 );
    output.wPos = vSkinned.Pos;
   
    return output;
}

//
// Pixel shader for drawing the scene into UV
//
float4 PSRenderToUV(PSUVIn input) : SV_Target
{   
    return input.wPos;
}

//
// Vertex for painting particles into the texture
//
PSQuadIn VSQuad(VSQuadIn input)
{
    PSQuadIn output = (PSQuadIn)0;
    
    output.Pos = float4(input.Pos,1);
    output.Tex = input.Tex;
    
    return output;
}

//
// Pixel shader for painting
//
float4 PSPaint(PSQuadIn input) : SV_Target
{   	
	// get the position
	float4 meshPos = g_txDiffuse.Sample( g_samPoint, input.Tex );
	
	// loop through the particles
	float3 color = float3(0,0,0);
	float alpha = 0;
    for( int i=g_ParticleStart; i<g_NumParticles; i+=g_ParticleStep )
    {
		// load the particle
		float4 particlePos = g_ParticleBuffer.Load( i*4 );
		float4 particleColor = g_ParticleBuffer.Load( (i*4) + 2 );
		
		float3 delta = particlePos.xyz - meshPos.xyz;
		float distSq = dot( delta, delta );
		if( distSq < g_fParticleRadiusSq )
		{
			color = color.xyz*(1-particleColor.a) + particleColor.xyz * particleColor.a;
			alpha += particleColor.a;
		}
    }
   
    return saturate( float4(color,alpha) );
}

//
// Render to UV
//
technique10 RenderToUV
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, VSRenderToUV() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PSRenderToUV() ) );
        
        SetBlendState( NoBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( DisableDepth, 0 );
        SetRasterizerState( CullNone );
    }  
}


//
// Render animated mesh to UV
//
technique10 RenderAnimToUV
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, VSRenderAnimToUV() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PSRenderToUV() ) );
        
        SetBlendState( NoBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( DisableDepth, 0 );
        SetRasterizerState( CullNone );
    }  
}


//
// Paint
//
technique10 Paint
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, VSQuad() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PSPaint() ) );
        
        SetBlendState( PaintBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( DisableDepth, 0 );
        SetRasterizerState( CullNone );
    }  
}