//--------------------------------------------------------------------------------------
// File: Particle.hlsl
//
// HLSL file containing shader function to render front-facing particles.
//
// Copyright (c) Microsoft Corporation. All rights reserved.
//--------------------------------------------------------------------------------------
#include "Shader_include.h"

//--------------------------------------------------------------------------------------
// Internal defines
//--------------------------------------------------------------------------------------
#define FIXED_VERTEX_RADIUS 5.0

//--------------------------------------------------------------------------------------
// Structures
//--------------------------------------------------------------------------------------
struct VS_PARTICLE_INPUT
{
    float3 WSPos : POSITION;
};

struct GS_PARTICLE_INPUT
{
    float4 WSPos : POSITION;
};

struct PS_PARTICLE_INPUT
{
    float4 Pos : SV_POSITION;
    float2 Tex : TEXCOORD0;
};

//--------------------------------------------------------------------------------------
// Vertex Shader to GS
//--------------------------------------------------------------------------------------
GS_PARTICLE_INPUT VSPassThrough( VS_PARTICLE_INPUT input )
{
    GS_PARTICLE_INPUT output = (GS_PARTICLE_INPUT)0;
    
    // Pass world space position to GS
    output.WSPos = float4( input.WSPos, 1.0 );
    
    return output;
}

//--------------------------------------------------------------------------------------
// Geometry Shader to render point sprites
//--------------------------------------------------------------------------------------
[maxvertexcount(4)]
void GSPointSprite(point GS_PARTICLE_INPUT input[1], inout TriangleStream<PS_PARTICLE_INPUT> SpriteStream)
{
    const float3 g_positions[4] =
    {
        float3( -1.0,  1.0, 0.0 ),
        float3(  1.0,  1.0, 0.0 ),
        float3( -1.0, -1.0, 0.0 ),
        float3(  1.0, -1.0, 0.0 ),
    };
    const float2 g_texcoords[4] = 
    { 
        float2( 0.0, 1.0 ), 
        float2( 1.0, 1.0 ),
        float2( 0.0, 0.0 ),
        float2( 1.0, 0.0 ),
    };
    PS_PARTICLE_INPUT output = (PS_PARTICLE_INPUT)0;
    
    // Emit two new triangles
    [unroll]for( int i=0; i<4; ++i )
    {
        float3 position = g_positions[i] * FIXED_VERTEX_RADIUS;
        position = mul( position, (float3x3)g_mInvView ) + input[0].WSPos;
        output.Pos = mul( float4( position, 1.0 ), g_mViewProjection );

        // Pass texture coordinates
        output.Tex = g_texcoords[i];
        
        // Add vertex
        SpriteStream.Append( output );
    }
    SpriteStream.RestartStrip();
}

//--------------------------------------------------------------------------------------
// Pixel Shader to display constant single color
//--------------------------------------------------------------------------------------
float4 PSConstantColor( PS_PARTICLE_INPUT input ) : SV_TARGET
{
    // Sample particle texture
    float4 vColor = g_baseTexture.Sample( g_samLinear, input.Tex ).wwww;
    
    // Clip fully transparent pixels
    clip( vColor.a - 1.0/255.0 );
    
    // Return color
    return vColor;
}
