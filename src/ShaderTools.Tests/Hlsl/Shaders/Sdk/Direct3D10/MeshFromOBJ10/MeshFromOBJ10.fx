//--------------------------------------------------------------------------------------
// File: MeshFromOBJ10.fx
//
// Copyright (c) Microsoft Corporation. All rights reserved.
//--------------------------------------------------------------------------------------

//--------------------------------------------------------------------------------------
// Global variables
//--------------------------------------------------------------------------------------
float3 g_vMaterialAmbient   = float3( 0.2f, 0.2f, 0.2f );   // Material's ambient color
float3 g_vMaterialDiffuse   = float3( 0.8f, 0.8f, 0.8f );   // Material's diffuse color
float3 g_vMaterialSpecular  = float3( 1.0f, 1.0f, 1.0f );   // Material's specular color
float  g_fMaterialAlpha     = 1.0f;
int    g_nMaterialShininess = 32;

float3 g_vLightColor        = float3( 1.0f, 1.0f, 1.0f );     // Light color
float3 g_vLightPosition     = float3( 50.0f, 10.0f, 0.0f );   // Light position
float3 g_vCameraPosition ;

Texture2D g_MeshTexture;        // Color texture for mesh

float  g_fTime ;                // App's time in seconds
matrix g_mWorld ;               // World matrix
matrix g_mWorldViewProjection ; // World * View * Projection matrix

//--------------------------------------------------------------------------------------
// Texture sampler
//--------------------------------------------------------------------------------------
SamplerState samLinear
{
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = Wrap;
    AddressV = Wrap;
};

//--------------------------------------------------------------------------------------
// Vertex shader input structure
//--------------------------------------------------------------------------------------
struct VS_INPUT
{
    float3 vPosObject   : POSITION;
    float3 vNormalObject: NORMAL;
    float2 vTexUV       : TEXCOORD0;
};

//--------------------------------------------------------------------------------------
// Pixel shader input structure
//--------------------------------------------------------------------------------------
struct PS_INPUT
{
    float4 vPosProj : SV_POSITION;
    float4 vColor   : COLOR0;
    float2 vTexUV   : TEXCOORD0;
};


PS_INPUT VS( VS_INPUT input, uniform bool bSpecular )
{
    PS_INPUT output;
    
    // Transform the position into world space for lighting, and projected space
    // for display
    float4 vPosWorld = mul( float4(input.vPosObject,1), g_mWorld );
    output.vPosProj = mul( float4(input.vPosObject,1), g_mWorldViewProjection );

    // Transform the normal into world space for lighting
    float3 vNormalWorld = mul( input.vNormalObject, (float3x3)g_mWorld );

    // Pass the texture coordinate
    output.vTexUV = input.vTexUV;

    // Compute the light vector
    float3 vLight = normalize( g_vLightPosition - vPosWorld.xyz );

    // Compute the ambient and diffuse components of illumination
    output.vColor.rgb = g_vLightColor * g_vMaterialAmbient;
    output.vColor.rgb += g_vLightColor * g_vMaterialDiffuse * saturate( dot( vLight, vNormalWorld ) );

    // If enabled, calculate the specular term
    if ( bSpecular )
    {
        float3 vCamera = normalize( vPosWorld.xyz - g_vCameraPosition );
        float3 vReflection = reflect( vLight, vNormalWorld );
        float  fPhoneValue = saturate( dot(vReflection, vCamera) );

        output.vColor.rgb += g_vMaterialSpecular * pow( fPhoneValue, g_nMaterialShininess );
    }

    output.vColor.a = g_fMaterialAlpha;

    return output;
}

float4 PS( PS_INPUT input, uniform bool bTexture ) : SV_Target
{
    float4 output = input.vColor;

    // Sample and modulate the texture
    if ( bTexture )
    {
        output.rgb *= g_MeshTexture.Sample( samLinear, input.vTexUV );
    }

    return output;
}

//--------------------------------------------------------------------------------------
// Techniques
//--------------------------------------------------------------------------------------
technique10 NoSpecular
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, VS(false) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(false) ) );
    }
}
technique10 Specular
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, VS(true) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(false) ) );
    }
}
technique10 TexturedNoSpecular
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, VS(false) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(true) ) );
    }
}
technique10 TexturedSpecular
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, VS(true) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(true) ) );
    }
}

