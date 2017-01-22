//--------------------------------------------------------------------------------------
// File: ShadowVolume.fx
//
// The effect file for the ShadowVolume sample.  
// 
// Copyright (c) Microsoft Corporation. All rights reserved.
//--------------------------------------------------------------------------------------


#define LIGHT_FALLOFF 1.2f


//--------------------------------------------------------------------------------------
// Global variables
//--------------------------------------------------------------------------------------
float4   g_vAmbient;                // Ambient light color
float3   g_vLightView;              // View space light position/direction
float4   g_vLightColor;             // Light color
float4   g_vShadowColor;            // Shadow volume color (for visualization)
float4   g_vMatColor;               // Color of the material
float4x4 g_mWorldView;              // World * View matrix
float4x4 g_mProj;                   // Projection matrix
float4x4 g_mWorldViewProjection;    // World * View * Projection matrix
texture  g_txScene;                 // texture for scene rendering
float    g_fFarClip;                // Z of far clip plane


//-----------------------------------------------------------------------------
// Texture samplers
//-----------------------------------------------------------------------------
sampler g_samScene =
sampler_state
{
    Texture = <g_txScene>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
};


void VertSceneAmbient( float4 vPos : POSITION,
                       float2 vTex0 : TEXCOORD0,
                       out float4 oPos : POSITION,
                       out float2 oTex0 : TEXCOORD0 )
{
    // Transform the position from object space to homogeneous projection space
    oPos = mul( vPos, g_mWorldViewProjection );

    // Just copy the texture coordinate through
    oTex0 = vTex0;
}


float4 PixSceneAmbient( float2 Tex0 : TEXCOORD0 ) : COLOR0
{
    // Lookup mesh texture and modulate it with material and ambient amount
    return g_vAmbient * tex2D( g_samScene, Tex0 ) * g_vMatColor;
}


void VertScene( float4 vPos : POSITION,
                float3 vNormal : NORMAL,
                float2 vTex0 : TEXCOORD0,
                out float4 oPos : POSITION,
                out float4 ViewPos : TEXCOORD0,
                out float3 ViewNormal : TEXCOORD1,
                out float2 oTex0 : TEXCOORD2,
                out float4 oDiffuse : TEXCOORD3 )
{
    // Transform the position from view space to homogeneous projection space
    oPos = mul( vPos, g_mWorldViewProjection );

    // Compute view space position
    ViewPos = mul( vPos, g_mWorldView );

    // Compute world space normal
    ViewNormal = normalize( mul( vNormal, (float3x3)g_mWorldView ) );

    // Modulate material with light to obtain diffuse
    oDiffuse = g_vMatColor * g_vLightColor;

    // Just copy the texture coordinate through
    oTex0 = vTex0;
}


float4 PixScene( float4 ViewPos : TEXCOORD0,
                 float3 ViewNormal : TEXCOORD1,
                 float2 Tex0 : TEXCOORD2,
                 float4 Diffuse : TEXCOORD3 ) : COLOR0
{
    // Pixel to light vector
    float3 L = g_vLightView - ViewPos;
    float LenSq = dot( L, L );
    L = normalize( L );

    // Compute lighting amount
    float4 I = saturate( dot( normalize( ViewNormal ), L ) ) * Diffuse *
               (LIGHT_FALLOFF * LIGHT_FALLOFF) / LenSq;

    // Lookup mesh texture and modulate it with diffuse
    return float4( tex2D( g_samScene, Tex0 ).xyz, 1.0f ) * I;
}


void VertShadowVolume( float4 vPos : POSITION,
                       float3 vNormal : NORMAL,
                       out float4 oPos : POSITION )
{
    // Compute view space normal
    float3 N = mul( vNormal, (float3x3)g_mWorldView );

    // Obtain view space position
    float4 PosView = mul( vPos, g_mWorldView );

    // Light-to-vertex vector in view space
    float3 LightVecView = PosView - g_vLightView;

    // Perform reverse vertex extrusion
    // Extrude the vertex away from light if it's facing away from the light.
    if( dot( N, -LightVecView ) < 0.0f )
    {
        if( PosView.z > g_vLightView.z )
            PosView.xyz += LightVecView * ( g_fFarClip - PosView.z ) / LightVecView.z;
        else
            PosView = float4( LightVecView, 0.0f );

        // Transform the position from view space to homogeneous projection space
        oPos = mul( PosView, g_mProj );
    } else
        oPos = mul( vPos, g_mWorldViewProjection );
}


float4 PixShadowVolume() : COLOR0
{
    return float4( g_vShadowColor.xyz, 0.1f );
}


float4 ShowDirtyStencil() : COLOR0
{
    return g_vShadowColor;
}


float4 PixComplexity( uniform float4 Color ) : COLOR0
{
    return Color;
}


//--------------------------------------------------------------------------------------
// Techniques
//--------------------------------------------------------------------------------------
technique RenderSceneAmbient
{
    pass P0
    {
        VertexShader = compile vs_2_0 VertSceneAmbient();
        PixelShader  = compile ps_2_0 PixSceneAmbient();
        StencilEnable = false;
        ZFunc = LessEqual;
    }
}


technique ShowShadowVolume
{
    pass P0
    {
        VertexShader = compile vs_2_0 VertShadowVolume();
        PixelShader  = compile ps_2_0 PixShadowVolume();
        CullMode = Ccw;
        AlphaBlendEnable = true;
        SrcBlend = SrcAlpha;
        DestBlend = InvSrcAlpha;
        // Disable writing to depth buffer
        ZWriteEnable = false;
        ZFunc = Less;
        // Setup stencil states
        StencilEnable = true;
        StencilRef = 1;
        StencilMask = 0xFFFFFFFF;
        StencilWriteMask = 0xFFFFFFFF;
        StencilFunc = Always;
        StencilZFail = Decr;
        StencilPass = Keep;
    }
    pass P1
    {
        VertexShader = compile vs_2_0 VertShadowVolume();
        PixelShader  = compile ps_2_0 PixShadowVolume();
        CullMode = Cw;
        StencilZFail = Incr;
    }
}


technique ShowShadowVolume2Sided
{
    pass P0
    {
        VertexShader = compile vs_2_0 VertShadowVolume();
        PixelShader  = compile ps_2_0 PixShadowVolume();
        CullMode = None;
        AlphaBlendEnable = true;
        SrcBlend = SrcAlpha;
        DestBlend = InvSrcAlpha;
        // Disable writing to depth buffer
        ZWriteEnable = false;
        ZFunc = Less;
        // Setup stencil states
        TwoSidedStencilMode = true;
        StencilEnable = true;
        StencilRef = 1;
        StencilMask = 0xFFFFFFFF;
        StencilWriteMask = 0xFFFFFFFF;
        Ccw_StencilFunc = Always;
        Ccw_StencilZFail = Incr;
        Ccw_StencilPass = Keep;
        StencilFunc = Always;
        StencilZFail = Decr;
        StencilPass = Keep;
    }
}


technique RenderShadowVolume
{
    pass P0
    {
        VertexShader = compile vs_2_0 VertShadowVolume();
        PixelShader  = compile ps_2_0 PixShadowVolume();
        CullMode = Ccw;
        // Disable writing to the frame buffer
        AlphaBlendEnable = true;
        SrcBlend = Zero;
        DestBlend = One;
        // Disable writing to depth buffer
        ZWriteEnable = false;
        ZFunc = Less;
        // Setup stencil states
        StencilEnable = true;
        StencilRef = 1;
        StencilMask = 0xFFFFFFFF;
        StencilWriteMask = 0xFFFFFFFF;
        StencilFunc = Always;
        StencilZFail = Decr;
        StencilPass = Keep;
    }
    pass P1
    {
        VertexShader = compile vs_2_0 VertShadowVolume();
        PixelShader  = compile ps_2_0 PixShadowVolume();
        CullMode = Cw;
        StencilZFail = Incr;
    }
}


technique RenderShadowVolume2Sided
{
    pass P0
    {
        VertexShader = compile vs_2_0 VertShadowVolume();
        PixelShader  = compile ps_2_0 PixShadowVolume();
        CullMode = None;
        // Disable writing to the frame buffer
        AlphaBlendEnable = true;
        SrcBlend = Zero;
        DestBlend = One;
        // Disable writing to depth buffer
        ZWriteEnable = false;
        ZFunc = Less;
        // Setup stencil states
        TwoSidedStencilMode = true;
        StencilEnable = true;
        StencilRef = 1;
        StencilMask = 0xFFFFFFFF;
        StencilWriteMask = 0xFFFFFFFF;
        Ccw_StencilFunc = Always;
        Ccw_StencilZFail = Incr;
        Ccw_StencilPass = Keep;
        StencilFunc = Always;
        StencilZFail = Decr;
        StencilPass = Keep;
    }
}


technique RenderShadowVolumeComplexity
{
    pass P0
    {
        VertexShader = compile vs_2_0 VertShadowVolume();
        PixelShader  = compile ps_2_0 PixShadowVolume();
        CullMode = None;
        // Disable writing to the frame buffer
        AlphaBlendEnable = false;
        // Disable writing to depth buffer
        ZWriteEnable = false;
        ZFunc = Less;
        // Setup stencil states
        StencilEnable = true;
        StencilRef = 1;
        StencilMask = 0xFFFFFFFF;
        StencilWriteMask = 0xFFFFFFFF;
        StencilFunc = Always;
        StencilZFail = Incr;
        StencilPass = Incr;
    }
}


technique RenderScene
{
    pass P0
    {
        VertexShader = compile vs_2_0 VertScene();
        PixelShader  = compile ps_2_0 PixScene();
        ZEnable = true;
        ZFunc = LessEqual;
        StencilEnable = true;
        AlphaBlendEnable = true;
        BlendOp = Add;
        SrcBlend = One;
        DestBlend = One;
        StencilRef = 1;
        StencilFunc = Greater;
        StencilPass = Keep;
    }
}


technique RenderDirtyStencil
{
    pass P0
    {
        VertexShader = compile vs_2_0 VertScene();
        PixelShader  = compile ps_2_0 ShowDirtyStencil();
        ZEnable = false;
        StencilEnable = true;
        AlphaBlendEnable = true;
        SrcBlend = SrcAlpha;
        DestBlend = InvSrcAlpha;
        StencilRef = 0;
        StencilFunc = Less;
        StencilPass = Keep;
    }
}


technique RenderComplexity
{
    pass p0
    {
        VertexShader = null;
        PixelShader = compile ps_2_0 PixComplexity( float4( 1.0f, 1.0f, 1.0f, 1.0f ) );
        StencilRef = 71;
        ZEnable = false;
        StencilEnable = true;
        AlphaBlendEnable = false;
        StencilFunc = LessEqual;
        StencilPass = Zero;
        StencilFail = Keep;
    }
    pass p1
    {
        PixelShader = compile ps_2_0 PixComplexity( float4( 1.0f, 0.0f, 0.0f, 1.0f ) );
        StencilRef = 51;
    }
    pass p2
    {
        PixelShader = compile ps_2_0 PixComplexity( float4( 1.0f, 0.5f, 0.0f, 1.0f ) );
        StencilRef = 41;
    }
    pass p3
    {
        PixelShader = compile ps_2_0 PixComplexity( float4( 1.0f, 1.0f, 0.0f, 1.0f ) );
        StencilRef = 31;
    }
    pass p4
    {
        PixelShader = compile ps_2_0 PixComplexity( float4( 0.0f, 1.0f, 0.0f, 1.0f ) );
        StencilRef = 21;
    }
    pass p5
    {
        PixelShader = compile ps_2_0 PixComplexity( float4( 0.0f, 1.0f, 1.0f, 1.0f ) );
        StencilRef = 11;
    }
    pass p6
    {
        PixelShader = compile ps_2_0 PixComplexity( float4( 0.0f, 0.0f, 1.0f, 1.0f ) );
        StencilRef = 6;
    }
    pass p7
    {
        PixelShader = compile ps_2_0 PixComplexity( float4( 1.0f, 0.0f, 1.0f, 1.0f ) );
        StencilRef = 1;
    }
}
