//-----------------------------------------------------------------------------
// File: HLSLwithoutEffects.vsh
//
// Desc: The effect file for the BasicHLSL sample.  It contains a vertex 
//		 shader which animates the vertices.
// 
// Copyright (c) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------


//-----------------------------------------------------------------------------
// Global variables
//-----------------------------------------------------------------------------
float4x4 mWorldViewProj;  // World * View * Projection transformation
float fTime;			  // Time parameter. This keeps increasing


//-----------------------------------------------------------------------------
// Vertex shader output structure
//-----------------------------------------------------------------------------
struct VS_OUTPUT
{
    float4 Position   : POSITION;   // vertex position 
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


