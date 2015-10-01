//-----------------------------------------------------------------------------
// File: PRT.fx
//
// Desc: The technique PrecomputedSHLighting renders the scene with per vertex PRT
// 
// Copyright (c) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------


//-----------------------------------------------------------------------------
// Global variables
//-----------------------------------------------------------------------------
float4x4 g_mWorldViewProjection;
float4x4 g_mWorld;
float4 g_vCameraPosOS;
texture AlbedoTexture;

#define NUM_CHANNELS    3
// The values for NUM_CLUSTERS and NUM_PCA are
// defined by the app upon the D3DXCreateEffectFromFile() call.

float4 aPRTConstants[NUM_CLUSTERS*(1+NUM_CHANNELS*(NUM_PCA/4))];

float4 MaterialDiffuseColor = { 1.0f, 1.0f, 1.0f, 1.0f };    


//-----------------------------------------------------------------------------
sampler AlbedoSampler = sampler_state
{ 
    Texture = (AlbedoTexture);
    MipFilter = LINEAR; 
    MinFilter = LINEAR;
    MagFilter = LINEAR;
};


//-----------------------------------------------------------------------------
// Vertex shader output structure
//-----------------------------------------------------------------------------
struct VS_OUTPUT
{
    float4 Position  : POSITION;    // position of the vertex
    float4 Diffuse   : COLOR0;      // diffuse color of the vertex
    float2 TexCoord  : TEXCOORD0;
};


//-----------------------------------------------------------------------------
float4 GetPRTDiffuse( int iClusterOffset, float4 vPCAWeights[NUM_PCA/4] )
{
    // With compressed PRT, a single diffuse channel is caluated by:
    //       R[p] = (M[k] dot L') + sum( w[p][j] * (B[k][j] dot L');
    // where the sum runs j between 0 and # of PCA vectors
    //       R[p] = exit radiance at point p
    //       M[k] = mean of cluster k 
    //       L' = source radiance coefficients
    //       w[p][j] = the j'th PCA weight for point p
    //       B[k][j] = the j'th PCA basis vector for cluster k
    //
    // Note: since both (M[k] dot L') and (B[k][j] dot L') can be computed on the CPU, 
    // these values are passed in as the array aPRTConstants.   
           
    float4 vAccumR = float4(0,0,0,0);
    float4 vAccumG = float4(0,0,0,0);
    float4 vAccumB = float4(0,0,0,0);
    
    // For each channel, multiply and sum all the vPCAWeights[j] by aPRTConstants[x] 
    // where: vPCAWeights[j] is w[p][j]
    //        aPRTConstants[x] is the value of (B[k][j] dot L') that was
    //        calculated on the CPU and passed in as a shader constant
    // Note this code is multipled and added 4 floats at a time since each 
    // register is a 4-D vector, and is the reason for using (NUM_PCA/4)
    for (int j=0; j < (NUM_PCA/4); j++) 
    {
        vAccumR += vPCAWeights[j] * aPRTConstants[iClusterOffset+1+(NUM_PCA/4)*0+j];
        vAccumG += vPCAWeights[j] * aPRTConstants[iClusterOffset+1+(NUM_PCA/4)*1+j];
        vAccumB += vPCAWeights[j] * aPRTConstants[iClusterOffset+1+(NUM_PCA/4)*2+j];
    }    

    // Now for each channel, sum the 4D vector and add aPRTConstants[x] 
    // where: aPRTConstants[x] which is the value of (M[k] dot L') and
    //        was calculated on the CPU and passed in as a shader constant.
    float4 vDiffuse = aPRTConstants[iClusterOffset];
    vDiffuse.r += dot(vAccumR,1);
    vDiffuse.g += dot(vAccumG,1);
    vDiffuse.b += dot(vAccumB,1);
    
    return vDiffuse;
}


//-----------------------------------------------------------------------------
// Renders using per vertex PRT with compression with optional texture
//-----------------------------------------------------------------------------
VS_OUTPUT PRTDiffuseVS( float4 vPos : POSITION,
                        float2 TexCoord : TEXCOORD0,
                        int iClusterOffset : BLENDWEIGHT,
                        float4 vPCAWeights[NUM_PCA/4] : BLENDWEIGHT1,
                        uniform bool bUseTexture )
{
    VS_OUTPUT Output;
    
    // Output the vetrex position in projection space
    Output.Position = mul(vPos, g_mWorldViewProjection);
    if( bUseTexture )
        Output.TexCoord = TexCoord;
    else
        Output.TexCoord = 0;
    
    // For spectral simulations the material properity is baked into the transfer coefficients.
    // If using nonspectral, then you can modulate by the diffuse material properity here.
    Output.Diffuse = GetPRTDiffuse( iClusterOffset, vPCAWeights );
    
    Output.Diffuse *= MaterialDiffuseColor;
    
    return Output;
}


//-----------------------------------------------------------------------------
// Pixel shader output structure
//-----------------------------------------------------------------------------
struct PS_OUTPUT
{   
    float4 RGBColor : COLOR0;  // Pixel color    
};


//-----------------------------------------------------------------------------
// Name: StandardPS
// Type: Pixel shader
// Desc: Trival pixel shader
//-----------------------------------------------------------------------------
PS_OUTPUT StandardPS( VS_OUTPUT In, uniform bool bUseTexture ) 
{ 
    PS_OUTPUT Output;
    
    if( bUseTexture )
    {
        float4 Albedo = tex2D(AlbedoSampler, In.TexCoord);    
        Output.RGBColor = In.Diffuse * Albedo;
    }
    else
    {
        Output.RGBColor = In.Diffuse;
    }

    return Output;
}


//-----------------------------------------------------------------------------
// Renders with per vertex PRT 
//-----------------------------------------------------------------------------
technique RenderWithPRTColorLights
{
    pass P0
    {   
        CullMode = NONE;       
        VertexShader = compile vs_2_0 PRTDiffuseVS( true );
        PixelShader  = compile ps_2_0 StandardPS( true ); // trival pixel shader 
    }
}

//-----------------------------------------------------------------------------
// Renders with per vertex PRT w/o albedo texture
//-----------------------------------------------------------------------------
technique RenderWithPRTColorLightsNoAlbedo
{
    pass P0
    {   
        CullMode = NONE;
        VertexShader = compile vs_2_0 PRTDiffuseVS( false );
        PixelShader  = compile ps_2_0 StandardPS( false ); // trival pixel shader 
    }
}
