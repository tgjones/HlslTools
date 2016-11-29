//--------------------------------------------------------------------------------------
// File: MeshFromOBJ.fx
//
// The effect file for the MeshFromOBJ sample.  
// 
// Copyright (c) Microsoft Corporation. All rights reserved.
//--------------------------------------------------------------------------------------


//--------------------------------------------------------------------------------------
// Global variables
//--------------------------------------------------------------------------------------
float3 g_vMaterialAmbient : Ambient = float3( 0.2f, 0.2f, 0.2f );   // Material's ambient color
float3 g_vMaterialDiffuse : Diffuse = float3( 0.8f, 0.8f, 0.8f );   // Material's diffuse color
float3 g_vMaterialSpecular : Specular = float3( 1.0f, 1.0f, 1.0f );  // Material's specular color
float  g_fMaterialAlpha : Opacity = 1.0f;
int    g_nMaterialShininess : SpecularPower = 32;

float3 g_vLightColor : LightColor = float3( 1.0f, 1.0f, 1.0f );        // Light color
float3 g_vLightPosition : LightPosition = float3( 50.0f, 10.0f, 0.0f );   // Light position
float3 g_vCameraPosition : CameraPosition;

texture  g_MeshTexture : Texture;   // Color texture for mesh
            
float    g_fTime : Time;            // App's time in seconds
float4x4 g_mWorld : World;          // World matrix
float4x4 g_mWorldViewProjection : WorldViewProjection; // World * View * Projection matrix



//--------------------------------------------------------------------------------------
// Texture samplers
//--------------------------------------------------------------------------------------
sampler MeshTextureSampler = 
sampler_state
{
    Texture = <g_MeshTexture>;    
    MipFilter = LINEAR;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
};


//--------------------------------------------------------------------------------------
// Name: Projection
// Type: Vertex Shader Fragment
// Desc: Projection transform
//--------------------------------------------------------------------------------------
void Projection( float4 vPosObject: POSITION,
                 float3 vNormalObject: NORMAL,
                 float2 vTexCoordIn: TEXCOORD0,
                 out float4 vPosProj: POSITION,
                 out float2 vTexCoordOut: TEXCOORD0,
                 out float4 vColorOut: COLOR0,
                 uniform bool bSpecular
                )
{
    // Transform the position into world space for lighting, and projected space
    // for display
    float4 vPosWorld = mul( vPosObject, g_mWorld );
    vPosProj = mul( vPosObject, g_mWorldViewProjection );
    
    // Transform the normal into world space for lighting
    float3 vNormalWorld = mul( vNormalObject, (float3x3)g_mWorld );
    
    // Pass the texture coordinate
    vTexCoordOut = vTexCoordIn;
    
    // Compute the light vector
    float3 vLight = normalize( g_vLightPosition - vPosWorld.xyz );
    
    // Compute the ambient and diffuse components of illumination
    vColorOut.rgb = g_vLightColor * g_vMaterialAmbient;
    vColorOut.rgb += g_vLightColor * g_vMaterialDiffuse * saturate( dot( vLight, vNormalWorld ) );
    
    // If enabled, calculate the specular term
    if( bSpecular )
    {
        float3 vCamera = normalize(vPosWorld.xyz - g_vCameraPosition);
        float3 vReflection = reflect( vLight, vNormalWorld );
        float  fPhongValue = saturate( dot( vReflection, vCamera ) );

        vColorOut.rgb += g_vMaterialSpecular * pow(fPhongValue, g_nMaterialShininess);
    }
        
    vColorOut.a = g_fMaterialAlpha;
}



//--------------------------------------------------------------------------------------
// Name: Lighting
// Type: Pixel Shader
// Desc: Compute lighting and modulate the texture
//--------------------------------------------------------------------------------------
void Lighting( float2 vTexCoord: TEXCOORD0,
               float4 vColorIn: COLOR0,
               out float4 vColorOut: COLOR0,
               uniform bool bTexture )
{  
    vColorOut = vColorIn;
    
    // Sample and modulate the texture
    if( bTexture )
        vColorOut.rgb *= tex2D( MeshTextureSampler, vTexCoord );
}


//--------------------------------------------------------------------------------------
// Techniques
//--------------------------------------------------------------------------------------
technique Specular
{
    pass P0
    {
        VertexShader = compile vs_2_0 Projection(true);    
        PixelShader = compile ps_2_0 Lighting(false);    
    }
}

technique NoSpecular
{
    pass P0
    {
        VertexShader = compile vs_2_0 Projection(false);    
        PixelShader = compile ps_2_0 Lighting(false);    
    }
}

technique TexturedSpecular
{
    pass P0
    {
        VertexShader = compile vs_2_0 Projection(true);    
        PixelShader = compile ps_2_0 Lighting(true);    
    }
}

technique TexturedNoSpecular
{
    pass P0
    {
        VertexShader = compile vs_2_0 Projection(false);    
        PixelShader = compile ps_2_0 Lighting(true);    
    }
}