//--------------------------------------------------------------------------------------
// File: Meshes.fx
//
// Copyright (c) Microsoft Corporation. All rights reserved.
//--------------------------------------------------------------------------------------
#include "Common.fxh"

struct VSSceneIn
{
    float3 Pos              : POSITION;
    float3 Norm             : NORMAL;
    float2 Tex				: TEXCOORD0;
};

struct PSSceneIn
{
    float4 Pos				: SV_POSITION;
    float3 Norm             : NORMAL;
    float2 Tex				: TEXCOORD0;
};

cbuffer cb0
{
    float4x4 g_mWorldViewProj;
    float4x4 g_mViewProj;
    float4x4 g_mWorld;
    float3   g_vLightDir;
    float3   g_vEyePt;
};

cbuffer cbImmutable
{
	float4	g_vAmbient = float4(0.2, 0.2, 0.3, 0.0);
};

Texture2D g_txDiffuse;
Texture2D g_txNormal;
Texture2D g_txSpecular;
Texture2D g_txPaint;
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

DepthStencilState EnableDepth
{
    DepthEnable = TRUE;
    DepthWriteMask = ALL;
};

RasterizerState CullBack
{
	CullMode = BACK;
};


//
// Vertex shader for drawing the scene
//
PSSceneIn VSScene( VSSceneIn input )
{
    PSSceneIn output = (PSSceneIn)0;
    
    output.Pos = mul( float4(input.Pos,1), g_mWorldViewProj );
    output.Norm = normalize( mul( input.Norm, (float3x3)g_mWorld ) );
    output.Tex = input.Tex;
    
    return output;
}


//
// Vertex shader for drawing the animated scene
//
VSAnimOut VSAnim( VSAnimIn input )
{
    VSAnimOut output;
    
    SkinnedInfo vSkinned = SkinVert( input );
    output.Pos = mul( vSkinned.Pos, g_mViewProj );
    output.vPos = vSkinned.Pos;
    output.Norm = normalize( vSkinned.Norm );
    output.Tan = normalize( vSkinned.Tan );
    output.Tex = input.Tex;
    
    return output;
}


//
// Pixel shader for drawing the scene
//
float4 PSScene( PSSceneIn input ) : SV_Target
{   
	// calc lighting
	float4 light = saturate( dot( g_vLightDir, input.Norm ) ).xxxx;
	
    return g_txDiffuse.Sample( g_samLinear, input.Tex ) * (light + g_vAmbient);
}


//
// Pixel shader for drawing the animated scene
//
float4 PSAnim( VSAnimOut input ) : SV_Target
{   
	float4 diffuse = g_txDiffuse.Sample( g_samLinear, input.Tex )*float4(1,1,1,0.3);
	float4 paint = g_txPaint.Sample( g_samLinear, input.Tex );
    float3 Norm = g_txNormal.Sample( g_samLinear, input.Tex );
    Norm *= 2.0;
    Norm -= float3(1,1,1);
    
    float3 lightDir = g_vLightDir;
    float3 viewDir = normalize( g_vEyePt - input.vPos );
    float3 BiNorm = normalize( cross( input.Norm, input.Tan ) );
    float3x3 BTNMatrix = float3x3( BiNorm, input.Tan, input.Norm );
    Norm = normalize( mul( Norm, BTNMatrix ) ); //world space bump
    
    //diffuse lighting
    float lightAmt = saturate( dot( lightDir, Norm ) );
    float4 lightColor = lightAmt.xxxx + g_vAmbient;

    // Calculate specular power
    float3 halfAngle = normalize( viewDir + lightDir );
    float4 spec = pow( saturate(dot( halfAngle, Norm )), 64 );
        
    // combine diffuse with paint
    diffuse = lerp( diffuse, paint, paint.w );
    
    // Return combined lighting
    return lightColor*diffuse + spec*diffuse.a;
}

//
// RenderScene
//
technique10 RenderScene
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, VSScene() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PSScene() ) );
        
        SetBlendState( NoBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( EnableDepth, 0 );
        SetRasterizerState( CullBack );
    }  
}

//
// RenderAnimScene
//
technique10 RenderAnimScene
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, VSAnim() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PSAnim() ) );
        
        SetBlendState( NoBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( EnableDepth, 0 );
        SetRasterizerState( CullBack );
    }  
}