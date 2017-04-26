//--------------------------------------------------------------------------------------
//
// Reflective Lighting Model
// Copyright (c) Microsoft Corporation. All rights reserved.
//
//--------------------------------------------------------------------------------------


//--------------------------------------------------------------------------------------
// Scene Setup
//--------------------------------------------------------------------------------------

// light direction (world space)
float3 lightDir = {0.577, -0.577, -0.577};


// light intensity
float4 I_a = { 0.5f, 0.5f, 0.5f, 1.0f };    // ambient
float4 I_d = { 0.5f, 0.5f, 0.5f, 1.0f };    // diffuse
float4 I_s = { 1.0f, 1.0f, 1.0f, 1.0f };    // specular

// Transformation Matrices
matrix matView      : VIEW;
matrix matProj      : PROJECTION;
matrix matWorld     : WORLD;

//--------------------------------------------------------------------------------------
// Material Properties
//--------------------------------------------------------------------------------------

// Set by EffectInstance when mesh is loaded
// (Default values provided for Effect Edit)
float4 Diffuse = float4( 0.358824f, 0.311765f, 0.059804f, 1.f );
float4 Ambient = float4( 0.358824f, 0.311765f, 0.059804f, 1.f );
float4 Specular = float4( 0.9f, 0.9f, 0.9f, 0.f );
float4 Emissive = float4( 0.f, 0.f, 0.f, 0.f );
float  Power = 32.f;
float  k_r = 0.20f;
	

// Texture Parameters
texture Texture0 < string name = "signs2.jpg"; >;
texture Texture1 < string type = "CUBE"; string name = "skybox02.dds"; >;


sampler linear_sampler = sampler_state
{
    Texture   = (Texture0);
    MipFilter = LINEAR;
    MinFilter = ANISOTROPIC;
    MagFilter = LINEAR;
};

sampler envmap_sampler = sampler_state
{
    Texture   = (Texture1);
    MipFilter = LINEAR;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
};


//--------------------------------------------------------------------------------------
// Vertex Shader Output
//--------------------------------------------------------------------------------------
struct VS_OUTPUT
{
    float4 Pos  : POSITION;
    float3 Diff : COLOR0;
    float3 Spec : COLOR1;
    float2 T0   : TEXCOORD0;
    float3 T1   : TEXCOORD1;
    float3 Reflect : TEXCOORD2;
};


//--------------------------------------------------------------------------------------
// Vertex Shader
//--------------------------------------------------------------------------------------
VS_OUTPUT VS(
    float3 Pos  : POSITION, 
    float3 Norm : NORMAL,
    float2 iT0  : TEXCOORD0 )
{
    VS_OUTPUT Out = (VS_OUTPUT)0;

    matrix matWorldView = mul( matWorld, matView );

    float3 L = -normalize(mul(lightDir,(float3x3)matView));

    float3 P = mul(float4(Pos, 1), (float4x3)matWorldView);      // position (view space)
    float3 N = normalize(mul(Norm, (float3x3)matWorldView));     // normal (view space)

    float3 R = normalize(2 * dot(N, L) * N - L);             // Reflection vector (view space)
    float3 V = -normalize(P);                                // view direction (view space)
    float3 G = normalize(2 * dot(N, -P) * N + P);            // Glance vector (view space)
    float  f = 0.5 - dot(V, N); f = 1 - 4 * f * f;           // fresnel term
  

    Out.Pos   = mul(float4(P, 1), matProj);                          // position (projected)
    Out.Diff  = I_a * Ambient + I_d * Diffuse * max(0, dot(N, L));   // diffuse + ambient
    Out.Spec  = I_s * Specular * pow(max(0, dot(R, V)), Power/4);    // specular
    Out.T0    = iT0;                                                 // Diffuse Tex coords
    Out.T1    = mul( G, transpose( matView ) );                      // Glance Vector to View Space
    Out.Reflect = max( 0, k_r * f);                                  // Reflection
    
    return Out;
}


//--------------------------------------------------------------------------------------
// Pixel Shader
//--------------------------------------------------------------------------------------
float4 PS(
    float3 Diff : COLOR0,
    float3 Spec : COLOR1,
    float2 Tex  : TEXCOORD0,
    float3 EnvCoords : TEXCOORD1,
    float  Reflect   : TEXCOORD2 ) : COLOR
{
    // Vertex Diffuse Lighting is modulated with the Diffuse Texture
    float3 DiffuseC = Diff * tex2D( linear_sampler, Tex );
    
    // Sample the Environment map
    float3 ReflectionC = Reflect * texCUBE( envmap_sampler, EnvCoords );
    
    // Return the sum of Vertex-Specular, the Reflection, 
    return float4( DiffuseC + ReflectionC + Spec, 1.f );
}


//--------------------------------------------------------------------------------------
// Default Technique
// Establishes Vertex and Pixel Shader
// Ensures base states are set to required values
// (Other techniques within the scene perturb these states)
//--------------------------------------------------------------------------------------
technique tec0
{
    pass P0
    {
        // shaders
        VertexShader = compile vs_2_0 VS();
        PixelShader  = compile ps_2_0 PS();

        ZEnable          = TRUE;
        ZWriteEnable     = TRUE;
        AlphaBlendEnable = FALSE;
        CullMode         = CCW;
        AlphaTestEnable  = FALSE;
    }  
}
