//--------------------------------------------------------------------------------------
// File: DepthOfField10.1.fx
//
// The effect file for the DepthOfField10.1 sample.
//
// Copyright (c) Microsoft Corporation. All rights reserved.
//--------------------------------------------------------------------------------------

struct VSSceneIn
{
    float3 pos    : POSITION;            
    float3 norm : NORMAL;            
    float2 tex    : TEXTURE0;            
};

struct PSSceneIn
{
    float4 pos : SV_Position;
    float2 tex : TEXTURE0;
};

cbuffer cb0
{
    float4x4    g_mWorldViewProj;
};

cbuffer cb1 
{
    float4      g_vDepth;
    float4x4    g_mInvProj;
};

Texture2D                       g_txDiffuse;
Texture2D                       g_txDepth;
Texture2DMS<float>              g_txDepthMSAA;

SamplerState g_samLinear
{
    Filter                      = MIN_MAG_MIP_LINEAR;
    AddressU                    = Clamp;
    AddressV                    = Clamp;
};

BlendState AlphaBlendState
{
    AlphaToCoverageEnable       = FALSE;
    BlendEnable[0]              = TRUE;
    SrcBlend                    = SRC_ALPHA;
    DestBlend                   = ONE;
    BlendOp                     = ADD;
    SrcBlendAlpha               = ZERO;
    DestBlendAlpha              = ZERO;
    BlendOpAlpha                = ADD;
    RenderTargetWriteMask[0]    = 0x0F;
};

RasterizerState DisableCullingNoMSAA
{
    CullMode                    = NONE;
    MultiSampleEnable           = FALSE;
};

RasterizerState EnableCulling
{
    CullMode                    = BACK;
    MultiSampleEnable           = TRUE;
};

DepthStencilState DisableDepthTestWrite
{
    DepthEnable                 = FALSE;
    DepthWriteMask              = ZERO;
};

DepthStencilState DisableDepthWrite
{
    DepthEnable                 = TRUE;
    DepthWriteMask              = ZERO;
};

DepthStencilState EnableDepthTestWrite
{
    DepthEnable                 = TRUE;
    DepthWriteMask              = ALL;
};

BlendState NoBlending
{
    AlphaToCoverageEnable       = FALSE;
    BlendEnable[0]              = FALSE;
};

PSSceneIn 
VSScenemain(
    VSSceneIn input )
{
    PSSceneIn output;
    
    output.pos = mul( float4( input.pos, 1.0 ), g_mWorldViewProj );
    output.tex = input.tex;
    
    return output;
}

PSSceneIn 
VSDepth(
    VSSceneIn input )
{
    PSSceneIn output;
    
    output.pos = mul( float4(input.pos,1.0), g_mWorldViewProj );
    output.tex = output.pos.zw;
    
    return output;
}

PSSceneIn 
VSQuad(
    VSSceneIn input )
{
    PSSceneIn output;
    
    //
    //  Pass the point through
    //
    output.pos = float4(input.pos,1.0);
    output.tex = input.tex;
    
    return output;
}

float4 
PSDepth(
    PSSceneIn input ) : SV_Target
{    
    float fDepth = input.tex.x / input.tex.y;
    
    return fDepth.xxxx;
}

float4 
PSScenemain(
    PSSceneIn input ) : SV_Target
{    
    return g_txDiffuse.Sample( g_samLinear, input.tex );
}

float4 
PSQuad(
    PSSceneIn input,
    uniform bool bSampleMSAA ) : SV_Target
{    
    int2 iScreenCoord = int2( input.tex * g_vDepth.yz );
        
    float fDepth;
    
    if( bSampleMSAA ) {
        fDepth = g_txDepthMSAA.Load( int3( iScreenCoord, 0 ), 0 );
    } else {
        fDepth = g_txDepth.Load( int3( iScreenCoord, 0 ) );
    }
    
    float4 fDepthSample = mul( float4( input.tex, fDepth, 1), g_mInvProj );
    
    fDepth = fDepthSample.z / fDepthSample.w;
    
    float fBlur = abs( fDepth - g_vDepth.w ) / 2;
    
    return g_txDiffuse.SampleLevel( g_samLinear, input.tex, fBlur.x );
    
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

technique10 RenderDepth
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, VSDepth() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PSDepth() ) );
        
        SetBlendState( NoBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( EnableDepthTestWrite, 0 );
        SetRasterizerState( EnableCulling );
    }  
}

technique10 RenderQuad
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, VSQuad() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PSQuad( false ) ) );
        
        SetBlendState( NoBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( DisableDepthTestWrite, 0 );
        SetRasterizerState( DisableCullingNoMSAA );
    }  
}


technique10 RenderQuadMSAA
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_1, VSQuad() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_1, PSQuad( true ) ) );
        
        SetBlendState( NoBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( DisableDepthTestWrite, 0 );
        SetRasterizerState( DisableCullingNoMSAA );
    }  
}
