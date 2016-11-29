//--------------------------------------------------------------------------------------
// File: EffectPools.fxh
//
// The include effect file that contains common shader variables.
// 
// Copyright (c) Microsoft Corporation. All rights reserved.
//--------------------------------------------------------------------------------------

//--------------------------------------------------------------------------------------
// shader input/output structures
//--------------------------------------------------------------------------------------
struct VS_INPUT
{
    float3 Position   : POSITION;
    float3 Normal     : NORMAL;
    float2 Tex        : TEXCOORD0; 
};

struct PS_INPUT
{
    float4 Position   : POSITION;
    float3 Normal     : TEXCOORD0;
    float2 Tex        : TEXCOORD1;
    float4 Color      : TEXCOORD2;
};

//--------------------------------------------------------------------------------------
// Shared variables
//--------------------------------------------------------------------------------------
#ifdef D3D10

shared cbuffer cbShared
{
float3 g_LightDir;                  // Light's direction in world space
float4 g_LightDiffuse;              // Light's diffuse color
float4x4 g_mViewProj;				// View * Projection matrix
float  g_TexMove;					// random variable that offsets the V texcoord
};

#else

shared float3 g_LightDir;               // Light's direction in world space
shared float4 g_LightDiffuse;           // Light's diffuse color
shared float4x4 g_mViewProj;			// View * Projection matrix
shared float  g_TexMove;				// random variable that offsets the V texcoord

#endif

//-----------------------------------------------------------------------------------------
// Textures and Samplers
//-----------------------------------------------------------------------------------------
shared texture2D g_txDiffuse;
#ifndef D3D10
sampler2D MeshTextureSampler = sampler_state
#else
shared sampler2D MeshTextureSampler = sampler_state
#endif
{
    Texture = (g_txDiffuse);
#ifndef D3D10
    MinFilter = Linear;
    MagFilter = Linear;
#endif
    AddressU = WRAP;
    AddressV = WRAP;
};

//--------------------------------------------------------------------------------------
// Simple pixel shader
//--------------------------------------------------------------------------------------
shared float4 ScenePS( PS_INPUT input ) : COLOR0
{ 
    float4 light = saturate( dot( input.Normal, g_LightDir ) ) * g_LightDiffuse * input.Color;
    return tex2D( MeshTextureSampler, input.Tex) * light;
}