//-----------------------------------------------------------------------------
// File: PP_DofCombine.fx
//
// Desc: Effect file for image post-processing sample.  This effect contains
//       a single technique with a pixel shader that outputs a value between
//       the original pixel and blurred pixel based on how far the pixel is
//       from a focal plane.
//
// Copyright (c) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------




float4 FocalPlane = float4( 0.0f, 0.0f, 0.2f, -0.6f );

texture g_txSrcColor;
texture g_txSrcNormal;
texture g_txSrcPosition;
texture g_txSrcVelocity;

texture g_txSceneColor;
texture g_txSceneNormal;
texture g_txScenePosition;
texture g_txSceneVelocity;

sampler2D g_samSrcColor =
sampler_state
{
    Texture = <g_txSrcColor>;
    AddressU = Clamp;
    AddressV = Clamp;
    MinFilter = Point;
    MagFilter = Linear;
    MipFilter = Linear;
};
sampler2D g_samSrcNormal =
sampler_state
{
    Texture = <g_txSrcNormal>;
    AddressU = Clamp;
    AddressV = Clamp;
    MinFilter = Point;
    MagFilter = Linear;
    MipFilter = Linear;
};
sampler2D g_samSrcPosition =
sampler_state
{
    Texture = <g_txSrcPosition>;
    AddressU = Clamp;
    AddressV = Clamp;
    MinFilter = Point;
    MagFilter = Linear;
    MipFilter = Linear;
};
sampler2D g_samSrcVelocity =
sampler_state
{
    Texture = <g_txSrcVelocity>;
    AddressU = Clamp;
    AddressV = Clamp;
    MinFilter = Point;
    MagFilter = Linear;
    MipFilter = Linear;
};

sampler2D g_samSceneColor = sampler_state
{
    Texture = <g_txSceneColor>;
    AddressU = Clamp;
    AddressV = Clamp;
    MinFilter = Point;
    MagFilter = Linear;
    MipFilter = Linear;
};
sampler2D g_samSceneNormal = sampler_state
{
    Texture = <g_txSceneNormal>;
    AddressU = Clamp;
    AddressV = Clamp;
    MinFilter = Point;
    MagFilter = Linear;
    MipFilter = Linear;
};
sampler2D g_samScenePosition = sampler_state
{
    Texture = <g_txScenePosition>;
    AddressU = Clamp;
    AddressV = Clamp;
    MinFilter = Point;
    MagFilter = Linear;
    MipFilter = Linear;
};
sampler2D g_samSceneVelocity = sampler_state
{
    Texture = <g_txSceneVelocity>;
    AddressU = Clamp;
    AddressV = Clamp;
    MinFilter = Point;
    MagFilter = Linear;
    MipFilter = Linear;
};




float2 PixelCoordsDownFilter[16] =
{
    { 1.5,  -1.5 },
    { 1.5,  -0.5 },
    { 1.5,   0.5 },
    { 1.5,   1.5 },

    { 0.5,  -1.5 },
    { 0.5,  -0.5 },
    { 0.5,   0.5 },
    { 0.5,   1.5 },

    {-0.5,  -1.5 },
    {-0.5,  -0.5 },
    {-0.5,   0.5 },
    {-0.5,   1.5 },

    {-1.5,  -1.5 },
    {-1.5,  -0.5 },
    {-1.5,   0.5 },
    {-1.5,   1.5 },
};

float2 TexelCoordsDownFilter[16]
<
    string ConvertPixelsToTexels = "PixelCoordsDownFilter";
>;




//-----------------------------------------------------------------------------
// Pixel Shader: DofCombine
// Desc: Combine the source image with the original image based on pixel
//       depths.
//-----------------------------------------------------------------------------
void DofCombine( float2 Tex : TEXCOORD0,
                 float2 Tex2 : TEXCOORD1,
                 out float4 oCol : COLOR0 )
{
    float3 ColorOrig = tex2D( g_samSceneColor, Tex2 );

    float3 ColorBlur = tex2D( g_samSrcColor, Tex );

    float Blur = dot( tex2D( g_samSrcPosition, Tex ), FocalPlane );

    oCol = float4( lerp( ColorOrig, ColorBlur, saturate(abs(Blur)) ), 1.0f );
}




//-----------------------------------------------------------------------------
// Technique: PostProcess
// Desc: Performs post-processing effect that down-filters.
//-----------------------------------------------------------------------------
technique PostProcess
<
    string Parameter0 = "FocalPlane";
    float4 Parameter0Def = float4( 0.0f, 0.0f, 0.2f, -0.6f );
    int Parameter0Size = 4;
    string Parameter0Desc = " (vector of 4 floats)";
>
{
    pass p0
    {
        VertexShader = null;
        PixelShader = compile ps_2_0 DofCombine();
        ZEnable = false;
    }
}
