//--------------------------------------------------------------------------------------
// File: AntiAlias.fx
//
// The effect file for the AntiAlias sample.  
// 
// Copyright (c) Microsoft Corporation. All rights reserved.
//--------------------------------------------------------------------------------------


//--------------------------------------------------------------------------------------
// Global variables
//--------------------------------------------------------------------------------------
float4x4 g_mWorldViewProjection;	// World * View * Projection matrix

texture g_tDiffuse;

sampler PointSampler =
sampler_state
{
    Texture = <g_tDiffuse>;
    MinFilter = Point;
    MagFilter = Point;
};

sampler LinearSampler =
sampler_state
{
    Texture = <g_tDiffuse>;
    MinFilter = Linear;
    MagFilter = Linear;
};

sampler AnisotropicSampler =
sampler_state
{
    Texture = <g_tDiffuse>;
    MinFilter = Anisotropic;
    MagFilter = Linear;
    
    MaxAnisotropy = 4;
};

//--------------------------------------------------------------------------------------
// Vertex shader
//--------------------------------------------------------------------------------------
struct VS_OUTPUT
{
    float4 Position : POSITION;
    float4 Diffuse : COLOR0;
    float2 TexCoords : TEXCOORD0;
};

VS_OUTPUT ColorVS( float3 Position : POSITION,
                   float4 Diffuse : COLOR0 )
{
    VS_OUTPUT Output;
    
    Output.Position = mul( float4(Position, 1), g_mWorldViewProjection );
    Output.Diffuse = Diffuse;
    Output.TexCoords = 0;
    
    return Output;
}

VS_OUTPUT TextureVS( float3 Position : POSITION,
                     float2 TexCoords : TEXCOORD0 )
{
    VS_OUTPUT Output;
    
    Output.Position = mul( float4(Position, 1), g_mWorldViewProjection );
    Output.Diffuse = 0;
    Output.TexCoords = TexCoords;
    
    return Output;
}


//--------------------------------------------------------------------------------------
// Color fill
//--------------------------------------------------------------------------------------
float4 ColorPS( float4 vDiffuse : COLOR0 ) : COLOR0
{
    return vDiffuse;
}


technique Color
{
    pass P0
    {
        CullMode = NONE;
        
        VertexShader = compile vs_2_0 ColorVS();
        PixelShader = compile ps_2_0 ColorPS();          
    }
}


//--------------------------------------------------------------------------------------
// Texture - Point sampled
//--------------------------------------------------------------------------------------
float4 TexturePointPS( float4 TexCoord : TEXCOORD0 ) : COLOR0
{
    return tex2D( PointSampler, TexCoord );
}


technique TexturePoint
{
    pass P0
    {
        CullMode = NONE;
        
        VertexShader = compile vs_2_0 TextureVS();
        PixelShader = compile ps_2_0 TexturePointPS();          
    }
}


//--------------------------------------------------------------------------------------
// Texture - Point sampled (centroid)
//--------------------------------------------------------------------------------------
float4 TexturePointCentroidPS( float4 TexCoord : TEXCOORD0_centroid ) : COLOR0
{
    return tex2D( PointSampler, TexCoord );
}


technique TexturePointCentroid
{
    pass P0
    {
        CullMode = NONE;
        
        VertexShader = compile vs_2_0 TextureVS();
        PixelShader = compile ps_2_0 TexturePointCentroidPS();          
    }
}


//--------------------------------------------------------------------------------------
// Texture - Linear sampled
//--------------------------------------------------------------------------------------
float4 TextureLinearPS( float4 TexCoord : TEXCOORD0 ) : COLOR0
{
    return tex2D( LinearSampler, TexCoord );
}


technique TextureLinear
{
    pass P0
    {
        CullMode = NONE;
        
        VertexShader = compile vs_2_0 TextureVS();
        PixelShader = compile ps_2_0 TextureLinearPS();          
    }
}


//--------------------------------------------------------------------------------------
// Texture - Linear sampled (centroid)
//--------------------------------------------------------------------------------------
float4 TextureLinearCentroidPS( float4 TexCoord : TEXCOORD0_centroid ) : COLOR0
{
    return tex2D( LinearSampler, TexCoord );
}


technique TextureLinearCentroid
{
    pass P0
    {
        CullMode = NONE;
        
        VertexShader = compile vs_2_0 TextureVS();
        PixelShader = compile ps_2_0 TextureLinearCentroidPS();          
    }
}


//--------------------------------------------------------------------------------------
// Texture - Anisotropic sampled
//--------------------------------------------------------------------------------------
float4 TextureAnisotropicPS( float4 TexCoord : TEXCOORD0 ) : COLOR0
{
    return tex2D( AnisotropicSampler, TexCoord );
}


technique TextureAnisotropic
{
    pass P0
    {
        CullMode = NONE;
        
        VertexShader = compile vs_2_0 TextureVS();
        PixelShader = compile ps_2_0 TextureAnisotropicPS();          
    }
}


//--------------------------------------------------------------------------------------
// Texture - Anisotropic sampled (centroid)
//--------------------------------------------------------------------------------------
float4 TextureAnisotropicCentroidPS( float4 TexCoord : TEXCOORD0_centroid ) : COLOR0
{
    return tex2D( AnisotropicSampler, TexCoord );
}


technique TextureAnisotropicCentroid
{
    pass P0
    {
        CullMode = NONE;
        
        VertexShader = compile vs_2_0 TextureVS();
        PixelShader = compile ps_2_0 TextureAnisotropicCentroidPS();          
    }
}