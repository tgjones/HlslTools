//-----------------------------------------------------------------------------
// File: HDRCubeMap.fx
//
// Desc: Effect file for high dynamic range cube mapping sample.
//
// Copyright (c) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------




#ifndef MAX_NUM_LIGHTS
#define MAX_NUM_LIGHTS 4
#endif


float4x4 g_mWorldView;
float4x4 g_mProj;
texture  g_txCubeMap;
texture  g_txCubeMap2;
texture  g_txScene;
float4   g_vLightIntensity = { 15.0f, 15.0f, 15.0f, 1.0f };
float4   g_vLightPosView[MAX_NUM_LIGHTS];  // Light positions in view space
float    g_fReflectivity;                  // Reflectivity value


//-----------------------------------------------------------------------------
// Sampler: samCubeMap
// Desc: Process vertex for HDR environment mapping
//-----------------------------------------------------------------------------
samplerCUBE g_samCubeMap = 
sampler_state
{
    Texture = <g_txCubeMap>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
};


samplerCUBE g_samCubeMap2 = 
sampler_state
{
    Texture = <g_txCubeMap2>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
};


sampler2D g_samScene =
sampler_state
{
    Texture = <g_txScene>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
};


//-----------------------------------------------------------------------------
// Vertex Shader: HDRVertEnvMap
// Desc: Process vertex for HDR environment mapping
//-----------------------------------------------------------------------------
void HDRVertEnvMap( float4 Pos : POSITION,
                    float3 Normal : NORMAL,
                    out float4 oPos : POSITION,
                    out float3 EnvTex : TEXCOORD0 )
{
    oPos = mul( Pos, g_mWorldView );

    //
    // Compute normal in camera space
    //
    float3 vN = mul( Normal, g_mWorldView );
    vN = normalize( vN );

    //
    // Obtain the reverse eye vector
    //
    float3 vEyeR = -normalize( oPos );

    //
    // Compute the reflection vector
    //
    float3 vRef = 2 * dot( vEyeR, vN ) * vN - vEyeR;

    //
    // Store the reflection vector in texcoord0
    //
    EnvTex = vRef;

    //
    // Apply the projection
    //
    oPos = mul( oPos, g_mProj );
}


//-----------------------------------------------------------------------------
// Pixel Shader: HDRPixEnvMap
// Desc: Process pixel for HDR environment mapped object
//-----------------------------------------------------------------------------
float4 HDRPixEnvMap( float3 Tex : TEXCOORD0 ) : COLOR
{
    return g_fReflectivity * texCUBE( g_samCubeMap, Tex );
}


float4 HDRPixEnvMap2Tex( float3 Tex : TEXCOORD0 ) : COLOR
{
    return g_fReflectivity * float4( texCUBE( g_samCubeMap, Tex ).xy, texCUBE( g_samCubeMap2, Tex ).xy );
}


//-----------------------------------------------------------------------------
// Vertex Shader: HDRVertScene
// Desc: Process vertex for HDR-enabled scene
//-----------------------------------------------------------------------------
void HDRVertScene( float4 iPos : POSITION,
                   float3 iNormal : NORMAL,
                   float2 iTex : TEXCOORD0,
                   out float4 oPos : POSITION,
                   out float2 Tex : TEXCOORD0,
                   out float3 Pos : TEXCOORD1,
                   out float3 Normal : TEXCOORD2 )
{
    //
    // Transform position to view space
    //
    oPos = mul( iPos, g_mWorldView );

    //
    // Also write view position to texcoord1 to do per-pixel lighting
    //
    Pos = oPos;

    //
    // Transform to screen coord
    //
    oPos = mul( oPos, g_mProj );

    //
    // Transform normal and write to texcoord2 for per-pixel lighting
    //
    Normal = normalize( mul( iNormal, (float3x3)g_mWorldView ) );
    
    //
    // Propagate texture coord
    //
    Tex = iTex;
}


//-----------------------------------------------------------------------------
// Pixel Shader: HDRPixScene
// Desc: Process pixel (do per-pixel lighting) for HDR-enabled scene
//-----------------------------------------------------------------------------
float4 HDRPixScene( float2 Tex : TEXCOORD0,
                    float3 Pos : TEXCOORD1,
                    float3 Normal : TEXCOORD2 ) : COLOR
{
    float3 N = normalize( Normal );

    // Variable to save lit value by each light
    float4 vPixValue = (float4)0;

    //
    // Iterate through each light and apply the light on the pixel
    //
    for( int LightIndex = 0; LightIndex < MAX_NUM_LIGHTS; ++LightIndex )
    {
        //
        // Compute light vector (pixel to light)
        //
        float3 vRLightVec = (float3)(g_vLightPosView[LightIndex] - Pos);

        //
        // Find out the light intensity at the vertex based on
        // N dot L and distance from the light.
        //
        float fDiffuse = saturate( dot( normalize( vRLightVec ), N ) );

        //
        // Attenuation is 1 / D^2. Clamp at 1 to avoid infinity.
        //
        float fAttenuation = saturate( 1.0f / dot( vRLightVec, vRLightVec ) );

        //
        // Compute and add pixel color to final value
        //
        vPixValue += fDiffuse * fAttenuation;
    }

    return tex2D( g_samScene, Tex ) * vPixValue * g_vLightIntensity;
}


float4 HDRPixScene_FirstHalf( float2 Tex : TEXCOORD0,
                              float3 Pos : TEXCOORD1,
                              float3 Normal : TEXCOORD2 ) : COLOR
{
    return HDRPixScene( Tex, Pos, Normal ).xyzw;
}


float4 HDRPixScene_SecondHalf( float2 Tex : TEXCOORD0,
                               float3 Pos : TEXCOORD1,
                               float3 Normal : TEXCOORD2 ) : COLOR
{
    return HDRPixScene( Tex, Pos, Normal ).zwww;
}


//-----------------------------------------------------------------------------
// Vertex Shader: HDRVertLight
// Desc: Process vertex for light objects
//-----------------------------------------------------------------------------
void HDRVertLight( float4 iPos : POSITION,
                   out float4 oPos : POSITION,
                   out float4 Diffuse : TEXCOORD1 )
{
    //
    // Transform position to screen space
    //
    oPos = mul( iPos, g_mWorldView );
    oPos = mul( oPos, g_mProj );

    //
    // Diffuse color is the light intensity value
    //
    Diffuse = g_vLightIntensity;
}


//-----------------------------------------------------------------------------
// Pixel Shader: HDRPixLight
// Desc: Process pixel for HDR-enabled scene
//-----------------------------------------------------------------------------
float4 HDRPixLight( float4 Diffuse : TEXCOORD1 ) : COLOR
{
    //
    // Diffuse has the full intensity of the light.
    // Just output it.
    //
    return Diffuse;
}


float4 HDRPixLight_FirstHalf( float4 Diffuse : TEXCOORD1 ) : COLOR
{
    //
    // Diffuse has the full intensity of the light.
    // Just output it.
    //
    return Diffuse.xyww;
}


float4 HDRPixLight_SecondHalf( float4 Diffuse : TEXCOORD1 ) : COLOR
{
    //
    // Diffuse has the full intensity of the light.
    // Just output it.
    //
    return Diffuse.zwww;
}


//-----------------------------------------------------------------------------
// Technique: RenderScene
// Desc: Renders scene objects
//-----------------------------------------------------------------------------
technique RenderScene
{
    pass p0
    {
        VertexShader = compile vs_2_0 HDRVertScene();
        PixelShader = compile ps_2_0 HDRPixScene();
    }
}


technique RenderSceneFirstHalf
{
    pass p0
    {
        VertexShader = compile vs_2_0 HDRVertScene();
        PixelShader = compile ps_2_0 HDRPixScene_FirstHalf();
    }
}


technique RenderSceneSecondHalf
{
    pass p0
    {
        VertexShader = compile vs_2_0 HDRVertScene();
        PixelShader = compile ps_2_0 HDRPixScene_SecondHalf();
    }
}


//-----------------------------------------------------------------------------
// Technique: RenderLight
// Desc: Renders light objects
//-----------------------------------------------------------------------------
technique RenderLight
{
    pass p0
    {
        VertexShader = compile vs_2_0 HDRVertLight();
        PixelShader = compile ps_2_0 HDRPixLight();
    }
}


technique RenderLightFirstHalf
{
    pass p0
    {
        VertexShader = compile vs_2_0 HDRVertLight();
        PixelShader = compile ps_2_0 HDRPixLight_FirstHalf();
    }
}


technique RenderLightSecondHalf
{
    pass p0
    {
        VertexShader = compile vs_2_0 HDRVertLight();
        PixelShader = compile ps_2_0 HDRPixLight_SecondHalf();
    }
}


//-----------------------------------------------------------------------------
// Technique: RenderEnvMesh
// Desc: Renders the HDR environment-mapped mesh
//-----------------------------------------------------------------------------
technique RenderHDREnvMap
{
    pass p0
    {
        VertexShader = compile vs_2_0 HDRVertEnvMap();
        PixelShader = compile ps_2_0 HDRPixEnvMap();
    }
}


technique RenderHDREnvMap2Tex
{
    pass p0
    {
        VertexShader = compile vs_2_0 HDRVertEnvMap();
        PixelShader = compile ps_2_0 HDRPixEnvMap2Tex();
    }
}
