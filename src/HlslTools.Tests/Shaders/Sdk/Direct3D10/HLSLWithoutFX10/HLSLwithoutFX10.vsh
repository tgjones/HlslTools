//-----------------------------------------------------------------------------
// File: HLSLwithoutEffects.vsh
//
// Desc: The vertex shader file for the HLSLWithoutFX sample.  It contains a vertex 
//		 shader which animates the vertices.
// 
// Copyright (c) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------


//-----------------------------------------------------------------------------
// Global variables
//-----------------------------------------------------------------------------
cbuffer cb0
{
    row_major float4x4 mWorldViewProj : packoffset(c0);     // World * View * Projection transformation
    float fTime :                       packoffset(c5.y);   // Time parameter. This keeps increasing
                                                            // Notice, that this parameter is placed in c5.y.
                                                            // If it were packed by the default packing rules, 
                                                            // it would be placed in c4.x
};

//-----------------------------------------------------------------------------
// Vertex shader output structure
//-----------------------------------------------------------------------------
struct VS_OUTPUT
{
    float4 Position   : SV_Position;   // vertex position 
    float4 Diffuse    : COLOR0;     // vertex diffuse color
};


//-----------------------------------------------------------------------------
// Name: Ripple
// Type: Vertex shader                                      
// Desc: This shader ripples the vertices
//-----------------------------------------------------------------------------
VS_OUTPUT Ripple( in float2 vPosition : POSITION )
{
    VS_OUTPUT Output;
    
    float fSin, fCos;   
    float x = length( vPosition ) * sin( fTime ) * 15.0f;
    
    // This HLSL intrinsic computes returns both the sine and cosine of x
    sincos( x, fSin, fCos );

    // Change the y of the vertex position based on a function of time 
    // and transform the vertex into projection space. 
    Output.Position = mul( float4( vPosition.x, fSin * 0.1f, vPosition.y, 1.0f ), mWorldViewProj );
    
    // Output the diffuse color as function of time and 
    // the vertex's object space position
    Output.Diffuse = 0.5f - 0.5f * fCos;
    
    return Output;
}


