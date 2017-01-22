//-----------------------------------------------------------------------------
// File: PP_ColorToneMap.fx
//
// Desc: Effect file for image post-processing sample.  This effect contains
//       a single technique with a pixel shader that performs tone mapping
//       on the input color texture.
//
// Copyright (c) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------




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




float Luminance = 0.08f;
static const float fMiddleGray = 0.18f;
static const float fWhiteCutoff = 0.8f;

//-----------------------------------------------------------------------------
// Pixel Shader: ToneMapFilter
// Desc: Perform a tone map filter on the source texture
//-----------------------------------------------------------------------------
float4 ToneMapFilter( float2 Tex : TEXCOORD ) : COLOR0
{
    float4 Color;

    Color = tex2D( g_samSrcColor, Tex ) * fMiddleGray / ( Luminance + 0.001f );
    Color *= ( 1.0f + ( Color / ( fWhiteCutoff * fWhiteCutoff ) ) );
    Color /= ( 1.0f + Color );

    return Color;
}




//-----------------------------------------------------------------------------
// Technique: PostProcess
// Desc: Performs post-processing effect that converts a colored image to
//       black and white.
//-----------------------------------------------------------------------------
technique PostProcess
<
    string Parameter0 = "Luminance";
    float4 Parameter0Def = float4( 0.08f, 0, 0, 0 );
    int Parameter0Size = 1;
    string Parameter0Desc = " (float)";
>
{
    pass p0
    {
        VertexShader = null;
        PixelShader = compile ps_2_0 ToneMapFilter();
        ZEnable = false;
    }
}
