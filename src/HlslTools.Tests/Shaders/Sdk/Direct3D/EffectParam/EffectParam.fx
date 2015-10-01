//--------------------------------------------------------------------------------------
// File: EffectParam.fx
//
// The effect file for the EffectParam sample.  
// 
// Copyright (c) Microsoft Corporation. All rights reserved.
//--------------------------------------------------------------------------------------


//--------------------------------------------------------------------------------------
// Global variables
//--------------------------------------------------------------------------------------
shared float4x4 g_mWorld;                     // World view matrix
shared float4x4 g_mView; // World * View * Projection matrix
shared float4x4 g_mProj; // World * View * Projection matrix

shared float4 g_vLight = float4( 0.0f, 0.0f, -10.0f, 1.0f );  // Light position in view space
shared float4 g_vLightColor = float4( 1.0f, 1.0f, 1.0f, 1.0f );
texture  g_txScene;
float4   Diffuse;


sampler2D g_samScene =
sampler_state
{
    Texture = <g_txScene>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
};


void VertScene( float4 vPos : POSITION,
                float3 vNormal : NORMAL,
                float2 vTex0 : TEXCOORD0,
                out float4 oDiffuse : COLOR0,
                out float4 oPos : POSITION,
                out float2 oTex0 : TEXCOORD0 )
{
    // Transform the position from object space to homogeneous projection space
    oPos = mul( vPos, g_mWorld );
    oPos = mul( oPos, g_mView );
    oPos = mul( oPos, g_mProj );

    // Compute view space position
    float4 wPos = mul( vPos, g_mWorld );
    wPos = mul( wPos, g_mView );

    // Compute view space normal
    float3 N =  mul( vNormal, (float3x3)g_mWorld );
    N = normalize( mul( N, (float3x3)g_mView ) );

    float3 InvL = g_vLight - wPos;
    float LengthSq = dot( InvL, InvL );

    InvL = normalize( InvL );
    oDiffuse = saturate( dot( N, InvL ) ) * Diffuse * g_vLightColor;// / LengthSq;

    // Just copy the texture coordinate through
    oTex0 = vTex0;
}


float4 PixScene( float4 Diffuse : COLOR0,
                 float2 Tex0 : TEXCOORD0 ) : COLOR0
{
    // Lookup mesh texture and modulate it with diffuse and light color
    return tex2D( g_samScene, Tex0 ) * Diffuse;
//    return float4( tex2D( g_samScene, Tex0 ).xyz, 1.0f ) * Diffuse;
}


//--------------------------------------------------------------------------------------
// Techniques
//--------------------------------------------------------------------------------------
technique RenderScene
{
    pass P0
    {
        VertexShader = compile vs_2_0 VertScene();
        PixelShader  = compile ps_2_0 PixScene();
    }
}
