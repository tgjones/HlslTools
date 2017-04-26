//--------------------------------------------------------------------------------------
//
// Skybox Lighting Model
// Copyright (c) Microsoft Corporation. All rights reserved.
//
//--------------------------------------------------------------------------------------

//--------------------------------------------------------------------------------------
// Effect Edit defaults
//--------------------------------------------------------------------------------------

string XFile = "skybox01.x";     // model
int    BCLR = 0xff202080;        // background


//--------------------------------------------------------------------------------------
// Scene Setup
//--------------------------------------------------------------------------------------

// There is no lighting information,
// as the skybox texture is pre-lit

// matrices
matrix matView  : VIEW;
matrix matProj  : PROJECTION;


//--------------------------------------------------------------------------------------
// Material Properties
//--------------------------------------------------------------------------------------

// Texture Parameter, annotation specifies default texture for EffectEdit
texture Texture0 <  string type = "CUBE"; string name = "skybox02.dds"; >;

// Sampler, for sampling the skybox texture
sampler linear_sampler = sampler_state
{
    Texture   = (Texture0);
    MipFilter = LINEAR;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    ADDRESSU = Clamp;
    ADDRESSV = Clamp;
};


//--------------------------------------------------------------------------------------
// Vertex Shader
//--------------------------------------------------------------------------------------
void VS ( in  float3 v0   : POSITION,
          out float4 oPos : POSITION,
          out float3 oT0  : TEXCOORD0 )
{
    // Strip any translation off of the view matrix
    // Use only rotations & the projection matrix
    float4x4 matViewNoTrans =
    {
        matView[0],
        matView[1],
        matView[2],
        float4( 0.f, 0.f, 0.f, 1.f )
    };

    // Output the position
    oPos = mul( float4(v0,1.f), mul( matViewNoTrans, matProj ) );
    
    // Calculate the cube map texture coordinates
    // Because this is a cube-map, the 3-D texture coordinates are calculated
    // from the world-position of the skybox vertex.
    // v0 (from the skybox mesh) is considered to be pre-transformed into world space
    oT0 = v0;
}




//--------------------------------------------------------------------------------------
// Pixel Shader
//--------------------------------------------------------------------------------------
void PS( in  float3 t0 : TEXCOORD0,
         out float4 r0 : COLOR0 )
{
    // The skybox texture is pre-lit, so simply output the texture color
    r0 = texCUBE( linear_sampler, t0 );
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
        VertexShader = compile vs_2_0 VS();
        PixelShader  = compile ps_2_0 PS();
        
        ZEnable = FALSE;
        ZWriteEnable = FALSE;
        AlphaBlendEnable = FALSE;
        CullMode = CCW;
        AlphaTestEnable = FALSE;
    }
}

