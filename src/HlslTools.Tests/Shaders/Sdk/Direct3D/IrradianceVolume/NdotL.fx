//-----------------------------------------------------------------------------
// File: NdotL.fx
//
// Desc: The technique SimpleLighting renders the
//		 scene with standard N.L lighting.
// 
// Copyright (c) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------


//-----------------------------------------------------------------------------
// Global variables
//-----------------------------------------------------------------------------
float4x4 g_mWorldViewProjection;
float4x4 g_mWorldInv;
texture  AlbedoTexture;

float4 MaterialDiffuseColor = { 1.0f, 1.0f, 1.0f, 1.0f };    
float4 LightsDir[10]        : register(c10);
float4 Light1Dir            : register(c30);
float4 Light2Dir            : register(c31);
float4 LightsDiffuse[10]    : register(c20);


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
// Renders with standard N.L lighting
//-----------------------------------------------------------------------------
VS_OUTPUT SimpleLightingVS( float4 vPos : POSITION, 
                            float4 vNormal : NORMAL,
                            float2 vTexCoord0 : TEXCOORD0,
							uniform bool bTexture )
{
    VS_OUTPUT Output;
    
    // Output the vetrex position in projection space
    Output.Position = mul(vPos, g_mWorldViewProjection);    
  
    Output.Diffuse = 0;
    for( int i=0; i<10; i++ )    
    {
        float3 vLightObjSpace = mul(LightsDir[i], g_mWorldInv);
        Output.Diffuse += LightsDiffuse[i] * max(0, dot(vNormal, vLightObjSpace ));
    }
    
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
// Trival pixel shader
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
// Renders with standard N.L lighting with an albedo texture
//-----------------------------------------------------------------------------
technique RenderWithNdotL
{
    pass P0
    {          
        VertexShader = compile vs_2_0 SimpleLightingVS( true ); // trival vertex shader (could use FF if desired)
        PixelShader  = compile ps_2_0 StandardPS( true );       // trival pixel shader (could use FF if desired)
    }
}


//-----------------------------------------------------------------------------
// Renders with standard N.L lighting w/o an albedo texture
//-----------------------------------------------------------------------------
technique RenderWithNdotLNoAlbedo
{
    pass P0
    {          
        VertexShader = compile vs_2_0 SimpleLightingVS( false ); // trival vertex shader (could use FF if desired)
        PixelShader  = compile ps_2_0 StandardPS( false );       // trival pixel shader (could use FF if desired)
    }
}


