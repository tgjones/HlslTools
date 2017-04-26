//-----------------------------------------------------------------------------
// File: Scene.fx
//
// Copyright (c) ATI Research, Inc.
//-----------------------------------------------------------------------------


//-----------------------------------------------------------------------------
// Global variables
//-----------------------------------------------------------------------------
float4x4 g_mWorldViewProjection;
float4x4 g_mWorldView;
texture g_tTexture;

//-----------------------------------------------------------------------------
sampler TextureSampler = sampler_state
{ 
    Texture = (g_tTexture);
    MipFilter = LINEAR; 
    MinFilter = LINEAR;
    MagFilter = LINEAR;
};


//-----------------------------------------------------------------------------
// Vertex shader input/output structure
//-----------------------------------------------------------------------------
struct VS_INPUT
{
    float4 Position  : POSITION;    // position of the vertex
    float2 TexCoord  : TEXCOORD0;
};

struct VS_OUTPUT
{
    float4 Position  : POSITION;    // position of the vertex
    float2 TexCoord  : TEXCOORD0;
};

//-----------------------------------------------------------------------------
// Vertex Shader
//-----------------------------------------------------------------------------
VS_OUTPUT SceneVS ( VS_INPUT Input )
{
    VS_OUTPUT Output;
    
    // Output the vetrex position in projection space
    Output.Position = mul(Input.Position, g_mWorldViewProjection);
    Output.TexCoord = Input.TexCoord;
   
    return Output;
}

VS_OUTPUT SceneDepthVS ( VS_INPUT Input )
{
    VS_OUTPUT Output;
    
    // Output the vetrex position in projection space
    Output.Position = mul(Input.Position, g_mWorldViewProjection);
    Output.TexCoord.xy = mul( Input.Position, g_mWorldView ).z;
   
    return Output;
}


//-----------------------------------------------------------------------------
// Pixel shader
//-----------------------------------------------------------------------------
float4 ScenePS ( VS_OUTPUT Input ) : COLOR0 
{ 
    float4 Output = tex2D(TextureSampler, Input.TexCoord);    
    return Output;
}

float4 SceneDepthPS ( VS_OUTPUT Input ) : COLOR0 
{ 
    float  fDepth = 1.0 / Input.TexCoord.x;
    
    float4 Output = float4(fDepth, fDepth, fDepth, 1);
    return Output;
}


//-----------------------------------------------------------------------------
// 
//-----------------------------------------------------------------------------
technique RenderScene
{
    pass P0
    {   
        //CullMode = CCW;
        VertexShader = compile vs_2_0 SceneVS( );
        PixelShader  = compile ps_2_0 ScenePS( ); 
    }
}

technique RenderSceneRadiance
{
    pass P0
    {   
        CullMode = NONE;       
        VertexShader = compile vs_2_0 SceneVS( );
        PixelShader  = compile ps_2_0 ScenePS( ); 
    }
}

technique RenderSceneDepth
{
    pass P0
    {      
        CullMode = NONE;    
        VertexShader = compile vs_2_0 SceneDepthVS( );
        PixelShader  = compile ps_2_0 SceneDepthPS( ); 
    }
}