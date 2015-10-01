//-----------------------------------------------------------------------------
// File: SHIrradianceEnvMap.fx
//
// Copyright (c) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------


//-----------------------------------------------------------------------------
// Global variables
//-----------------------------------------------------------------------------
float4x4 g_mWorldViewProjection;
texture  AlbedoTexture;

float4 MaterialDiffuseColor = { 1.0f, 1.0f, 1.0f, 1.0f };    

float4 cAr;
float4 cAg;
float4 cAb;
float4 cBr;
float4 cBg;
float4 cBb;
float4 cC;


//-----------------------------------------------------------------------------
// Texture samplers
//-----------------------------------------------------------------------------
sampler AlbedoSampler = 
sampler_state
{
    Texture = <AlbedoTexture>;
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
    float2 TextureUV : TEXCOORD0;   // typical texture coords stored here
};


//-----------------------------------------------------------------------------
VS_OUTPUT SHIrradianceEnvironmentMapVS( float4 vPos : POSITION, 
                                        float4 vNormal : NORMAL,
                                        float2 vTexCoord0 : TEXCOORD0,
                                        uniform bool bTexture )
{
    VS_OUTPUT Output;
    
    // Output the vetrex position in projection space
    Output.Position = mul(vPos, g_mWorldViewProjection);
    
    float3 x1, x2, x3;
    
    // Linear + constant polynomial terms
    x1.r = dot(cAr,vNormal);
    x1.g = dot(cAg,vNormal);
    x1.b = dot(cAb,vNormal);
    
    // 4 of the quadratic polynomials
    float4 vB = vNormal.xyzz * vNormal.yzzx;   
    x2.r = dot(cBr,vB);
    x2.g = dot(cBg,vB);
    x2.b = dot(cBb,vB);
   
    // Final quadratic polynomial
    float vC = vNormal.x*vNormal.x - vNormal.y*vNormal.y;
    x3 = cC.rgb * vC;    

    Output.Diffuse.rgb = x1 + x2 + x3;
    Output.Diffuse.a   = 1.0f; 
    
    Output.Diffuse *= MaterialDiffuseColor;
    
	if( bTexture )
		Output.TextureUV = vTexCoord0;
	else
		Output.TextureUV = 0;
    
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
// Trival pixel shader (could use FF if desired)
//-----------------------------------------------------------------------------
PS_OUTPUT StandardPS( VS_OUTPUT In,
                      uniform bool bTexture ) 
{ 
    PS_OUTPUT Output;

	if( bTexture )
		Output.RGBColor = tex2D(AlbedoSampler, In.TextureUV) * In.Diffuse;
	else
		Output.RGBColor = In.Diffuse;

    return Output;
}


//-----------------------------------------------------------------------------
technique RenderWithSHIrradEnvMap
{
    pass P0
    {          
        VertexShader = compile vs_2_0 SHIrradianceEnvironmentMapVS( true );
        PixelShader  = compile ps_2_0 StandardPS( true ); 
    }
}


//-----------------------------------------------------------------------------
technique RenderWithSHIrradEnvMapNoAlbedo
{
    pass P0
    {          
        VertexShader = compile vs_2_0 SHIrradianceEnvironmentMapVS( false );
        PixelShader  = compile ps_2_0 StandardPS( false );
    }
}




