//-----------------------------------------------------------------------------
// File: ShadowMap.fx
//
// Desc: Effect file for high dynamic range cube mapping sample.
//
// Copyright (c) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------


#define SMAP_SIZE 512


#define SHADOW_EPSILON 0.00005f


float4x4 g_mWorldView;
float4x4 g_mProj;
float4x4 g_mViewToLightProj;  // Transform from view space to light projection space
float4   g_vMaterial;
texture  g_txScene;
texture  g_txShadow;
float3   g_vLightPos;  // Light position in view space
float3   g_vLightDir;  // Light direction in view space
float4   g_vLightDiffuse = float4( 1.0f, 1.0f, 1.0f, 1.0f );  // Light diffuse color
float4   g_vLightAmbient = float4( 0.3f, 0.3f, 0.3f, 1.0f );  // Use an ambient light of 0.3
float    g_fCosTheta;  // Cosine of theta of the spot light



sampler2D g_samScene =
sampler_state
{
    Texture = <g_txScene>;
    MinFilter = Point;
    MagFilter = Linear;
    MipFilter = Linear;
};




sampler2D g_samShadow =
sampler_state
{
    Texture = <g_txShadow>;
    MinFilter = Point;
    MagFilter = Point;
    MipFilter = Point;
    AddressU = Clamp;
    AddressV = Clamp;
};




//-----------------------------------------------------------------------------
// Vertex Shader: VertScene
// Desc: Process vertex for scene
//-----------------------------------------------------------------------------
void VertScene( float4 iPos : POSITION,
                float3 iNormal : NORMAL,
                float2 iTex : TEXCOORD0,
                out float4 oPos : POSITION,
                out float2 Tex : TEXCOORD0,
                out float4 vPos : TEXCOORD1,
                out float3 vNormal : TEXCOORD2,
                out float4 vPosLight : TEXCOORD3 )
{
    //
    // Transform position to view space
    //
    vPos = mul( iPos, g_mWorldView );

    //
    // Transform to screen coord
    //
    oPos = mul( vPos, g_mProj );

    //
    // Compute view space normal
    //
    vNormal = mul( iNormal, (float3x3)g_mWorldView );

    //
    // Propagate texture coord
    //
    Tex = iTex;

    //
    // Transform the position to light projection space, or the
    // projection space as if the camera is looking out from
    // the spotlight.
    //
    vPosLight = mul( vPos, g_mViewToLightProj );
}




//-----------------------------------------------------------------------------
// Pixel Shader: PixScene
// Desc: Process pixel (do per-pixel lighting) for enabled scene
//-----------------------------------------------------------------------------
float4 PixScene( float2 Tex : TEXCOORD0,
                 float4 vPos : TEXCOORD1,
                 float3 vNormal : TEXCOORD2,
                 float4 vPosLight : TEXCOORD3 ) : COLOR
{
    float4 Diffuse;

    // vLight is the unit vector from the light to this pixel
    float3 vLight = normalize( float3( vPos - g_vLightPos ) );

    // Compute diffuse from the light
    if( dot( vLight, g_vLightDir ) > g_fCosTheta ) // Light must face the pixel (within Theta)
    {
        // Pixel is in lit area. Find out if it's
        // in shadow using 2x2 percentage closest filtering

        //transform from RT space to texture space.
        float2 ShadowTexC = 0.5 * vPosLight.xy / vPosLight.w + float2( 0.5, 0.5 );
        ShadowTexC.y = 1.0f - ShadowTexC.y;

        // transform to texel space
        float2 texelpos = SMAP_SIZE * ShadowTexC;
        
        // Determine the lerp amounts           
        float2 lerps = frac( texelpos );

        //read in bilerp stamp, doing the shadow checks
        float sourcevals[4];
        sourcevals[0] = (tex2D( g_samShadow, ShadowTexC ) + SHADOW_EPSILON < vPosLight.z / vPosLight.w)? 0.0f: 1.0f;  
        sourcevals[1] = (tex2D( g_samShadow, ShadowTexC + float2(1.0/SMAP_SIZE, 0) ) + SHADOW_EPSILON < vPosLight.z / vPosLight.w)? 0.0f: 1.0f;  
        sourcevals[2] = (tex2D( g_samShadow, ShadowTexC + float2(0, 1.0/SMAP_SIZE) ) + SHADOW_EPSILON < vPosLight.z / vPosLight.w)? 0.0f: 1.0f;  
        sourcevals[3] = (tex2D( g_samShadow, ShadowTexC + float2(1.0/SMAP_SIZE, 1.0/SMAP_SIZE) ) + SHADOW_EPSILON < vPosLight.z / vPosLight.w)? 0.0f: 1.0f;  
        
        // lerp between the shadow values to calculate our light amount
        float LightAmount = lerp( lerp( sourcevals[0], sourcevals[1], lerps.x ),
                                  lerp( sourcevals[2], sourcevals[3], lerps.x ),
                                  lerps.y );
        // Light it
        Diffuse = ( saturate( dot( -vLight, normalize( vNormal ) ) ) * LightAmount * ( 1 - g_vLightAmbient ) + g_vLightAmbient )
                  * g_vMaterial;
    } else
    {
        Diffuse = g_vLightAmbient * g_vMaterial;
    }

    return tex2D( g_samScene, Tex ) * Diffuse;
}




//-----------------------------------------------------------------------------
// Vertex Shader: VertLight
// Desc: Process vertex for the light object
//-----------------------------------------------------------------------------
void VertLight( float4 iPos : POSITION,
                float3 iNormal : NORMAL,
                float2 iTex : TEXCOORD0,
                out float4 oPos : POSITION,
                out float2 Tex : TEXCOORD0 )
{
    //
    // Transform position to view space
    //
    oPos = mul( iPos, g_mWorldView );

    //
    // Transform to screen coord
    //
    oPos = mul( oPos, g_mProj );

    //
    // Propagate texture coord
    //
    Tex = iTex;
}




//-----------------------------------------------------------------------------
// Pixel Shader: PixLight
// Desc: Process pixel for the light object
//-----------------------------------------------------------------------------
float4 PixLight( float2 Tex : TEXCOORD0,
                 float4 vPos : TEXCOORD1 ) : COLOR
{
    return tex2D( g_samScene, Tex );
}




//-----------------------------------------------------------------------------
// Vertex Shader: VertShadow
// Desc: Process vertex for the shadow map
//-----------------------------------------------------------------------------
void VertShadow( float4 Pos : POSITION,
                 float3 Normal : NORMAL,
                 out float4 oPos : POSITION,
                 out float2 Depth : TEXCOORD0 )
{
    //
    // Compute the projected coordinates
    //
    oPos = mul( Pos, g_mWorldView );
    oPos = mul( oPos, g_mProj );

    //
    // Store z and w in our spare texcoord
    //
    Depth.xy = oPos.zw;
}




//-----------------------------------------------------------------------------
// Pixel Shader: PixShadow
// Desc: Process pixel for the shadow map
//-----------------------------------------------------------------------------
void PixShadow( float2 Depth : TEXCOORD0,
                out float4 Color : COLOR )
{
    //
    // Depth is z / w
    //
    Color = Depth.x / Depth.y;
}




//-----------------------------------------------------------------------------
// Technique: RenderScene
// Desc: Renders scene objects
//-----------------------------------------------------------------------------
technique RenderScene
{
    pass p0
    {
        VertexShader = compile vs_2_0 VertScene();
        PixelShader = compile ps_2_0 PixScene();
    }
}




//-----------------------------------------------------------------------------
// Technique: RenderLight
// Desc: Renders the light object
//-----------------------------------------------------------------------------
technique RenderLight
{
    pass p0
    {
        VertexShader = compile vs_2_0 VertLight();
        PixelShader = compile ps_2_0 PixLight();
    }
}




//-----------------------------------------------------------------------------
// Technique: RenderShadow
// Desc: Renders the shadow map
//-----------------------------------------------------------------------------
technique RenderShadow
{
    pass p0
    {
        VertexShader = compile vs_2_0 VertShadow();
        PixelShader = compile ps_2_0 PixShadow();
    }
}
