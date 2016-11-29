//-----------------------------------------------------------------------------
// File: Scene.fx
//
// Desc: The technique PrecomputedSHLighting renders the scene with per vertex PRT
// 
// Copyright (c) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------


//-----------------------------------------------------------------------------
// Global variables
//-----------------------------------------------------------------------------
float4x4 g_mWorldViewProjection;

//-----------------------------------------------------------------------------
// Vertex shader input/output structure
//-----------------------------------------------------------------------------
struct VS_INPUT
{
    float4 Position  : POSITION;    // position of the vertex
};

struct VS_OUTPUT
{
    float4 Position  : POSITION;    // position of the vertex
};

//-----------------------------------------------------------------------------
// Vertex Shader
//-----------------------------------------------------------------------------
VS_OUTPUT WireframeVS ( VS_INPUT Input )
{
    VS_OUTPUT Output;
    
    // Output the vetrex position in projection space
    Output.Position = mul(Input.Position, g_mWorldViewProjection);
   
    return Output;
}


//-----------------------------------------------------------------------------
// Pixel shader
//-----------------------------------------------------------------------------
float4 WireframePS ( VS_OUTPUT Input ) : COLOR0 
{ 
    float4 Output = float4( 1.0, 1.0, 0.0, 0.0 );
    return Output;
}


//-----------------------------------------------------------------------------
// Renders with per vertex PRT 
//-----------------------------------------------------------------------------
technique Wireframe
{
    pass P0
    {          
        VertexShader = compile vs_2_0 WireframeVS( );
        PixelShader  = compile ps_2_0 WireframePS( ); 
    }
}