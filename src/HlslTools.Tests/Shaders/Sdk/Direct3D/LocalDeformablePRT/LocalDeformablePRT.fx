//--------------------------------------------------------------------------------------
// File: LocalDeformablePRT.fx
//
// The effect file for the LocalDeformablePRT sample.  
// 
// Copyright (c) Microsoft Corporation. All rights reserved.
//--------------------------------------------------------------------------------------

//--------------------------------------------------------------------------------------
// Global variables
//--------------------------------------------------------------------------------------
float    g_fTime;                       // App's time in seconds
float4x4 g_mViewProjection;             // View * Projection matrix
float3   g_vLightDirection;             // Directional light
float    g_fLightIntensity;             // Intensity
float4   g_vLightCoeffsR[4];            // SH lighting coefficients
float4   g_vLightCoeffsG[4];            // SH lighting coefficients
float4   g_vLightCoeffsB[4];            // SH lighting coefficients
float3   g_vColorTransmit;              // Per-channel transmission ratio

// Matrix Pallette
int g_NumBones = 1;
static const int MAX_MATRICES = 26;
float4x3 g_mWorldMatrixArray[MAX_MATRICES];

texture Albedo; 
texture NormalMap;

textureCUBE YlmCoeff0;
textureCUBE YlmCoeff4;
textureCUBE YlmCoeff8;
textureCUBE YlmCoeff12;

sampler AlbedoSampler = sampler_state {
    Texture = (Albedo);
    MipFilter = LINEAR;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    ADDRESSU = WRAP;
    ADDRESSV = WRAP;
};

sampler NormalMapSampler = sampler_state {
    Texture = (NormalMap);
    MipFilter = NONE;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    ADDRESSU = WRAP;
    ADDRESSV = WRAP;
};

sampler YlmCoeff0Sampler = sampler_state {
    Texture = (YlmCoeff0);
    MipFilter = LINEAR;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    ADDRESSU = CLAMP;
    ADDRESSV = CLAMP;
};

sampler YlmCoeff4Sampler = sampler_state {
    Texture = (YlmCoeff4);
    MipFilter = LINEAR;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    ADDRESSU = CLAMP;
    ADDRESSV = CLAMP;
};

sampler YlmCoeff8Sampler = sampler_state {
    Texture = (YlmCoeff8);
    MipFilter = LINEAR;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    ADDRESSU = CLAMP;
    ADDRESSV = CLAMP;
};

sampler YlmCoeff12Sampler = sampler_state {
    Texture = (YlmCoeff12);
    MipFilter = LINEAR;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    ADDRESSU = CLAMP;
    ADDRESSV = CLAMP;
};


//--------------------------------------------------------------------------------------
// The Local Deformable PRT coefficients are a zonal harmonic
// approximation of a spherical harmonic PRT transfer vector
// representing the light transport of the scene including
// self-shadowing, interreflections and subsurface scattering.
// 
// They are the solution of a least squares optimization w.r.t to a
// normal vector.  Here we construct a
// spherical harmonic transfer vector by scaling the spherical
// harmonics coefficients in the normal direction with these
// coefficients.  
//--------------------------------------------------------------------------------------
void BuildSHTransferVector(float4 vLDPRTCoeffs, float4 vSHCoeffs[4], out float4 vTransferVector[4]) 
{
    vTransferVector[0] = float4(vLDPRTCoeffs.x, vLDPRTCoeffs.y, vLDPRTCoeffs.y, vLDPRTCoeffs.y)*vSHCoeffs[0];
    vTransferVector[1] = float4(vLDPRTCoeffs.z, vLDPRTCoeffs.z, vLDPRTCoeffs.z, vLDPRTCoeffs.z)*vSHCoeffs[1];
    vTransferVector[2] = float4(vLDPRTCoeffs.z, vLDPRTCoeffs.w, vLDPRTCoeffs.w, vLDPRTCoeffs.w)*vSHCoeffs[2];
    vTransferVector[3] = float4(vLDPRTCoeffs.w, vLDPRTCoeffs.w, vLDPRTCoeffs.w, vLDPRTCoeffs.w)*vSHCoeffs[3];
} 


//--------------------------------------------------------------------------------------
// Computes the cubic spherical harmonic (aka Ylm(theta,phi)) coefficients in the
// direction vSHCoeffDir
//--------------------------------------------------------------------------------------
void Ylm(float3 vSHCoeffDir, out float4 vYlm[4]) 
{  
    vYlm[0] = texCUBE(YlmCoeff0Sampler,  vSHCoeffDir);
    vYlm[1] = texCUBE(YlmCoeff4Sampler,  vSHCoeffDir);
    vYlm[2] = texCUBE(YlmCoeff8Sampler,  vSHCoeffDir);
    vYlm[3] = texCUBE(YlmCoeff12Sampler, vSHCoeffDir);  
} 


//--------------------------------------------------------------------------------------
// 1 channel LDPRT, 3 channel lighting 
//--------------------------------------------------------------------------------------
float4 LDPRTCoeffLighting(float2 vTexCoord, float3 vSHCoeffDir) 
{
    float4 vExitRadiance[3] = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    float4 vTransferVector[4], vYlm[4];
    Ylm(vSHCoeffDir, vYlm);

    // A 4 channel texture can hold a cubics worth of LDPRT coefficients.
    float4 vLDPRTCoeffs = { 1,  2.0/3.0, 0.25, 0};
    BuildSHTransferVector(vLDPRTCoeffs, vYlm, vTransferVector);
    
    // Calculate sub-surface contribution
    
    // Negating the odd-order coefficients will mirror the lobe across the tangent plane
    vLDPRTCoeffs.y *= -1; 
    float4 vTransferVectorBehind[4];
    BuildSHTransferVector(vLDPRTCoeffs, vYlm, vTransferVectorBehind);
    
    // The alpha channel of the albedo texture is being used to store how 'thin'
    // the material is, where higher values allow more light to transmit.
    // Although each color channel could have a separate thickness texture, we're
    // simply scaling the amount of transmitted light by a channel scalar. 
    float4 vAlbedo = tex2D( AlbedoSampler, vTexCoord );
    for( int i=0; i < 4; i++ )
    {
        vTransferVectorBehind[i] *= vAlbedo.a;
    }

    // Red
    vExitRadiance[0] += g_vLightCoeffsR[0] * (vTransferVector[0] + g_vColorTransmit.r * vTransferVectorBehind[0]);
    vExitRadiance[0] += g_vLightCoeffsR[1] * (vTransferVector[1] + g_vColorTransmit.r * vTransferVectorBehind[1]);
    vExitRadiance[0] += g_vLightCoeffsR[2] * (vTransferVector[2] + g_vColorTransmit.r * vTransferVectorBehind[2]);
    vExitRadiance[0] += g_vLightCoeffsR[3] * (vTransferVector[3] + g_vColorTransmit.r * vTransferVectorBehind[3]);

    // Green
    vExitRadiance[1] += g_vLightCoeffsG[0] * (vTransferVector[0] + g_vColorTransmit.g * vTransferVectorBehind[0]);
    vExitRadiance[1] += g_vLightCoeffsG[1] * (vTransferVector[1] + g_vColorTransmit.g * vTransferVectorBehind[1]);
    vExitRadiance[1] += g_vLightCoeffsG[2] * (vTransferVector[2] + g_vColorTransmit.g * vTransferVectorBehind[2]);
    vExitRadiance[1] += g_vLightCoeffsG[3] * (vTransferVector[3] + g_vColorTransmit.g * vTransferVectorBehind[3]);

    // Blue
    vExitRadiance[2] += g_vLightCoeffsB[0] * (vTransferVector[0] + g_vColorTransmit.b * vTransferVectorBehind[0]);
    vExitRadiance[2] += g_vLightCoeffsB[1] * (vTransferVector[1] + g_vColorTransmit.b * vTransferVectorBehind[1]);
    vExitRadiance[2] += g_vLightCoeffsB[2] * (vTransferVector[2] + g_vColorTransmit.b * vTransferVectorBehind[2]);
    vExitRadiance[2] += g_vLightCoeffsB[3] * (vTransferVector[3] + g_vColorTransmit.b * vTransferVectorBehind[3]);

    return float4( dot(vExitRadiance[0], 1), 
                   dot(vExitRadiance[1], 1), 
                   dot(vExitRadiance[2], 1), 
                   1 );
} 


//--------------------------------------------------------------------------------------
// Basic vertex transformation
//--------------------------------------------------------------------------------------
struct VS_INPUT {
    float4 Position     : POSITION;
    float3 Normal       : NORMAL;
    float3 Tangent      : TANGENT; 
    float4 BlendWeights : BLENDWEIGHT;
    float4 BlendIndices : BLENDINDICES;
    float2 TexCoord     : TEXCOORD0;
};

struct VS_OUTPUT 
{
    float4 Position   : POSITION;
    float2 TexCoord   : TEXCOORD0;
    float3 Normal     : TEXCOORD1;
    float3 Tangent    : TEXCOORD2;
    float3 Binormal   : TEXCOORD3;
};

VS_OUTPUT VS( VS_INPUT In, uniform int NumBones ) 
{
    VS_OUTPUT Out;
    Out.Position = 0;
    Out.Normal = 0;
    Out.Tangent = 0;
    
    // Cast the vectors to arrays for use in the for loop below
    int IndexArray[4] = (int[4])In.BlendIndices;
    float BlendWeightsArray[4] = (float[4])In.BlendWeights;
    
    float LastWeight = 0;
    for (int iBone = 0; iBone < NumBones-1; iBone++)
    {
        Out.Position.xyz += mul( In.Position, g_mWorldMatrixArray[IndexArray[iBone]] ) * BlendWeightsArray[iBone];
        Out.Normal += mul( In.Normal, g_mWorldMatrixArray[IndexArray[iBone]] ) * BlendWeightsArray[iBone];
        Out.Tangent += mul( In.Tangent, g_mWorldMatrixArray[IndexArray[iBone]] ) * BlendWeightsArray[iBone];
        
        LastWeight += BlendWeightsArray[iBone];
    }
    LastWeight = 1.0f - LastWeight; 
    
    // Now that we have the calculated weight, add in the final influence
    Out.Position.xyz += ( mul( In.Position, g_mWorldMatrixArray[IndexArray[NumBones-1]] ) * LastWeight );
    Out.Normal += ( mul( In.Normal, g_mWorldMatrixArray[IndexArray[NumBones-1]] ) * LastWeight ); 
    Out.Tangent += ( mul( In.Tangent, g_mWorldMatrixArray[IndexArray[NumBones-1]] ) * LastWeight ); 
    
    // Transform the position into screen space
    Out.Position.w = 1;
    Out.Position = mul( Out.Position, g_mViewProjection );
    
    // Set up the tangent frame in world space
    Out.Binormal = cross( Out.Normal, Out.Tangent ); 

    // Pass the texture coordinate
    Out.TexCoord = In.TexCoord;
    
    return Out;
} 


//--------------------------------------------------------------------------------------
// Per-pixel N dot L / local deformable PRT lighting
//--------------------------------------------------------------------------------------
float4 PS( VS_OUTPUT In, uniform bool bNdotL, uniform bool bUnBias ) : COLOR0 
{
    // Albedo
    float4 Color = tex2D( AlbedoSampler, In.TexCoord );
  
    // Normal map
    float3 Normal = tex2D( NormalMapSampler, In.TexCoord );
    
    // If using a signed texture, we must unbias the normal map data
    if(bUnBias)
        Normal = (Normal * 2) - 1;
   
    // Move the normal from tangent space to world space
    float3x3 mTangentFrame = { In.Tangent, In.Binormal, In.Normal };
    Normal = mul( Normal, mTangentFrame );
    
    if( bNdotL )
    {
        // Basic N dot L lighting
        Color *= g_fLightIntensity * saturate( dot( Normal, g_vLightDirection ) );
    }
    else
    {
        // 1 channel LDPRT, 3 channel lighting loading Ylm coeffs from cubemaps
        Color *= LDPRTCoeffLighting( In.TexCoord, Normal );
    }   

    return Color;
} 

                          
//--------------------------------------------------------------------------------------
// Techniques
//--------------------------------------------------------------------------------------
technique NdotL
{
    pass p0 
    {
        VertexShader = compile vs_2_0 VS(g_NumBones);
        PixelShader  = compile ps_2_0 PS(true,false);
    }
}

technique LDPRT
{
    pass p0 
    {
        VertexShader = compile vs_2_0 VS(g_NumBones);
        PixelShader  = compile ps_2_0 PS(false,false);
    }
}

technique NdotL_Unbias
{
    pass p0 
    {
        VertexShader = compile vs_2_0 VS(g_NumBones);
        PixelShader  = compile ps_2_0 PS(true,true);
    }
}

technique LDPRT_Unbias
{
    pass p0 
    {
        VertexShader = compile vs_2_0 VS(g_NumBones);
        PixelShader  = compile ps_2_0 PS(false,true);
    }
}
