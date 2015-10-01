//--------------------------------------------------------------------------------------
//
// Alpha-Tested Pine Lighting Model
// Copyright (c) Microsoft Corporation. All rights reserved.
//
//--------------------------------------------------------------------------------------


//--------------------------------------------------------------------------------------
// Effect Edit defaults
//--------------------------------------------------------------------------------------

string XFile = "pine04.x";   // model
int    BCLR = 0xff202080;    // background
string BIMG  = "lake.bmp";   // Background image


//--------------------------------------------------------------------------------------
// Scene Setup
//--------------------------------------------------------------------------------------

// light direction (world space)
float3 lightDir = {0.577, -0.577, 0.577};

// light intensity
float4 I_a = { 0.5f, 0.5f, 0.5f, 1.0f };    // ambient
float4 I_d = { 0.5f, 0.5f, 0.5f, 1.0f };    // diffuse
float4 I_s = { 1.0f, 1.0f, 1.0f, 1.0f };    // specular

// Transformation Matrices
matrix matWorld     : WORLD;
matrix matViewProj  : VIEWPROJECTION;


//--------------------------------------------------------------------------------------
// Material Properties
//--------------------------------------------------------------------------------------

// Set by EffectInstance when mesh is loaded
// (Default values provided for Effect Edit)
float4 Diffuse = float4( 1.f, 1.f, 1.f, 1.f );
float4 Ambient = float4( 1.f, 1.f, 1.f, 1.f );

// Texture Parameter, annotation specifies default texture for EffectEdit
texture Texture0 < string name = "pine04.dds"; >;

// Sampler, for sampling the pine texture
sampler s0 = sampler_state
{
    texture = <Texture0>;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    magfilter  = LINEAR;
};

//--------------------------------------------------------------------------------------
// Vertex Shader
//--------------------------------------------------------------------------------------
void VS( in  float3 pos      : POSITION,
         in  float3 norm     : NORMAL,
         in  float2 texcrds  : TEXCOORD,
         out float4 opos     : POSITION,
         out float4 ocolor   : COLOR0,
         out float2 otexcrds : TEXCOORD )
{
    // Transform the vertex to clip space
    opos = mul( float4(pos, 1.f), mul( matWorld, matViewProj ) );

    // Calculate the normal (in world-space)
    // Assumes inverse( transpose( matWorld ) ) == matWorld
    float3 norm_w = normalize( mul( norm, (float3x3)matWorld ) );
    
    // Calculate the diffuse lighting coefficient
    // The Pine Primitives are 2-Sided, and may be illuminated from either direction
    float dotResult = abs( dot( -lightDir, norm_w ) );
    
    // Simple Diffuse/Ambient Lighting
    ocolor = saturate( dotResult * Diffuse * I_d + Ambient * I_a );

	// Pass the texture coordinates to the pixel shader
    otexcrds = texcrds;
}

//--------------------------------------------------------------------------------------
// Pixel Shader
//--------------------------------------------------------------------------------------
void PS( in  float4 color   : COLOR,
         in  float2 texcord : TEXCOORD,
         out float4 ocolor  : COLOR )
{
    // Sample the Texture
    float4 tex_color = tex2D( s0, texcord );
    
    // Modulate the texture color by the vertex lighting
    ocolor = tex_color * color;
}


//--------------------------------------------------------------------------------------
// Default Technique
// Establishes Vertex and Pixel Shader
// Ensures base states are set to required values
// (Other techniques within the scene perturb these states)
//--------------------------------------------------------------------------------------
technique tec0
{
    pass p0
    {
        VertexShader = compile vs_2_0 VS();
        PixelShader  = compile ps_2_0 PS();

        ZEnable          = TRUE;
        ZWriteEnable     = TRUE;
        CullMode         = NONE;
        AlphaBlendEnable = FALSE;
        AlphaTestEnable  = TRUE;
        AlphaRef         = 156;
        AlphaFunc        = GREATER;
    }
}
