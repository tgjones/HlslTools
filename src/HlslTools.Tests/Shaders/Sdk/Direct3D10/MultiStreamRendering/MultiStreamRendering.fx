//--------------------------------------------------------------------------------------
// File: SimpleSample.fx
//
// The effect file for the SimpleSample sample.  
// 
// Copyright (c) Microsoft Corporation. All rights reserved.
//--------------------------------------------------------------------------------------


//--------------------------------------------------------------------------------------
// Global variables
//--------------------------------------------------------------------------------------
float4 g_MaterialAmbientColor = float4(0,0,0,0);      // Material's ambient color
float4 g_MaterialDiffuseColor = float4(1,1,1,1);      // Material's diffuse color
float3 g_LightDir;                  // Light's direction in world space
float4 g_LightDiffuse;              // Light's diffuse color

float4x4 g_mWorld;                  // World matrix for object
float4x4 g_mWorldViewProjection;    // World * View * Projection matrix

//-----------------------------------------------------------------------------------------
// Textures and Samplers
//-----------------------------------------------------------------------------------------
texture2D g_MeshTexture;
sampler2D MeshTextureSampler = sampler_state
{
    Texture = (g_MeshTexture);
    //MinFilter = Linear;
    //MagFilter = Linear;
    AddressU = WRAP;
    AddressV = WRAP;
};

//--------------------------------------------------------------------------------------
// buffers
//--------------------------------------------------------------------------------------
#ifdef D3D10
Buffer<float4> g_posBuffer;
Buffer<float4> g_normBuffer;
#endif

//--------------------------------------------------------------------------------------
// shader input/output structure
//--------------------------------------------------------------------------------------
struct VS_INPUT_SI
{
    float4 Position   : POSITION;   // vertex position 
    float3 Normal     : NORMAL;		// this normal comes in per-vertex
    float2 TextureUV  : TEXCOORD0;  // vertex texture coords 
};

#ifdef D3D10
struct VS_INPUT_MI
{
    uint PositionIndex: POSINDEX;   // this is the index of the vertex position in g_posBuffer 
    float2 TextureUV  : TEXCOORD0;  // vertex texture coords 
    uint vertID  : SV_VertexID; // use this to lookup our normal in g_normBuffer
};
#endif

struct VS_OUTPUT
{
    float4 Position   : POSITION;   // vertex position 
    float4 Diffuse    : COLOR0;     // vertex diffuse color (note that COLOR0 is clamped from 0..1)
    float2 TextureUV  : TEXCOORD0;  // vertex texture coords 
};


//--------------------------------------------------------------------------------------
// This shader computes standard transform and lighting
//--------------------------------------------------------------------------------------
VS_OUTPUT RenderSceneSI_VS( VS_INPUT_SI input )
{
    VS_OUTPUT Output;
    float3 vNormalWorldSpace;
    
    // Transform the position from object space to homogeneous projection space
    Output.Position = mul( input.Position, g_mWorldViewProjection);
    
    // Transform the normal from object space to world space    
    vNormalWorldSpace = normalize(mul(input.Normal, (float3x3)g_mWorld)); // normal (world space)

    // Calc diffuse color    
    Output.Diffuse.rgb = g_MaterialDiffuseColor * g_LightDiffuse * max(0,dot(vNormalWorldSpace, g_LightDir)) + 
                         g_MaterialAmbientColor;   
    Output.Diffuse.a = 1.0f; 
    
    // Just copy the texture coordinate through
    Output.TextureUV = input.TextureUV; 
    
    return Output;    
}

//--------------------------------------------------------------------------------------
// This shader computes standard transform and lighting
//--------------------------------------------------------------------------------------
#ifdef D3D10
VS_OUTPUT RenderSceneMI_VS( VS_INPUT_MI input )
{
    VS_OUTPUT Output;
    float3 vNormalWorldSpace;
    
    // Load position from a buffer
    float4 Pos = g_posBuffer.Load( input.PositionIndex );
    float4 Norm = g_normBuffer.Load( input.vertID/3 );
    
    // Transform the position from object space to homogeneous projection space
    Output.Position = mul( Pos, g_mWorldViewProjection);
    
    // Transform the normal from object space to world space    
    vNormalWorldSpace = normalize(mul(Norm.xyz, (float3x3)g_mWorld)); // normal (world space)

    // Calc diffuse color    
    Output.Diffuse.rgb = g_MaterialDiffuseColor * g_LightDiffuse * max(0,dot(vNormalWorldSpace, g_LightDir)) + 
                         g_MaterialAmbientColor;   
    Output.Diffuse.a = 1.0f; 
    
    // Just copy the texture coordinate through
    Output.TextureUV = input.TextureUV; 
    
    return Output;    
}
#endif

//--------------------------------------------------------------------------------------
// This shader outputs the pixel's color by modulating the texture's
// color with diffuse material color
//--------------------------------------------------------------------------------------
float4 RenderScenePS( VS_OUTPUT In ) : COLOR0
{ 
    // Lookup mesh texture and modulate it with diffuse
    return tex2D( MeshTextureSampler, In.TextureUV) * In.Diffuse;
}


//--------------------------------------------------------------------------------------
// Renders scene for Direct3D 9
//--------------------------------------------------------------------------------------
technique RenderScene
{
    pass P0
    {   
        VertexShader = compile vs_2_0 RenderSceneSI_VS();
        PixelShader  = compile ps_2_0 RenderScenePS();     
    }
}

#ifdef D3D10
//--------------------------------------------------------------------------------------
// RendersScene Single Index for Direct3D 10
//--------------------------------------------------------------------------------------
technique10 RenderScene_SI
{
    pass P0
    {       
        SetVertexShader( CompileShader( vs_4_0, RenderSceneSI_VS() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, RenderScenePS() ) );
    }
}

//--------------------------------------------------------------------------------------
// RendersScene Multi Index for Direct3D 10
//--------------------------------------------------------------------------------------
technique10 RenderScene_MI
{
    pass P0
    {       
        SetVertexShader( CompileShader( vs_4_0, RenderSceneMI_VS() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, RenderScenePS() ) );
    }
}
#endif
