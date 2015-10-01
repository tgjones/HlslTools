//--------------------------------------------------------------------------------------
// File: Noise.fx
//
// The noise effect file for the ProceduralMaterials sample.  This file implements
// both Simplex noise in 3D and 4D and voronoi noise.
//
// For a good explaination of the simplex noise algorithm see
// http://staffwww.itn.liu.se/~stegu/simplexnoise/simplexnoise.pdf
// 
// Copyright (c) Microsoft Corporation. All rights reserved.
//--------------------------------------------------------------------------------------
Texture2D<float>   g_txRandomByte;   // permutation texture
Texture1D<float4>  g_txRandVector;	 // random 1D texture of random vectors used for voronoi noise

//--------------------------------------------------------------------------------------
// Texture samplers
//--------------------------------------------------------------------------------------
SamplerState g_samWrap
{
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = Wrap;
    AddressV = Wrap;
};

//--------------------------------------------------------------------------------------
// Find which simplex we are in by magnitude sorting (3D case)
// This fast code for finding the simplex comes from the Perlin Fire sample from NVidia Corporation.  
// For more information about it, visit NVidia's developer website.
// http://developer.download.nvidia.com/whitepapers/2007/SDK10/PerlinFire.pdf
//--------------------------------------------------------------------------------------
void Simplex3D( const in float3 P, out float3 simplex[4] )
{
    float3 T = P.xzy >= P.yxz;
    simplex[0] = 0;
    simplex[1] = T.xzy > T.yxz;
    simplex[2] = T.yxz <= T.xzy;
    simplex[3] = 1;
}

//--------------------------------------------------------------------------------------
// Find which simplex we are in by magnitude sorting (4D case)
// This fast code for finding the simplex comes from the Perlin Fire sample from NVidia Corporation.  
// For more information about it, visit NVidia's developer website.
//--------------------------------------------------------------------------------------
void Simplex4D( const in float4 P, out float4 simplex[5] )
{
    float4 offset0;

    float3 isX = step( P.yzw, P.xxx );
    offset0.x = dot( isX, float3(1, 1, 1) );
    offset0.yzw = 1 - isX;

    float2 isY = step( P.zw, P.yy );
    offset0.y += dot( isY, float2(1, 1) );
    offset0.zw += 1 - isY;

    float isZ = step( P.w, P.z );
    offset0.z += isZ;
    offset0.w += 1 - isZ;

    // offset0 now contains the unique values 0,1,2,3 in each channel
    simplex[4] = 1;
    simplex[3] = saturate (   offset0);
    simplex[2] = saturate (-- offset0);
    simplex[1] = saturate (-- offset0);
    simplex[0] = 0;
}

//--------------------------------------------------------------------------------------
// 3D gradients
// These are used as a sort of gradient lookup table when doing noise evaluation.
//--------------------------------------------------------------------------------------
cbuffer cbCubeGradients
{
    const float g_TexDims = 256.0f;
    const float g_TexRandomDims = 1024.0f;
    
    // Gradients for the 3D cube that the 6 tetrahedra live in
    const float3 g_grad3[16] = 
    {
        float3(1,1,0),float3(-1,1,0),float3(1,-1,0),float3(-1,-1,0),
        float3(1,0,1),float3(-1,0,1),float3(1,0,-1),float3(-1,0,-1),
        float3(0,1,1),float3(0,-1,1),float3(0,1,-1),float3(0,-1,-1),
        
        //Wrap around to let us use an AND instead of a MOD
        float3(1,1,0),float3(-1,1,0),float3(1,-1,0),float3(-1,-1,0)
    };

    // Gradients for the 4D hypercube that the 24 tetrahetra live in
    const float4 g_grad4[32] = 
    {
        float4(0,1,1,1), float4(0,1,1,-1), float4(0,1,-1,1), float4(0,1,-1,-1),
        float4(0,-1,1,1), float4(0,-1,1,-1), float4(0,-1,-1,1), float4(0,-1,-1,-1),
        float4(1,0,1,1), float4(1,0,1,-1), float4(1,0,-1,1), float4(1,0,-1,-1),
        float4(-1,0,1,1), float4(-1,0,1,-1), float4(-1,0,-1,1), float4(-1,0,-1,-1),
        float4(1,1,0,1), float4(1,1,0,-1), float4(1,-1,0,1), float4(1,-1,0,-1),
        float4(-1,1,0,1), float4(-1,1,0,-1), float4(-1,-1,0,1), float4(-1,-1,0,-1),
        float4(1,1,1,0), float4(1,1,-1,0), float4(1,-1,1,0), float4(1,-1,-1,0),
        float4(-1,1,1,0), float4(-1,1,-1,0), float4(-1,-1,1,0), float4(-1,-1,-1,0),
    };
};

//--------------------------------------------------------------------------------------
// 3D simplex noise
//--------------------------------------------------------------------------------------
float3 SimplexNoise3D( float3 P )
{
    #define F3 0.333333333333    // 1 / 3: magic number for 3D space noise
    #define G3 0.166666666667    // 1 / 6: magic number for 3D space noise
    
    // Skew input space to determine which simplex we're in
    float u = dot( P, F3 );     // ( P.x + P.y + P.z ) * F3;
    float3 Pi = floor( P + u );
    float v = dot( Pi, G3 );    // ( Pi.x + Pi.y + Pi.z ) * G3;
    float3 PO = Pi - v;         // Unskew the cell origin back to < x, y, z > space

    float3 p[4];
    p[0] = P - PO;              // The x, y, z distances from the cell origin
    
    // For the 3D case, the simplex shape is a tetrahedron.
    // Find the 4 corners of the simplex
    float3 corner[4];
    Simplex3D( p[0], corner );
    
    //p[0] = p[0] - corner[0] + 0*G3;  // This line is commented out because, while it's complete to include it,
                                       // the value of p[0] turns out to be p[0].
    p[1] = p[0] - corner[1] +   G3;
    p[2] = p[0] - corner[2] + 2*G3;
    p[3] = p[0] - corner[3] + 3*G3;
    
    float totalNoise = 0;

    [unroll]
    for (int i = 0; i<4; i++)
    {        
        float3 tex = Pi + corner[i];
        tex /= g_TexDims;
        
        // We want an index, but the random values are normalized... so mul by 255
        uint index = g_txRandomByte.SampleLevel( g_samWrap, tex.xy + tex.z, 0 ) * 255;
        index &= 15; // Same thing as %16
        
        float t = saturate( 0.6f - dot( p[i], p[i] ) );
        t *= t; 
        totalNoise += t * t * dot( g_grad3[index], p[i] );
    }

    // Scale by 32 to get us into unit range
    return 32.0f * totalNoise;
}

//--------------------------------------------------------------------------------------
// 4D simplex noise
//--------------------------------------------------------------------------------------
float SimplexNoise4D( float4 P )
{
    #define F4 0.309016994375 // ( sqrt( 5 ) - 1 ) / 4
    #define G4 0.138196601125 // ( 5 - sqrt( 5 ) ) / 20

    // Skew the (x,y,z) space to determine which cell of 24 simplices we're in
    float u = dot( P, F4 );		// ( P.x + P.y + P.z + P.w ) * F4;
    float4 Pi = floor( P + u );
    float v = dot( Pi, G4 );	// ( Pi.x + Pi.y + Pi.z + Pi.w ) * G4;
    float4 PO = Pi - v;			// Unskew the cell origin back to < x, y, z, w > space

    float4 p[5];
    p[0] = P - PO;				// The x, y, z, w distances from the cell origin

    // To find out which of the 24 possible simplexes we're in
    float4 corner[5];
    Simplex4D( p[0], corner );

    //p[0] = p[0] - corner[0] + 0*G4;  // This line is commented out because, while it's complete to include it,
                                       // the value of p[0] turns out to be p[0].
    p[1] = p[0] - corner[1] +   G4;
    p[2] = p[0] - corner[2] + 2*G4;
    p[3] = p[0] - corner[3] + 3*G4;
    p[4] = p[0] - corner[4] + 4*G4;
    
    float totalNoise = 0;

    [unroll]
    for (int i = 0; i<5; i++)
    {        
        float4 tex = Pi + corner[i];
        tex /= g_TexDims;
        
        // We want an index, but the random values are normalized... so mul by 255
        uint index = g_txRandomByte.SampleLevel( g_samWrap, tex.xy + tex.z * tex.w, 0 ) * 255;
        index &= 31; // Same thing as %32
        
        float t = saturate( 0.6f - dot( p[i], p[i] ) );
        t *= t; 
        totalNoise += t * t * dot( g_grad4[index], p[i] );
    }

    // Scale by 32 to get us into unit range
    return 27.0f * totalNoise;
}

// Noise types
#define NT_SIMPLEX3D            0    // Standard 3D simplex noise
#define NT_SIMPLEX4D            1    // 4D simplex noise; good for animating
#define NT_SIMPLEX3D_TURB       2    // 3D simplex noise with turbulence
#define NT_SIMPLEX4D_TURB       3    // 4D simplex noise with turulence
#define NT_VORONOI3             4    // Voronoi noise evaluated over a 3x3x3 grid
#define NT_VORONOI3_2           5    // Voronoi noise over a 3x3x3 grid, but using the second distance function
#define NT_VORONOI2             6    // Voronoi noise evaluated over a 2x2x2 grid (faster, but some artifacts)
#define NT_VORONOI2_2           7    // Voronoi noise over a 2x2x2 grid, but using the second distance function


//--------------------------------------------------------------------------------------
// Voronoi noise overview
// For each sample position, we are trying to determine the distance to the closest point in
// a collection of randomly placed points in 3D space.  Because iterating over a series
// of points in space would take a very LONG time, we divide space up into a regular grid.
// The coordinates of each cell of this grid are used to create a hash that indexes into
// a set of random vectors stored in g_txRandVector.  This random vector corresponds to
// the location of a point inside that grid cell.  For each point in space, we examine
// all 9 surrounding grid cells and find the closest point for all 9 cells.  The methods
// that implement this routine are the 3 methods (for 3x3x3 surrounding cells).
//
// We also use a slightly faster method labelled the 2 method.  This method only looks at 4
// surrounding cells.  This means that we could miss the closest point in some cases.
// To increase the odds that we'll find the closest point, each cell in the 2X2 method
// contains 3 random points.  It's still not foolproof, but it's faster than the 3X3 method.
//--------------------------------------------------------------------------------------


//--------------------------------------------------------------------------------------
// Voronoi cell lookup - This gets a random point for a particular grid cell in 3D space.
// W is a random variation to the lookup.  Changing W with time will animate the 
// Voronoi noise.
//--------------------------------------------------------------------------------------
float3 GetCellPoint( float3 Cell, float W )
{
    float3 NewCell = Cell;
    
    // This is the hash for which random index corresponds to which cell
    float hashIndex = ( ( NewCell.z / NewCell.y ) * NewCell.x ) + ( NewCell.x * NewCell.y );
    hashIndex += W;
    hashIndex /= g_TexRandomDims;
    float3 randPt = g_txRandVector.SampleLevel( g_samWrap, hashIndex, 0 ).xyz;
    
    // Translate the random sampled point (randPt) to the center of the cell
    // and make sure it stays within the cell boundaries
    return Cell + float3( 0.5, 0.5, 0.5 ) + randPt * 0.5;
}

//--------------------------------------------------------------------------------------
// Voronoi noise 3x3x3 block.
// W is a random variation to the lookup.  Changing W with time will animate the 
// Voronoi noise.
//--------------------------------------------------------------------------------------
float Voronoi3( float3 P, float W )
{
    float3 Cell = floor( P );
    float mindistSq = 100.0f;  // Set mindistSq to a high number
    float3 Iter;
    
    for( Iter.x = Cell.x - 1; Iter.x < Cell.x + 2; Iter.x += 1 )
    {
        for( Iter.y = Cell.y - 1; Iter.y < Cell.y + 2; Iter.y += 1 )
        {
            for( Iter.z = Cell.z - 1; Iter.z < Cell.z + 2; Iter.z += 1 )
            {
                // Get the squared distance to the random sample point.
                float3 delta = GetCellPoint( Iter, W ) - P;
                float testDist = dot( delta, delta );
                mindistSq = min( mindistSq, testDist );
            }
        }
    }
    
    return mindistSq;
}

//--------------------------------------------------------------------------------------
// Voronoi noise 3x3x3 block second distance.
// W is a random variation to the lookup.  Changing W with time will animate the 
// Voronoi noise.
//--------------------------------------------------------------------------------------
float Voronoi3_2( float3 P, float W )
{
    float3 Cell = floor( P );
    float mindistSq = 100.0f;   // Set mindistSq to a high number
    float mindistSq2 = 100.0f;  // Set mindistSq2 to a high number
    float3 Iter;
    
    for( Iter.x = Cell.x - 1; Iter.x < Cell.x + 2 ; Iter.x += 1 )
    {
        for( Iter.y = Cell.y - 1; Iter.y < Cell.y + 2; Iter.y += 1 )
        {
            for( Iter.z = Cell.z - 1; Iter.z < Cell.z + 2; Iter.z += 1 )
            {
                // Get the squared distance to the random sample point.
                float3 delta = GetCellPoint( Iter, W ) - P;
                float testDist = dot( delta, delta );
                if( testDist < mindistSq )
                {
                    mindistSq2 = mindistSq;
                    mindistSq = testDist;
                }
                else
                {
                    mindistSq2 = min( testDist, mindistSq2 );
                }
            }
        }
    }
    
    return mindistSq2;
}

//--------------------------------------------------------------------------------------
// Voronoi noise 2x2x2 block approximation to the 3x3x3 version.
// W is a random variation to the lookup.  Changing W with time will animate the 
// Voronoi noise.
//--------------------------------------------------------------------------------------
float Voronoi2( float3 P, float W )
{
    float3 Cell = floor( P + float3( 0.5, 0.5, 0.5 ) );  // Bias to the position between the 8 neighboring cells
    float mindistSq = 100.0f;   // Set mindistSq to a high number
    float3 Iter;
    
    for( Iter.x = Cell.x - 1; Iter.x < Cell.x + 1; Iter.x += 1 )
    {
        for( Iter.y = Cell.y - 1; Iter.y < Cell.y + 1; Iter.y += 1 )
        {
            for( Iter.z = Cell.z - 1; Iter.z < Cell.z + 1; Iter.z += 1 )
            {
                // Sample 3 random points by shifting W
                for( int WShift = 0; WShift < 3; WShift++ )
                {
                    // Sample 3 points and shift W by WShift * 4.  Why 4?  Why not.
                    // Then get the squared distance to the random sample point.
                    float3 delta = GetCellPoint( Iter, W + WShift * 4 ) - P;
                    float testDist = dot( delta, delta );
                    mindistSq = min( mindistSq, testDist );
                }
            }
        }
    }
    
    return mindistSq;
}

//--------------------------------------------------------------------------------------
// Voronoi noise 2x2x2 block second distance approximation to the 3x3x3 version.
// W is a random variation to the lookup.  Changing W with time will animate the 
// Voronoi noise.
//--------------------------------------------------------------------------------------
float Voronoi2_2( float3 P, float W )
{
    float3 Cell = floor( P + float3(0.5,0.5,0.5) );  // Bias to the position between the 8 neighboring cells
    float mindistSq = 100.0f;  // Set mindistSq to a high number
    float mindistSq2 = 100.0f; // Set mindistSq2 to a high number
    float3 Iter;
    
    for( Iter.x = Cell.x - 1; Iter.x < Cell.x + 1; Iter.x += 1 )
    {
        for( Iter.y = Cell.y - 1; Iter.y < Cell.y + 1; Iter.y += 1 )
        {
            for( Iter.z = Cell.z - 1; Iter.z < Cell.z + 1; Iter.z += 1 )
            {
                // Sample 3 random points by shifting W
                for( int WShift = 0; WShift < 3; WShift++ )
                {
                    // Sample 3 points and shift W by WShift * 4.  Why 4?  Why not.
                    // Then get the squared distance to the random sample point.
                    float3 delta = GetCellPoint( Iter, W + WShift * 4 ) - P;
                    float testDist = dot( delta, delta );
                    if( testDist < mindistSq )
                    {
                        mindistSq2 = mindistSq;
                        mindistSq = testDist;
                    }
                    else
                    {
                        mindistSq2 = min( testDist, mindistSq2 );
                    }
                }
            }
        }
    }
    
    return mindistSq2;
}

//--------------------------------------------------------------------------------------
// Select different Voronoi noises based upon the input
//--------------------------------------------------------------------------------------
float VoronoiNoise( float3 P, float W, uniform uint NoiseType )
{
    float output = 0;
    
    if( NoiseType == NT_VORONOI3 )
        output = Voronoi3( P, W );
    else if( NoiseType == NT_VORONOI3_2 )
        output = Voronoi3_2( P, W );
    else if( NoiseType == NT_VORONOI2 )
        output = Voronoi2( P, W );
    else if( NoiseType == NT_VORONOI2_2 )
        output = Voronoi2_2( P, W );
        
    return output;
}

//--------------------------------------------------------------------------------------
// Sum octaves of noise at different frequencies.  For simplex noise, this produces the
// traditional "cloud" look.
//--------------------------------------------------------------------------------------
float fBm( float4 vInputCoords, uniform uint nNumOctaves, uniform float fLacunarity, 
           uniform float fGain, uniform uint NoiseType )
{
    float fNoiseSum = 0;
    float fAmplitude = 1;
    float fAmplitudeSum = 0;
    
    float4 vSampleCoords = vInputCoords;
    
    [unroll]
    for( uint i=0; i<nNumOctaves; i++ )
    {
        if( NoiseType == NT_SIMPLEX3D )
            fNoiseSum += fAmplitude * SimplexNoise3D( vSampleCoords );
        else if( NoiseType == NT_SIMPLEX4D )
            fNoiseSum += fAmplitude * SimplexNoise4D( vSampleCoords );
            
        else if( NoiseType == NT_SIMPLEX3D_TURB )
            fNoiseSum += fAmplitude * abs(SimplexNoise3D( vSampleCoords ));
        else if( NoiseType == NT_SIMPLEX4D_TURB )
            fNoiseSum += fAmplitude * abs(SimplexNoise4D( vSampleCoords ));
            
        else if( NoiseType >= NT_VORONOI3 )
            fNoiseSum += fAmplitude * VoronoiNoise( vSampleCoords.xyz, vSampleCoords.w, NoiseType );
            
        fAmplitudeSum += fAmplitude;
        fAmplitude *= fGain;
        vSampleCoords *= fLacunarity;
    }
    
    fNoiseSum /= fAmplitudeSum;
    return fNoiseSum;
}