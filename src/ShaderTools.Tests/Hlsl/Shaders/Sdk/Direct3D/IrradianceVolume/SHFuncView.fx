//-----------------------------------------------------------------------------
// File: SHFuncView.fx
//
// Copyright (c) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// Global variables
//-----------------------------------------------------------------------------
float4x4 g_mWorldViewProjection;
float3x3 g_mNormalXform;


//-----------------------------------------------------------------------------
// Vertex shader output structure
//-----------------------------------------------------------------------------
struct VS_OUTPUT
{
    float4 Position   : POSITION;      // position of the vertex
    float4 Color      : COLOR0;
};


VS_OUTPUT RenderSphere(float4 vPos    : POSITION,
                       float3 vNorm   : NORMAL,
                       float4 vAlbedo : COLOR0)   // B quartic/quintic      
{
    VS_OUTPUT Output;

    Output.Position = mul(vPos, g_mWorldViewProjection);
    float3 Normal = mul(vNorm, g_mNormalXform); // should be eye-space normal...

    float3 Light0 = normalize(float3(0.8,0.8,-0.5));
    float3 Light1 = normalize(float3(-0.8,-0.8,-0.5));
    float  fScale = 0.6f;
    
    float3 dColor = (saturate(dot(Light0,Normal)) + saturate(dot(Light1,Normal)))*fScale*vAlbedo;
    float3 H0 = normalize(Light0+float3(0,0,-1));
    float3 H1 = normalize(Light1+float3(0,0,-1));

    dColor += (pow(saturate(dot(H0,Normal)),6) + pow(saturate(dot(H1,Normal)),6))*0.9f;

    Output.Color = float4(dColor,1);
    
    return Output;
}


technique Render
{
    pass p0
    {
        CullMode = CCW;
        VertexShader = compile vs_2_0 RenderSphere();
    }
}

