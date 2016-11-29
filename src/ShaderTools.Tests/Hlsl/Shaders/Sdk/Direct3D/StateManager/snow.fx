//--------------------------------------------------------------------------------------
//
// Snow Lighting Model
// Copyright (c) Microsoft Corporation. All rights reserved.
//
//--------------------------------------------------------------------------------------


//--------------------------------------------------------------------------------------
// Scene Setup
//--------------------------------------------------------------------------------------

// light direction (world space)
float3 lightDir = {0.577, -0.577, 0.577};

// light intensity
float4 I_a = { 0.5f, 0.5f, 0.5f, 1.0f };    // ambient
float4 I_d = { 0.6f, 0.6f, 0.6f, 1.0f };    // diffuse
float4 I_s = { 1.0f, 1.0f, 1.0f, 1.0f };    // specular


// Transformation Matrices
matrix matWorld : WORLD;
matrix matViewProj  : VIEWPROJECTION;
matrix matViewInv   : VIEWINV;              // VIEWINV not supported by Effect Edit

texture Texture0 <  string type = "CUBE"; string name = "ripples.dds"; >;

//--------------------------------------------------------------------------------------
// Material Properties
//--------------------------------------------------------------------------------------

// Set by EffectInstance when mesh is loaded
// (Default values provided for Effect Edit)
float4 Diffuse = float4( 0.95f, 0.95f, 1.f, 1.f );
float4 Ambient = float4( 0.95f, 0.95f, 1.f, 1.f );
float4 Specular = float4( 0.2f, 0.2f,  0.2f, 1.f );


//--------------------------------------------------------------------------------------
// Vertex Shader
//--------------------------------------------------------------------------------------
void VS( in  float3 pos      : POSITION,
         in  float3 norm     : NORMAL,
         in  float2 iT0      : TEXCOORD0,
         in  float3 Tangent  : TANGENT0,
         out float4 oPos     : POSITION,
         out float2 oT0      : TEXCOORD0,
         out float3 oToLight : TEXCOORD1,
         out float3 oHalf    : TEXCOORD2 )
{
    // Transform the vertex to clip space
    float4 Pos_w = mul( float4(pos,1), matWorld );
    oPos = mul( Pos_w, matViewProj );

    // Calculate the tangent vectors in world space
    float3 Normal_w = mul( norm, (float3x3)matWorld );
    float3 Tangent_w = mul( Tangent, (float3x3)matWorld );
    float3 Binormal_w = cross( Tangent_w, Normal_w );

    // Calculate the tangent matrix
    float3x3 matTSpace = transpose(float3x3( Tangent_w, Binormal_w, Normal_w  ));

    // The Eye Position in World space is the last row of the 
    // full-inverse of the view transform
    float3 EyePos_w = matViewInv[3];
    float3 ToEye_w = normalize( EyePos_w - (float3)Pos_w );
    
    // Half Vector for the sparkly refections
    // The sparkles are computed by masking per-pixel specular with a specular mask
    // in the texture alpha channel.
    float3 Half_w = normalize( ToEye_w - lightDir );

    // Take the light and half-vector into tangent space
    float3 Half_t = mul( Half_w, matTSpace );
    float3 Light_t = mul( -lightDir, matTSpace );

    // Pack -1,1 into 0,1
    oHalf = 0.5f * Half_t + 0.5f;
    oToLight = 0.5f * Light_t + 0.5f;

    // pass TexCoords through
    oT0 = iT0;
}


sampler normal_sampler = sampler_state
{
    Texture = (Texture0);
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    MipFilter = LINEAR;
};

//--------------------------------------------------------------------------------------
// Pixel Shader
//--------------------------------------------------------------------------------------
float4 PS( in  float2 iT0      : TEXCOORD0,
           in  float3 iToLight : TEXCOORD1,
           in  float3 iHalf    : TEXCOORD2 ) : COLOR0
{
	// Read the normal direction from the texture
	float4 NormalMap = tex2D( normal_sampler, iT0 );
    
    // Unpack the Normal, Light, and Half-Vector from 0,1 to -1,1
    float3 Normal  = NormalMap * 2 - 1;
    float3 ToLight = iToLight * 2 - 1;
    float3 Half = iHalf * 2 - 1;

    // Calculate the diffuse coefficient
    float NdotL = dot( Normal.rgb, ToLight);
    
    // Calcuate the specular coefficient
    float NdotH = dot( Normal.rgb, Half );
    NdotH *= NdotH;
    NdotH *= NdotH;
    NdotH *= NdotH;

	// Modulate the Diffuse and Specular colors by the material properties
	float3 DiffuseC = Diffuse* NdotL * I_d + I_a * Ambient;
	float3 SpecularC = (float3)Specular * NdotH * NormalMap.a;

	// Return the final color    
    return float4( DiffuseC+SpecularC, 1.f);
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
        AlphaBlendEnable = FALSE;
        CullMode         = CCW;
        AlphaTestEnable  = FALSE;
    }
}
