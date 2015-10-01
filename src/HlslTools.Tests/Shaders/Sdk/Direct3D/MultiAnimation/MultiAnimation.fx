//--------------------------------------------------------------------------------------
// File: ShadowVolume.fx
//
// The effect file for the MultiAnimation sample.  
// 
// Copyright (c) Microsoft Corporation. All rights reserved.
//--------------------------------------------------------------------------------------


#include "skin.vsh"


float4 lhtDir       = { 0.0f, 0.0f, -1.0f, 1.0f };  // Light Direction
float4 lightDiffuse = { 0.6f, 0.6f, 0.6f, 1.0f };   // Light Diffuse
float4 MaterialAmbient : MATERIALAMBIENT = { 0.1f, 0.1f, 0.1f, 1.0f };
float4 MaterialDiffuse : MATERIALDIFFUSE = { 0.8f, 0.8f, 0.8f, 1.0f };

float4x4 g_mWorld : WORLD;
float4x4 g_mViewProj : VIEWPROJECTION;

texture g_txScene;


//--------------------------------------------------------------------------------------
// Texture samplers
//--------------------------------------------------------------------------------------
sampler g_samScene =
sampler_state
{
    Texture = <g_txScene>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Point;
};


//--------------------------------------------------------------------------------------
struct VS_INPUT
{
    float4  Pos             : POSITION;
    float3  BlendWeights    : BLENDWEIGHT;
    float4  BlendIndices    : BLENDINDICES;
    float3  Normal          : NORMAL;
    float3  Tex0            : TEXCOORD0;
};

struct VS_OUTPUT
{
    float4  Pos     : POSITION;
    float4  Diffuse : COLOR0;
    float2  Tex0    : TEXCOORD0;
};


VS_OUTPUT VertScene( float4 Pos : POSITION,
                     float3 Normal : NORMAL,
                     float2 Tex0 : TEXCOORD0 )
{
    VS_OUTPUT o;
    
    o.Pos = mul( Pos, g_mWorld );
    o.Pos = mul( o.Pos, g_mViewProj );
    o.Tex0 = Tex0;
    float3 N = normalize( mul( Normal, (float3x3)g_mWorld ) );

    // Always fully lit the floor
    o.Diffuse = 1.0f;
    
    return o;
}

float4 PixScene( float4 Diffuse : COLOR0,
                 float2 Tex0 : TEXCOORD0 ) : COLOR0
{
    return tex2D( g_samScene, Tex0 ) * Diffuse;
}


VS_OUTPUT VertSkinning( VS_INPUT i, uniform int iNumBones )
{
    VS_OUTPUT   o;
    float3      Pos = 0.0f;
    float3      Normal = 0.0f;
    float       LastWeight = 0.0f;
    
    // skin VB inputs
    VS_SKIN_INPUT vsi = { i.Pos, i.BlendWeights, i.BlendIndices, i.Normal };
    VS_SKIN_OUTPUT vso = VS_Skin( vsi, iNumBones );

    // transform position from world space into view and then projection space
    o.Pos = mul( float4( vso.vPos.xyz, 1.0f ), g_mViewProj );

    // normalize normals
    Normal = normalize( vso.vNor );

    // Shade (Ambient + etc.)
    o.Diffuse = float4( MaterialAmbient.xyz + saturate( dot( Normal, lhtDir.xyz ) ) * MaterialDiffuse.xyz, 1.0 );

    // copy the input texture coordinate through
    o.Tex0  = i.Tex0.xy;

    return o;
}


int CurNumBones = 2;

VertexShader vsArray20[ 4 ] = { compile vs_2_0 VertSkinning( 1 ),
                                compile vs_2_0 VertSkinning( 2 ),
                                compile vs_2_0 VertSkinning( 3 ),
                                compile vs_2_0 VertSkinning( 4 ) };


//--------------------------------------------------------------------------------------
// Techniques
//--------------------------------------------------------------------------------------


technique RenderScene
{
    pass p0
    {
        VertexShader = compile vs_2_0 VertScene();
        PixelShader = compile ps_2_0 PixScene();
    }
}


technique Skinning20
{
    pass p0
    {
        VertexShader = ( vsArray20[ CurNumBones ] );
        PixelShader = compile ps_2_0 PixScene();
    }
}
