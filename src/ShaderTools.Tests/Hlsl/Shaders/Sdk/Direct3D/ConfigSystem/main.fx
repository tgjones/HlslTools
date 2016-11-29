//--------------------------------------------------------------------------------------
// File: ConfigSystem.fx
//
// The effect file for the ConfigSystem sample.  
// 
// Copyright (c) Microsoft Corporation. All rights reserved.
//--------------------------------------------------------------------------------------


//--------------------------------------------------------------------------------------
// Global variables
//--------------------------------------------------------------------------------------
matrix   g_mWorld;                  // World matrix for object
matrix   g_mView;                   // View matrix for object
matrix   g_mProj;                   // Projection matrix for object
matrix   g_mWorldView;              // World * View matrix
matrix   g_mWorldViewProj;          // World * View * Projection matrix
texture  g_txScene;                 // texture for scene rendering
float4   g_matDiffuse;              // Diffuse component of material
float4   g_matSpecular;             // Specular component of material
float    g_matPower;                // Specular power of material
float4   g_vLightPos[4] = { float4(  3.5f, 1.0f,  5.5f, 1.0f ),
                            float4( -3.5f, 1.0f, -5.5f, 1.0f ),
                            float4(  3.5f, 1.0f, -5.5f, 1.0f ),
                            float4( -3.5f, 1.0f,  5.5f, 1.0f ) };  // Light position in world space
float4   g_vLightColor = float4( 0.5f, 0.5f, 0.5f, 1.0f );

// These parameters will change based on the config flags.
bool     g_bUseSpecular = false;
bool     g_bUseAnisotropic = false;
int      g_MinFilter = 2;           // Minification filtering
int      g_MaxAnisotropy = 1;       // Maximum anisotropy


//-----------------------------------------------------------------------------
// Texture samplers
//-----------------------------------------------------------------------------
sampler g_samScene =
sampler_state
{
    Texture = <g_txScene>;
    MinFilter = <g_MinFilter>;
    MagFilter = Linear;
    MipFilter = Linear;
    MaxAnisotropy = <g_MaxAnisotropy>;
};


void VS20( float4 vPos : POSITION,
           float3 vNormal : NORMAL,
           float2 vTex0 : TEXCOORD0,
           out float4 oPos : POSITION,
           out float2 oTex0 : TEXCOORD0,
           out float3 oVtE : TEXCOORD1,
           out float3 oVNormal : TEXCOORD2,
           out float3 oViewVtL0 : TEXCOORD3,
           out float3 oViewVtL1 : TEXCOORD4,
           out float3 oViewVtL2 : TEXCOORD5,
           out float3 oViewVtL3 : TEXCOORD6 )
{
    oPos = mul( vPos, g_mWorldViewProj );

    // Compute view space normal
    oVNormal = normalize( mul( vNormal, (float3x3)g_mWorldView ) );

    // Vertex pos in view space (normalize in pixel shader)
    oVtE = -mul( vPos, g_mWorldView );

    // Compute view space vertex to light vectors (normalized)
    oViewVtL0 = normalize( mul( g_vLightPos[0], g_mView ) + oVtE );
    oViewVtL1 = normalize( mul( g_vLightPos[1], g_mView ) + oVtE );
    oViewVtL2 = normalize( mul( g_vLightPos[2], g_mView ) + oVtE );
    oViewVtL3 = normalize( mul( g_vLightPos[3], g_mView ) + oVtE );

    // Propogate texcoords
    oTex0 = vTex0;
}


float4 PS20( float2 Tex0 : TEXCOORD0,
             float3 VtE : TEXCOORD1,
             float3 N : TEXCOORD2,
             float3 VtL0 : TEXCOORD3,
             float3 VtL1 : TEXCOORD4,
             float3 VtL2 : TEXCOORD5,
             float3 VtL3 : TEXCOORD6 ) : COLOR0
{
    // Diffuse luminance
    float LumD = max( 0.0f, dot( N, VtL0 ) ) +
                 max( 0.0f, dot( N, VtL1 ) ) +
                 max( 0.0f, dot( N, VtL2 ) ) +
                 max( 0.0f, dot( N, VtL3 ) );

    // Normalize view space vertex-to-eye
    VtE = normalize( VtE );

    // Specular luminance
    float LumS = pow( max( 0.0f, dot( N, normalize( VtE + VtL0 ) ) ), g_matPower ) +
                 pow( max( 0.0f, dot( N, normalize( VtE + VtL1 ) ) ), g_matPower ) +
                 pow( max( 0.0f, dot( N, normalize( VtE + VtL2 ) ) ), g_matPower ) +
                 pow( max( 0.0f, dot( N, normalize( VtE + VtL3 ) ) ), g_matPower );

    float Specular;
    if( g_bUseSpecular )
        Specular = g_matSpecular * LumS * g_vLightColor;
    else
        Specular = 0.0f;

    return tex2D( g_samScene, Tex0 ) * g_matDiffuse * LumD * g_vLightColor + Specular;
}


void VS( float4 vPos : POSITION,
         float3 vNormal : NORMAL,
         float2 vTex0 : TEXCOORD0,
         out float4 oDiffuse : COLOR0,
         out float4 oSpecular : COLOR1,
         out float4 oPos : POSITION,
         out float2 oTex0 : TEXCOORD0 )
{
    oPos = mul( vPos, g_mWorldViewProj );

    // Compute view space normal
    float3 N = normalize( mul( vNormal, (float3x3)g_mWorldView ) );

    // Vertex pos in view space
    float3 Vv = mul( vPos, g_mWorldView );

    float LumD = 0.0f;  // Diffuse luminance
    float LumS = 0.0f;  // Specular luminance
    for( int i = 0; i < 4; ++i )
    {
        // Light pos in view space
        float3 Lv = mul( g_vLightPos[i], g_mView );

        // View space vertex-to-light
        float3 VtL = normalize( Lv - Vv );

        LumD += max( 0.0f, dot( N, VtL ) );

        // View space vertex-to-eye
        float3 VtE = normalize( -Vv );

        // Half vector
        float3 H = normalize( VtE + VtL );

        LumS += pow( max( 0.0f, dot( N, H ) ), 40.0f ); // This power is hardcoded to not exceed inst limit.
    }

    oDiffuse = g_matDiffuse * LumD * g_vLightColor;
    if( g_bUseSpecular )
        oSpecular = g_matSpecular * LumS * g_vLightColor;
    else
        oSpecular = 0.0f;

    oTex0 = vTex0;
}


float4 PS( float4 Diffuse : COLOR0,
           float4 Specular : COLOR1,
           float2 Tex0 : TEXCOORD0 ) : COLOR0
{
    return tex2D( g_samScene, Tex0 ) * Diffuse + Specular;
}


//--------------------------------------------------------------------------------------
// Techniques
//--------------------------------------------------------------------------------------
technique RenderScenePS30
{
    pass P0
    {
        VertexShader = compile vs_3_0 VS20();
        PixelShader = compile ps_3_0 PS20();
    }
}


technique RenderScenePS20A
{
    pass P0
    {
        VertexShader = compile vs_2_0 VS20();
        PixelShader = compile ps_2_a PS20();
    }
}


technique RenderScenePS20B
{
    pass P0
    {
        VertexShader = compile vs_2_0 VS20();
        PixelShader = compile ps_2_b PS20();
    }
}


technique RenderScenePS20
{
    pass P0
    {
        VertexShader = compile vs_2_0 VS20();
        PixelShader = compile ps_2_0 PS20();
    }
}


technique RenderScenePS14
{
    pass P0
    {
        VertexShader = compile vs_1_1 VS();
        PixelShader = compile ps_1_4 PS();
    }
}


technique RenderScenePS13
{
    pass P0
    {
        VertexShader = compile vs_1_1 VS();
        PixelShader = compile ps_1_3 PS();
    }
}


technique RenderScenePS12
{
    pass P0
    {
        VertexShader = compile vs_1_1 VS();
        PixelShader = compile ps_1_2 PS();
    }
}


technique RenderScenePS11
{
    pass P0
    {
        VertexShader = compile vs_1_1 VS();
        PixelShader = compile ps_1_1 PS();
    }
}


technique FFRenderScene
{
    pass P0
    {
        VertexShader = null;
        PixelShader = null;

        WorldTransform[0] = <g_mWorld>;
        ViewTransform = <g_mView>;
        ProjectionTransform = <g_mProj>;

        Texture[0] = <g_txScene>;
        MinFilter[0] = <g_MinFilter>;
        MaxAnisotropy[0] = <g_MaxAnisotropy>;
        MagFilter[0] = Linear;
        MipFilter[0] = Linear;
        AddressU[0] = Wrap;
        AddressV[0] = Wrap;
        AddressW[0] = Wrap;
        ColorArg1[0] = Texture;
        ColorArg2[0] = Current;
        ColorOp[0] = Modulate;

        Lighting = True;
        LightEnable[0] = True;
//        LightAmbient[0] = float4( 0.0f, 0.0f, 0.0f, 0.0f );
        LightType[0] = Point;
        LightPosition[0] = <g_vLightPos[0]>;
        LightRange[0] = 100.0f;
        LightDiffuse[0] = <g_vLightColor>;
        LightSpecular[0] = <g_vLightColor>;
        LightAttenuation0[0] = 1.0f;
        LightAttenuation1[0] = 0.0f;
        LightAttenuation2[0] = 0.0f;
        LightEnable[1] = True;
//        LightAmbient[1] = float4( 0.0f, 0.0f, 0.0f, 0.0f );
        LightType[1] = Point;
        LightPosition[1] = <g_vLightPos[1]>;
        LightRange[1] = 100.0f;
        LightDiffuse[1] = <g_vLightColor>;
        LightSpecular[1] = <g_vLightColor>;
        LightAttenuation0[1] = 1.0f;
        LightAttenuation1[1] = 0.0f;
        LightAttenuation2[1] = 0.0f;
        LightEnable[2] = True;
//        LightAmbient[2] = float4( 0.0f, 0.0f, 0.0f, 0.0f );
        LightType[2] = Point;
        LightPosition[2] = <g_vLightPos[2]>;
        LightRange[2] = 100.0f;
        LightDiffuse[2] = <g_vLightColor>;
        LightSpecular[2] = <g_vLightColor>;
        LightAttenuation0[2] = 1.0f;
        LightAttenuation1[2] = 0.0f;
        LightAttenuation2[2] = 0.0f;
        LightEnable[3] = True;
//        LightAmbient[3] = float4( 0.0f, 0.0f, 0.0f, 0.0f );
        LightType[3] = Point;
        LightPosition[3] = <g_vLightPos[3]>;
        LightRange[3] = 100.0f;
        LightDiffuse[3] = <g_vLightColor>;
        LightSpecular[3] = <g_vLightColor>;
        LightAttenuation0[3] = 1.0f;
        LightAttenuation1[3] = 0.0f;
        LightAttenuation2[3] = 0.0f;

        NormalizeNormals = true;

        SpecularEnable = <g_bUseSpecular>;
        Ambient = float4( 0.0f, 0.0f, 0.0f, 0.0f );
        DiffuseMaterialSource = Material;
        SpecularMaterialSource = Material;
        MaterialAmbient = float4( 0.0f, 0.0f, 0.0f, 0.0f );
        MaterialDiffuse = <g_matDiffuse>;
        MaterialEmissive = float4( 0.0f, 0.0f, 0.0f, 0.0f );
        MaterialSpecular = <g_matSpecular>;
        MaterialPower = <g_matPower>;
    }
}
