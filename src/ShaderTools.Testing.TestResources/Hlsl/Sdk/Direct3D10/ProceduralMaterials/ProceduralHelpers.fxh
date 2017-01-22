//--------------------------------------------------------------------------------------
// File: ProceduralHelpers.fx
//
// This effect file contains procedural helper functions (or building blocks) for creating
// the procedural effects found in proceduralmaterials.fx
// 
// Copyright (c) Microsoft Corporation. All rights reserved.
//--------------------------------------------------------------------------------------
#include "noise.fx"

//--------------------------------------------------------------------------------------
// Creates a rolling noise look
//--------------------------------------------------------------------------------------
float RollNoise( uniform int octaves, float3 WorldPos, float2 UV, uniform float fSign )
{
    // Bias the position so that we get the "size" of noise that we want
    float3 NewPos = float3(WorldPos.x,UV.y,WorldPos.z);
    
    // Roll the noise along the Y direction
    NewPos.y += fSign* g_fTime * 0.1 * g_fSliderVal[2];
    NewPos.y += fSign* 0.2 * g_fSliderVal[3];
    
    // Bias this again before we lookup the noise
    float4 NoiseIn2 = float4( NewPos * 2, g_fTime * 0.05 );

    // simplex noise
    return fBm( NoiseIn2, octaves, 2, 0.5, NT_SIMPLEX4D );
}

//--------------------------------------------------------------------------------------
// Creates a grid pattern in 3D
// This is a relatively cheap grid technique.  LineWidth determines the width of the grid
// lines.  Hardness controls the hard look or "fuzziness" of the lines.
//--------------------------------------------------------------------------------------
float Grid( float3 WorldPos, float3 LineWidth, float Hardness )
{
    // Assume the world is full of axis aligned planes at integer values
    // Find the 6 planes nearest the World point
    float3 NearPlanes1 = ceil(WorldPos);
    float3 NearPlanes2 = floor(WorldPos);
    
    // Find the distance to each of these planes
    float3 Delta1 = abs(WorldPos - NearPlanes1);
    float3 Delta2 = abs(WorldPos - NearPlanes2);
    
    // Clamp to within the line width of the planes
    float3 Intermediate1 = saturate(LineWidth - Delta1);
    float3 Intermediate2 = saturate(LineWidth - Delta2);
    
    // This is basically squaring the distances to the planes together and summing them all up
    float retval = 0;
    retval += dot( Intermediate1, Intermediate1 );
    retval += dot( Intermediate2, Intermediate2 );

    // Multiplying by hardness will change how much of the value actually goes above 1
    // This effectively changes the hardness look of the lines
    return saturate(retval*Hardness);
}

//--------------------------------------------------------------------------------------
// Creates a grid pattern in 3D where some of the blocks are staggered based on the masks.
// LineWidth determines the width of the grid
// lines.  Hardness controls the hard look or "fuzziness" of the lines.
//--------------------------------------------------------------------------------------
float StaggeredGrid( float3 WorldPos, float3 LineWidth, float Hardness, float3 StaggerOnMask, float3 StaggerByMask )
{
    // Shift some of the coordinates by applying the stagger masks.
    // StaggerOnMask squishes the world position based upon the mask.
    // StaggerByMask moves the position in increments of hole units.
    float3 Test = abs( floor( ( WorldPos * StaggerOnMask ) ) );
    uint3 IntTest = (uint3)Test;
    uint3 Mod2s = IntTest & 1;
    float MaxMod = ( Mod2s.x + Mod2s.y + Mod2s.z );
    WorldPos += float3( 0.5, 0.5, 0.5 ) * MaxMod * StaggerByMask;
        
    // Just call Grid on the shifted coordinates
    return Grid( WorldPos, LineWidth, Hardness );
}

//--------------------------------------------------------------------------------------
// Noise function for cloud shader
// This is just a wrapper around the noise function that makes use of Clearness.  Clearness
// determines how clear or not overcast the sky is.
//--------------------------------------------------------------------------------------
float CloudNoise( float3 Position, int Octaves, float Clearness )
{
    float NoiseOut = fBm( float4( Position, g_fTime * 0.02 ), Octaves, 2, 0.5, NT_SIMPLEX3D );
    
    NoiseOut = NoiseOut * 0.5 + 0.5;
    NoiseOut -= Clearness;
    NoiseOut = saturate(NoiseOut) * (1/(1-Clearness));
    return NoiseOut;
}

//--------------------------------------------------------------------------------------
// Colored grid
// This is similar to Grid, except we lookup our color in the random texture.
//--------------------------------------------------------------------------------------
float GridColor( float3 WorldPos )
{
    // This assumes that we changes colors every 1 world unit.  The World variable should
    // be pre-scaled before calling this function.
    float3 NearLine = floor( WorldPos ) / 255;
    
    // We scale NearLine.z by 10 just to give some variation in the color
    float output = g_txRandomByte.SampleLevel( g_samWrap, NearLine.xy + 10 * NearLine.z, 0 ).r;
    
    return output;
}

//--------------------------------------------------------------------------------------
// Staggered grid color
//--------------------------------------------------------------------------------------
float StaggeredGridColor( float3 WorldPos, float3 staggeronmask, float3 staggerbymask )
{
    // Shift some of the coordinates by applying some fancy masks
    float3 Test = abs( floor( WorldPos * staggeronmask ) );
    uint3 IntTest = (uint3)Test;
    uint3 Mod2s = IntTest & 1;
    float MaxMod = ( Mod2s.x + Mod2s.y + Mod2s.z );
    WorldPos += float3( 0.5, 0.5, 0.5 ) * MaxMod * staggerbymask;
    
    // Then just call GridColor on the staggered coordinates
    return GridColor( WorldPos );
}

//--------------------------------------------------------------------------------------
// Similar to a grid but with spheres.  This assumes that there are a infinite number
// of spheres of size Radius placed at every point in 3D space where the coordinates
// are whole numbers.  Hardness controls the hardness of the edges of the sphere.  Lower
// numbers give a fuzzier edge.
//--------------------------------------------------------------------------------------
float SphereGrid( float3 WorldPos, float Radius, float Hardness )
{
    // This assumes that there is a sphere at every set of R3 that contains only whole
    // numbers as coordinates.  So find our distance to the whole numbers above us and
    // to the whole numbers below us.
    float3 Max = ceil( WorldPos );
    float3 Min = floor( WorldPos );
    
    // Then really go through and find the minimum distances
#define MAX_DELTAS 8
    float3 Deltas[MAX_DELTAS];
    Deltas[0] = WorldPos - Min;
    Deltas[1] = WorldPos - float3( Min.x, Min.y, Max.z );
    Deltas[2] = WorldPos - float3( Min.x, Max.y, Min.z );
    Deltas[3] = WorldPos - float3( Min.x, Max.y, Max.z );
    Deltas[4] = WorldPos - float3( Max.x, Min.y, Min.z );
    Deltas[5] = WorldPos - float3( Max.x, Min.y, Max.z );
    Deltas[6] = WorldPos - float3( Max.x, Max.y, Min.z );
    Deltas[7] = WorldPos - Max;
    
    float retval = 0;
    float RadSq = Radius * Radius;
    
    for( int i=0; i<MAX_DELTAS; i++ )
    {
        // Square the distance and check against RadSq
        float fDist = dot( Deltas[i], Deltas[i] );
        retval += saturate( RadSq - fDist) ;
    }
    
    return saturate( retval * Hardness );
}

//--------------------------------------------------------------------------------------
// Cheaper way to find the normal using ddx and ddy instead of rendering to the backbuffer
// Also slightly lower quality.
// For every value, we can find the change in that value with respect to screen x (ddx)
// and with respect to screen y (ddy).  Because this value is only computed per-quad
// there may be some blocking artifacts on the screen when using this approach.
//--------------------------------------------------------------------------------------
float3 FindNormal( float3 Value )
{
    float3 dWorldX = ddx(Value);
    float3 dWorldY = ddy(Value);
    float3 bumpNorm = normalize( cross( dWorldX, dWorldY ) );
    return bumpNorm;
}

//--------------------------------------------------------------------------------------
// Just passes stuff through
//--------------------------------------------------------------------------------------
struct VS_QUADOUT
{
    float4 Position     : SV_POSITION;
};

VS_QUADOUT RenderQuadVS( float4 vPos : POSITION,
                         float2 vTexcoord : TEXCOORD0 )
{
    VS_QUADOUT output;
    output.Position = vPos;
    return output;
}

//--------------------------------------------------------------------------------------
// Quick and dirty central differencing to reconstruct normals from depth buffer
// This isn't exact, but it's fast and generally gives good results.
//--------------------------------------------------------------------------------------
float4 CreateNormalsPS( VS_QUADOUT In ) : SV_TARGET
{
    // constants to help us determine when the differencing need not apply
    const float MinNormalDist = 0.1;  // Pick a small value for the minimum normal distance that we care about
    const float BlowOut = 10000000.0; // Pick a large number to oversaturate the distance between values.  This allows
                                      // us to avoid a branch and just do some masking of values that are too far out of range.
    
    // Load us and our neighbors
    uint3 pos = uint3(In.Position.xy,0);
    float4 Middle = g_PositionDepthTexture.Load( pos );
    float4 Bottom = g_PositionDepthTexture.Load( pos - uint3(0,1,0) );
    float4 Top = g_PositionDepthTexture.Load( pos + uint3(0,1,0) );
    float4 Left = g_PositionDepthTexture.Load( pos - uint3(1,0,0) );
    float4 Right = g_PositionDepthTexture.Load( pos + uint3(1,0,0) );
    
    // Difference
    float4 ToTop = Top - Middle;
    float4 ToBottom = Middle - Bottom;
    float4 ToRight = Right - Middle;
    float4 ToLeft = Middle - Left;
    
    // Bias us in such a way that pixels who are too far apart don't count.
    // It's  little nicer than an if statement and does about the same thing.
    float4 SatVector;
    SatVector.x = ( MinNormalDist - abs(ToTop.w) ) * BlowOut;
    SatVector.y = ( MinNormalDist - abs(ToBottom.w) ) * BlowOut;
    SatVector.z = ( MinNormalDist - abs(ToRight.w) ) * BlowOut;
    SatVector.w = ( MinNormalDist - abs(ToLeft.w) ) * BlowOut;
    SatVector = saturate( SatVector );
    
    float3 CrossUp = ( ToTop * SatVector.x + ToBottom * SatVector.y ) / ( SatVector.x + SatVector.y );
    float3 CrossRight = ( ToRight * SatVector.z + ToLeft * SatVector.w ) / ( SatVector.z + SatVector.w );

    float3 Normal = normalize( cross( CrossRight, CrossUp ) );
    
    return float4( Normal, 0 );
    
}