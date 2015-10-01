//--------------------------------------------------------------------------------------
// File: ProceduralMaterials.fx
//
// The effect file for the ProceduralMaterials sample.  This contains many different
// experimental procedural material effects.  They are provided as is and testbeds
// for different combinations of procedurals.
// 
// This file relies heavily on proceduralhelpers.fx.
//
// Copyright (c) Microsoft Corporation. All rights reserved.
//--------------------------------------------------------------------------------------

//--------------------------------------------------------------------------------------
// Global variables
//--------------------------------------------------------------------------------------
cbuffer cbPerFrame
{
    float3   g_vLightDir;               // Light's direction in world space
    float4   g_vLightDiffuse;           // Light's diffuse color

    float    g_fTime;                   // App's time in seconds
    float4x4 g_mWorld;                  // World matrix for object
    float4x4 g_mWorldViewProjection;    // World * View * Projection matrix
    float4x4 g_mViewProjection;         // World * View * Projection matrix
    float3   g_vEyePt;                  // Eye position in world space
    
    float4   g_vRenderTargetSize;       // Rendertarget size (in xy)
};

// NUM_SLIDERS is defined by the application
cbuffer cbPerUser
{
    float g_fSliderVal[NUM_SLIDERS];
    bool  g_bUseDDXDDY;
};

//--------------------------------------------------------------------------------------
// Texture objects
//--------------------------------------------------------------------------------------
Texture2D g_MeshTexture;            // Color texture for mesh
Texture2D g_PositionDepthTexture;   // Fullscreen position texture
Texture2D g_NormalDepthTexture;     // Fullscreen normal texture

//--------------------------------------------------------------------------------------
// Texture samplers
//--------------------------------------------------------------------------------------
SamplerState MeshTextureSampler
{
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = Wrap;
    AddressV = Wrap;
};

SamplerState g_samPoint
{
    Filter = MIN_MAG_MIP_POINT;
    AddressU = Clamp;
    AddressV = Clamp;
};

// Include proceduralhelpers.fx here so we can peek at any globals
#include "proceduralhelpers.fx"

//--------------------------------------------------------------------------------------
// States
//--------------------------------------------------------------------------------------
DepthStencilState EnableDepth
{
    DepthEnable = TRUE;
    DepthWriteMask = ALL;
    DepthFunc = LESS_EQUAL;
};

DepthStencilState DisableDepth
{
    DepthEnable = FALSE;
    DepthWriteMask = 0;
    DepthFunc = LESS_EQUAL;
};

BlendState DisableBlending
{
    BlendEnable[0] = FALSE;
};

BlendState SmokeBlending
{
    AlphaToCoverageEnable = FALSE;
    BlendEnable[0] = TRUE;
    SrcBlend = SRC_ALPHA;
    DestBlend = INV_SRC_ALPHA;
    BlendOp = ADD;
    SrcBlendAlpha = ZERO;
    DestBlendAlpha = ZERO;
    BlendOpAlpha = ADD;
    RenderTargetWriteMask[0] = 0x0F;
};

RasterizerState CullNone
{
    CullMode = NONE;
};

RasterizerState CullBack
{
    CullMode = BACK;
};

RasterizerState CullFront
{
    CullMode = FRONT;
};

//--------------------------------------------------------------------------------------
// Vertex shader output structure
//--------------------------------------------------------------------------------------
struct VS_OUTPUT
{
    float3 WorldPos     : POS;          // World-space position
    float3 OldWorldPos  : OLDPOS;       // Pre-displaced world-space position for the displacement shader
    float3 WorldNorm    : NORM;         // World-space normal
    float2 TextureUV    : TEXCOORD0;    // texture coordinates
    float2 OldTextureUV : TEXCOORD1;    // Pre-displaced texture coordnates for the displacement shader
    float4 Position     : SV_POSITION;  // Clip-space position
};

//--------------------------------------------------------------------------------------
//--------------------------------------------------------------------------------------
// VERTEX SHADERS
//--------------------------------------------------------------------------------------
//--------------------------------------------------------------------------------------

//--------------------------------------------------------------------------------------
// This generic shader computes standard transform and lighting
//--------------------------------------------------------------------------------------
VS_OUTPUT RenderSceneVS( float4 vPos : POSITION,
                         float3 vNormal : NORMAL,
                         float2 vTexCoord0 : TEXCOORD )
{
    VS_OUTPUT Output;
    
    // Transform the position from object space to homogeneous projection space
    Output.Position = mul(vPos, g_mWorldViewProjection);
    
    // Transform the normal from object space to world space
    Output.WorldNorm = normalize(mul(vNormal, (float3x3)g_mWorld)); // normalize here because not all shaders care about normalized normals
    Output.WorldPos = mul(vPos, g_mWorld);
    Output.OldWorldPos = vPos.xyz;
    Output.OldTextureUV = vTexCoord0;
    
    // Just copy the texture coordinate through
    Output.TextureUV = vTexCoord0; 
    
    return Output;    
}
    
//--------------------------------------------------------------------------------------
// Displacement based upon noise
//--------------------------------------------------------------------------------------
VS_OUTPUT RenderDisplacedVS( float4 vPos : POSITION,
                             float3 vNormal : NORMAL,
                             float2 vTexCoord0 : TEXCOORD,
                             uniform float fRollDirectionSign )
{
    VS_OUTPUT Output;
  
    // Pass world position and "old" world position through.
    // We store "old" world position because this helps with our relighting on the diffuse pass.
    Output.WorldPos = mul( vPos, g_mWorld );
    Output.OldWorldPos = vPos.xyz;
    float3 vNormalWorldSpace = normalize( mul(vNormal, (float3x3)g_mWorld) );
    
    // Just copy the texture coordinate through
    float3 tex = float3(0,vTexCoord0.y,0);
    float3 OutTex = mul( tex, (float3x3)g_mWorld );
    float2 newTex = float2(vTexCoord0.x,OutTex.y); 
    
    // Use RollNoise to get a billowing, rolling smoke look
    float3 NoisePos = Output.WorldPos;
    float NoiseOut2 = RollNoise( 4, NoisePos, newTex, fRollDirectionSign );
    
    // Switch scale based on which direction sign we use for the rolling noise
    float scale = 0.4;  // We just picked 0.4 here because it looks good.
    if( fRollDirectionSign > 0 )
    {
        scale = ( 1 - vTexCoord0.y ) * 0.8; // We just picked 0.8 here because it looks good.
    }
    // Shift the position based upon the noise
    float4 vNewPos = float4( Output.WorldPos + vNormalWorldSpace * NoiseOut2 * 0.5 * scale, 1 );
    
    // Transform the position from object space to homogeneous projection space
    if( vPos.y < -0.1 )
    {
        Output.Position = float4(0,0,0,1);
    }
    else
    {
        Output.Position = mul( vNewPos, g_mViewProjection );
    }
    Output.WorldNorm = vNormalWorldSpace;
    
    Output.TextureUV = newTex;
    Output.OldTextureUV = vTexCoord0;
    
    return Output;    
}

//--------------------------------------------------------------------------------------
//--------------------------------------------------------------------------------------
// PIXEL SHADERS
//--------------------------------------------------------------------------------------
//--------------------------------------------------------------------------------------

//--------------------------------------------------------------------------------------
// Combines several of the methods above to create a planet with an undulating
// lava surface and several islands.  This is really a stress test for how many
// procedurals we can put together and still get a framerate.
//--------------------------------------------------------------------------------------
float4 RenderLavaPlanetPS( VS_OUTPUT In, uniform bool bNormalPass ) : SV_TARGET
{ 	
    // Scale the world position to actually scale our noise instead
    float4 NoiseIn3 = float4(In.WorldPos*2,g_fTime*0.25);
    float NoiseOut3 = fBm( NoiseIn3, 8, 2.0, 0.5, NT_SIMPLEX3D );
    
    // We want there to be 40x more spheres (islands) in the scene than there would be normally
    float3 SphereIn = In.WorldPos * 40;
    
    // Wiggle the sphere coordinates by the noise we just calculated
    SphereIn.x += NoiseOut3 * 20;
    SphereIn.y += NoiseOut3 * 10;
    SphereIn.z += NoiseOut3 * 10;
    
    // Use these modified coordinates as a lookup into the sphere function
    float SphereHard = 20;
    float SphereOut = SphereGrid( SphereIn * 0.05, 0.5, SphereHard );
    
    // Add several bits of noise together at different frequencies
    float4 NoiseIn = float4( In.WorldPos * 32, g_fTime * 0.25 );
    float NoiseOut = fBm( NoiseIn, 6, 2, 0.5, NT_SIMPLEX3D );
    
    float4 NoiseIn2 = float4( In.WorldPos * 8, g_fTime * 0.1 );
    float NoiseOut2 = fBm( NoiseIn2, 8, 2, 0.5, NT_SIMPLEX4D_TURB );
    
    float4 NoiseIn4 = float4( In.WorldPos * 16, g_fTime );
    float NoiseOut4 = fBm( NoiseIn3, 4, 2, 0.5, NT_SIMPLEX3D_TURB );
    
    float4 NoiseIn5 = float4( In.WorldPos * 128, g_fTime );
    float NoiseOut5 = fBm( NoiseIn5, 2, 2.5, 0.5, NT_SIMPLEX3D_TURB );
    NoiseOut5 *= 0.5;
    
    NoiseOut = lerp( NoiseOut, NoiseOut5, NoiseOut4 );
    
    float BumpAmount = 0.02;
    float3 BumpedWorld = In.WorldPos + In.WorldNorm * NoiseOut2 * BumpAmount;
    BumpAmount = 0.005;
    float3 BumpedWorld2 = In.WorldPos + In.WorldNorm * NoiseOut * BumpAmount;
    BumpedWorld = lerp( BumpedWorld, BumpedWorld2, SphereOut );
    
    // If we're doing a normal pass, just output the bumped position
    if( bNormalPass )
    {
        return float4( BumpedWorld, 0 );
    }
    else
    {
        // If we're not doing a normal pass, either get the normal using ddx/ddy or sample it from the normal texture
        float3 bumpNorm;
        if( g_bUseDDXDDY )
            bumpNorm = FindNormal( BumpedWorld );
        else
            bumpNorm = g_NormalDepthTexture.SampleLevel( g_samPoint, In.Position.xy / g_vRenderTargetSize.xy, 0 );
        
        // Calculate the lighting from the bump
        float light = saturate( dot( bumpNorm, g_vLightDir ) );
        float light2 = light;
    
        // Lerp between red and dark grey (lit)
        // this is for lerping between the hot lava and the dark stuff floating in it
        float4 color1 = lerp( float4( 0.85, 0.07, 0.05, 1), light * float4( 0.1, 0.1, 0.1, 1 ), NoiseOut2 );
        
        // Do the land now
        float4 color2 = lerp( float4( 0.5, 0.3, 0.1, 1 ), float4( 0.6, 0.55, 0.4, 1 ), NoiseOut );
        color2 = lerp( color2, float4( 0.4, 0.0, 0.0, 1 ) , NoiseOut4 * NoiseOut4 );
        color2 *= light2;
        
        // Create a bright orange/yellow moving line in the lava
        float median = 0.12;  // Pick the value to center the line around
        float dev = 0.01;     // Pick the size (deviation) of the line
        if( NoiseOut2 > median - dev && NoiseOut2 < median + dev )
        {
            float4 color3 = float4( 1, 0.5, 0, 1);
            float lerpamount = abs( NoiseOut2 - median ) / dev;
            color1 = lerp( color3, color1, lerpamount );
        }
        
        // Create a bright red line in the lava
        median = 0.35;  // Pick the value to center the line around
        dev = 0.006;    // Pick the size (deviation) of the line
        if( NoiseOut2 > median - dev && NoiseOut2 < median + dev )
        {
            float4 color3 = float4( 1, 0, 0, 1 );
            float lerpamount = abs( NoiseOut2 - median ) / dev;
            color1 = lerp( color3, color1, lerpamount );
        }
            
        // Lerp between lava and land
        float4 Out = lerp( color2, color1, 1 - SphereOut );
        
        return Out;
    }
}

//--------------------------------------------------------------------------------------
// Simple pixel shader to test the grid
//--------------------------------------------------------------------------------------
float4 GridTest( VS_OUTPUT Input ) : SV_TARGET
{
    float3 GridIn = Input.WorldPos * 1.95;
    GridIn.y *= 2.0;
    float GridHard = 250;
    
    float GridOut = 1-StaggeredGrid( GridIn, float3( 0.04, 0.04, 0.04 ), GridHard, float3( 0, 1, 0 ), float3( 1, 0, 1 ) );
    return GridOut.rrrr;
}

//--------------------------------------------------------------------------------------
// Simple pixel shader to test noise
//--------------------------------------------------------------------------------------
float4 NoiseTest( VS_OUTPUT Input ) : SV_TARGET
{	
    float4 NoiseIn = float4( Input.WorldPos * 16, g_fTime );
    float NoiseOut = fBm( NoiseIn, 4, 2, 0.5, NT_SIMPLEX3D_TURB );
    
    return NoiseOut;
}

//--------------------------------------------------------------------------------------
// Pixel shader for noise clouds
//--------------------------------------------------------------------------------------
float4 NoiseClouds( VS_OUTPUT Input ) : SV_TARGET
{
    // Clouds are moving in the x direction
    // We get two positions here.  The PositionNext is used to help us with the shadowing.
    // PositionNext is a point along a ray from the sample position to the sun.
    // Checking PositionNext will help us determine if anything is between us and the sun.
    float3 Position = Input.WorldPos + float3( g_fTime * 0.1, 0, 0 );
    float3 PositionNext = Position + g_vLightDir * g_fSliderVal[1] * 0.05;
    
    // Scale the clouds by the slider value
    float3 Scale = float3( 8, 8, 8 );
    Position *= g_fSliderVal[0] * Scale;
    PositionNext *= g_fSliderVal[0] * Scale;
    
    // Clearness is controled by slider2
    float Clearness = g_fSliderVal[2] / 10;
    
    // Do noise for both the current sample position and the shadow position
    // The shadow position is done at a lower resolution noise (4 octaves)
    float Noise1 = CloudNoise( Position, 5, Clearness );
    float Noise2 = CloudNoise( PositionNext, 4, Clearness );
    
    // Calculating our shadowing amount
    float3 WorldNorm = -normalize( Input.WorldNorm );
    float ShadowFade = saturate( 1 - dot( WorldNorm, g_vLightDir ) );
    float NoiseShadow = ( Noise2 * ShadowFade );
        
    // Determine how cloudy we are, but do it with an x^4 falloff
    float Cloudiness = 1 - Clearness;
    float Overcast = Cloudiness * Cloudiness * Cloudiness * Cloudiness;
    
    // Calculate the background sky color based on how overcast it is
    float4 Cloudcolor = lerp( float4( 1, 1, 1, 1 ), float4( 0.1, 0.1, 0.2, 1 ), NoiseShadow * ( 1 - Overcast ) );
    float4 Bluecolor = lerp( float4( 0.2, 0.2, 0.8, 0 ), float4( 0.5, 0.5, 0.5, 0 ), Overcast );
    float4 Skycolor = lerp( Bluecolor, Cloudcolor, Noise1 );
        
    return Skycolor;
}

//--------------------------------------------------------------------------------------
// Renders a brick wall
//--------------------------------------------------------------------------------------
float4 RenderBlocks( VS_OUTPUT In, uniform bool staggered, uniform bool wiggly, uniform bool bNormalPass ) : SV_TARGET
{
    float3 WorldPos = In.WorldPos;
    
    if( wiggly )
    {
        // If we're rending wiggly blocks, perturb the input by some noise
        // before passing it to the Grid function
        float4 WiggleIn = float4( WorldPos * 1.95, g_fTime * 0.1 );
        float3 WiggleNoise = float3( 0, 0, 0 );
        WiggleNoise.x = fBm( WiggleIn, 4, 2, 0.5, NT_SIMPLEX4D );
        WiggleIn += float4( 12, 0, -8, 0 );
        WiggleNoise.y = fBm( WiggleIn, 4, 2, 0.5, NT_SIMPLEX4D );
        WiggleIn += float4( 0, -23, 22, 0 );
        WiggleNoise.z = fBm( WiggleIn, 4, 2, 0.5, NT_SIMPLEX4D );
        
        WorldPos += 0.1 * WiggleNoise;
    }
    
    // Scale the input so that our blocks are a certain size
    float3 GridIn = WorldPos * 3.85;
    GridIn.y *= 2.0;
        
    // Choose our grid pattern based upon whether we want staggered or regular
    float GridOut;
    if( staggered )
    {
        float GridHard = 500;
        float3 mortarwidth = float3( 0.07, 0.07, 0.07 );
        GridOut = StaggeredGrid( GridIn, mortarwidth, GridHard, float3( 0, 1, 0 ), float3( 1, 0, 1 ) );
    }
    else
    {
        float GridHard = 500;
        float3 mortarwidth = float3( 0.07, 0.07, 0.07 );
        GridOut = Grid( GridIn, mortarwidth, GridHard );
    }
    
    // Make our lines sharper than they would normally be
    float sqGridOut = pow( GridOut, 16 );
    sqGridOut = 1 - sqGridOut;
    GridOut = 1 - GridOut;
        
    // Bricks have little pits and nicks in them, so do that with noise that has been clamped
    float4 PitNoiseIn = float4( WorldPos*40, 0 );
    float PitNoise = fBm( PitNoiseIn, 3, 2, 0.5, NT_SIMPLEX3D );
    PitNoise = 1 - saturate( PitNoise - 0.5 );
    
    // Bricks are also bumpy
    float4 BumpNoiseIn = float4( WorldPos * 80, 0 );
    float BumpNoise = fBm( BumpNoiseIn, 3, 2, 0.5, NT_SIMPLEX3D );
    
    // Put them together
    float combinedbump = PitNoise * 0.01 + BumpNoise * 0.0001;
    float height = lerp( BumpNoise * 0.001, combinedbump, sqGridOut ) + GridOut;
    
    float BumpAmount = 4.0;
    float3 BumpedWorld = In.WorldPos + In.WorldNorm * height * BumpAmount;
        
    if( bNormalPass )
    {
        // If we're doing a normal pass, just output the bumped position
        return float4( BumpedWorld, 0 );
    }
    else
    {	
        // If we're not doing a normal pass, either get the normal using ddx/ddy or sample it from the normal texture
        float3 bump;
        if( g_bUseDDXDDY )
            bump = FindNormal( BumpedWorld );
        else
            bump = g_NormalDepthTexture.SampleLevel( g_samPoint, In.Position.xy / g_vRenderTargetSize.xy, 0 );
        
        float light = max(0.1,saturate( dot( bump, g_vLightDir ) ));
        
        // Change our coloring slightly based upon which block we are
        float OutBlock;
        if( staggered )		 
            OutBlock = StaggeredGridColor( GridIn, float3( 0, 1, 0 ), float3( 1, 0, 1 ));
        else
            OutBlock = GridColor( GridIn );
            
        // We will vary color bewteen brick and brick2 colors
        float4 brick = float4( 0.25, 0.03, 0.01, 1 );
        float4 brick2 = float4( 0.7, 0.02, 0.05, 1 );
        
        // Brick Reds
        float4 color = lerp( brick, brick2, OutBlock );
        
        // Pits
        float4 pitcolor = float4( 0.4, 0.3, 0.3, 1 );
        color = lerp( pitcolor, color, pow(PitNoise,16) );
        
        // Mortar
        float4 mortarcolor = float4( 0.7, 0.7, 0.7, 1 );
        color = lerp( mortarcolor, color, sqGridOut );
        
        return light * color;
    }
    
}

//--------------------------------------------------------------------------------------
// Renders a brick wall with holes in it
//--------------------------------------------------------------------------------------
float4 RenderBlocksHole( VS_OUTPUT In, uniform bool bNormalPass ) : SV_TARGET
{
    float3 WorldPos = In.WorldPos;
    
    // Hard-code our holes
    const float HoleRadius[2] = { 0.8, 0.6 };
    const float3 HoleCenter[2] = { float3( 1, 0.7, -1 ), float3( -0.5, 0.0, -1.0 ) };
    
    // Wiggle the input coordinates to make it look like the hole has rougher edges
    uint octaves = 3;
    float4 WiggleIn = float4( WorldPos, 0 );
    float WiggleNoise = fBm( WiggleIn, octaves, 4, 0.6, NT_SIMPLEX3D );
    
    // Add them together for the total effects
    float NoiseShift = 0.3 * WiggleNoise;
    
    // Loop through the two holes and see whether we're inside them.
    // If we are, don't draw this pixel.
    for( uint i=0; i<2; i++ )
    {
        float3 HoleDelta = WorldPos - HoleCenter[i];
        float ToCenter = dot( HoleDelta, HoleDelta ) + NoiseShift;
        if( ToCenter < HoleRadius[i] * HoleRadius[i] )
            discard;
    }
    
    float3 GridIn = WorldPos * 3.85;
    GridIn.y *= 2.0;
        
    // Now go the standard approach of creating grid lines
    float GridOut;
    float MortarLines;
    float GridHard = 500;
    float3 mortarwidth = float3( 0.07, 0.07, 0.07 );
    GridOut = StaggeredGrid( GridIn, mortarwidth, GridHard, float3( 0, 1, 0 ), float3( 1, 0, 1 ) );
    
    // Tighten up the grid lines so they're not so soft and fuzzy
    float sqGridOut = pow( GridOut, 16 );
    sqGridOut = 1 - sqGridOut;
    GridOut = 1 - GridOut;
        
    // Bricks have little pits and nicks in them, so do that with noise that has been clamped
    float4 PitNoiseIn = float4( WorldPos * 40, 0 );
    float PitNoise = fBm( PitNoiseIn, 3, 2, 0.5, NT_SIMPLEX3D );
    PitNoise = 1 - saturate( PitNoise - 0.5 );
    
    // Bricks have bumps too
    float4 BumpNoiseIn = float4( WorldPos * 80, 0 );
    float BumpNoise = fBm( BumpNoiseIn, 3, 2, 0.5, NT_SIMPLEX3D );
    
    // Combine the pits and bumps
    float combinedbump = PitNoise * 0.01 + BumpNoise * 0.0001;
    float height = lerp( BumpNoise * 0.001, combinedbump, sqGridOut ) + GridOut;
    
    float BumpAmount = 4.0;
    float3 BumpedWorld = In.WorldPos + In.WorldNorm * height * BumpAmount;
        
    if( bNormalPass )
    {
        // If we're doing a normal pass, just output the bumped position
        return float4( BumpedWorld, 0 );
    }
    else
    {
        // If we're not doing a normal pass, either get the normal using ddx/ddy or sample it from the normal texture
        float3 bump;
        if( g_bUseDDXDDY )
            bump = FindNormal( BumpedWorld );
        else
            bump = g_NormalDepthTexture.SampleLevel( g_samPoint, In.Position.xy / g_vRenderTargetSize.xy, 0 );
        
        float light = max(0.1,saturate( dot( bump, g_vLightDir ) ));
        
        // Modify the brick colors slightly
        float OutBlock = StaggeredGridColor( GridIn, float3( 0, 1, 0 ), float3( 1, 0, 1 ));
        float4 brick = float4( 0.25, 0.03, 0.01, 1 );
        float4 brick2 = float4( 0.7, 0.02, 0.05, 1 );
        
        // Brick Reds
        float4 color = lerp( brick, brick2, OutBlock );
        
        // Pits
        float4 pitcolor = float4( 0.4, 0.3, 0.3, 1 );
        color = lerp( pitcolor, color, pow( PitNoise, 16 ) );
        
        // Mortar
        float4 mortarcolor = float4( 0.7, 0.7 ,0.7 ,1 );
        color = lerp( mortarcolor, color, sqGridOut );
        
        return light * color;
    }
    
}

//--------------------------------------------------------------------------------------
// Renders wood using noise
//--------------------------------------------------------------------------------------
float4 RenderRadialWood( VS_OUTPUT In, uniform bool bNormalPass ) : SV_TARGET
{
    // As always, bias the input coordinates so that the noise has the right "size"
    float3 GridIn = In.WorldPos * 3.8;
    GridIn.y *= 0.3;
    float GridHard = 1000;
    // Use a grid to define the individual wood planks
    float GridOut = 1 - StaggeredGrid( GridIn, float3( 0.04, 0.03, 0.04 ), GridHard, float3( 1, 0, 1 ), float3( 0, 1, 0 ) );
    
    float BumpAmount = 0.01;
    float3 BumpedWorld = In.WorldPos + In.WorldNorm * GridOut * BumpAmount;
        
    if( bNormalPass )
    {
        // If we're doing a normal pass, just output the bumped position
        return float4( BumpedWorld, 0 );
    }
    else
    {
        // If we're not doing a normal pass, either get the normal using ddx/ddy or sample it from the normal texture
        float3 bump;
        if( g_bUseDDXDDY )
            bump = FindNormal( BumpedWorld );
        else
            bump = g_NormalDepthTexture.SampleLevel( g_samPoint, In.Position.xy / g_vRenderTargetSize.xy, 0 );
            
        float light = saturate( dot( bump, g_vLightDir ) );
        
        // Add some random variation on a per block (or plank) basis
        float OutBlock = StaggeredGridColor( GridIn, float3( 1, 0, 1 ), float3( 0, 1, 0 ));
        
        // First let's get some wiggly noise to randomly change where we sample the noise for the wood
        float4 WiggleNoiseIn = float4( In.WorldPos * 2, 0 );
        float WiggleNoise = fBm( WiggleNoiseIn, 2, 2, 0.5, NT_SIMPLEX3D );
        
        // Then we'll shift the center of where the noise is for that block of wood
        float3 centerPos = In.WorldPos + WiggleNoise * 0.03;
        centerPos *= 2.15;
        
        // We also want to shift the noise lookup on a per block basis 
        centerPos += OutBlock * 1.25;
        float3 Max = ceil( centerPos );
        float3 Min = floor( centerPos );
        
        // Find the distance to the center of the noise
        float Dist[4];
        Dist[0] = length( centerPos.xz - Min.xz );
        Dist[1] = length( centerPos.xz - float2( Min.x, Max.z ) );
        Dist[2] = length( centerPos.xz - float2( Max.x, Min.z ) );
        Dist[3] = length( centerPos.xz - Max.xz );
        
        float ToLine = min( min( Dist[0], Dist[1] ), min( Dist[2], Dist[3] ) );
        
        // Lookup some radial noise using the distance to this center
        float4 RadialNoiseIn = float4( ToLine.xxx * 12, 0 );
        float RadialNoise = fBm( RadialNoiseIn, 3, 2, 0.5, NT_SIMPLEX3D );
        
        // Alternate betweem some brownish wood colors
        float4 LightBrown = float4( 0.6, 0.5, 0.3, 1 );
        float4 DarkBrown = float4( 0.3, 0.2, 0.0, 1 );
        float4 colorOut = lerp( LightBrown, DarkBrown, saturate(RadialNoise) );
        
        return light * GridOut * colorOut;
    }
}

//--------------------------------------------------------------------------------------
// Colored grid
//--------------------------------------------------------------------------------------
float4 RenderGridColor( VS_OUTPUT In ) : SV_TARGET
{
    float3 GridIn = (In.WorldPos) * 3.8;
    float GridHard = 500;
    float GridOut = 1 - Grid( GridIn, float3( 0.03, 0.03, 0.03 ), GridHard );
    
    float OutGrid = GridColor( GridIn );
    
    float4 OutColor = lerp( float4( 1, 0, 0, 1 ), float4( 0, 0, 1, 1 ), OutGrid );
    return OutColor * GridOut;
}

//--------------------------------------------------------------------------------------
// Colored staggered grid
//--------------------------------------------------------------------------------------
float4 RenderStagGridColor( VS_OUTPUT In ) : SV_TARGET
{
    float3 GridIn = In.WorldPos * 3.8;
    float GridHard = 500;
    float GridOut = 1 - StaggeredGrid( GridIn, float3( 0.03, 0.03, 0.03 ), GridHard, float3( 1, 0, 1 ), float3( 0, 1, 0 ) );
    
    float OutStagger = StaggeredGridColor( GridIn, float3( 1, 0, 1 ), float3( 0, 1, 0 ) );
    
    float4 OutColor = lerp( float4( 1, 0, 0, 1 ), float4( 0, 0, 1, 1 ), OutStagger );
    return OutColor * GridOut;
}

//--------------------------------------------------------------------------------------
// Cellular Noise uses voronoi noise to generate nice cellular or lumpy textures
//--------------------------------------------------------------------------------------
float4 CellNoise( VS_OUTPUT In, uniform uint NoiseType, uniform bool DoBump, uniform bool bNormalPass ) : SV_TARGET
{
    // Scale the position to make the noise look smaller, and shift it away from the origin
    float3 NoiseIn = In.WorldPos * 10 + float3( 100, 100, 100 );
    float VNoise = VoronoiNoise( NoiseIn, g_fTime * 0.1, NoiseType );
    
    // If we're not doing bump mapping with this particular invocation, just return the noise
    if( !DoBump )
        return VNoise;
    
    float BumpAmount = 0.02;
    float3 BumpedWorld = In.WorldPos + In.WorldNorm * VNoise * BumpAmount;
        
    if( bNormalPass )
    {
        // If we're doing a normal pass, just output the bumped position
        return float4( BumpedWorld, 0 );
    }
    else
    {	
        // If we're not doing a normal pass, either get the normal using ddx/ddy or sample it from the normal texture
        float3 bump;
        if( g_bUseDDXDDY )
            bump = FindNormal( BumpedWorld );
        else
            bump = g_NormalDepthTexture.SampleLevel( g_samPoint, In.Position.xy / g_vRenderTargetSize.xy, 0 );
            
        float lighting = saturate( dot( bump, g_vLightDir) ) + 0.1;
        
        return lighting;
    }
}

//--------------------------------------------------------------------------------------
// Adding octaves of cellular noise
//--------------------------------------------------------------------------------------
float4 FractalVoronoi( VS_OUTPUT In, uniform uint NoiseType, uniform bool DoBump, uniform bool bNormalPass ) : SV_TARGET
{
    // Scale the position to make the noise look smaller, and shift it away from the origin
    float3 NoiseIn = In.WorldPos * 10 + float3( 100, 100, 100 );
    float VoronoiNoise = fBm( float4( NoiseIn, g_fTime * 0.1 ) , 4, 2, 0.5, NoiseType );
    
    if( !DoBump )
        return VoronoiNoise;
    
    // Shift and clamp the noise to make it look like like a bunch of small cracks
    VoronoiNoise = min( 1, ( VoronoiNoise + 0.3 ) );
    VoronoiNoise = 1 - pow( VoronoiNoise, 8 );
    
    float BumpAmount = 0.02;
    float3 BumpedWorld = In.WorldPos + In.WorldNorm * VoronoiNoise * BumpAmount;
        
    if( bNormalPass )
    {
        // If we're doing a normal pass, just output the bumped position
        return float4( BumpedWorld, 0 );
    }
    else
    {	
        // If we're not doing a normal pass, either get the normal using ddx/ddy or sample it from the normal texture
        float3 bump;
        if( g_bUseDDXDDY )
            bump = FindNormal( BumpedWorld );
        else
            bump = g_NormalDepthTexture.SampleLevel( g_samPoint, In.Position.xy / g_vRenderTargetSize.xy, 0 );
            
        float lighting = saturate( dot( bump, g_vLightDir) ) + 0.1;
    
        return lighting;
    }
}

//--------------------------------------------------------------------------------------
// Works with the DisplacedVS.  Just a smoky firey shader.
//--------------------------------------------------------------------------------------
float4 DisplacedPS( VS_OUTPUT In, uniform bool bNormalPass, uniform float fRollDirectionSign ) : SV_TARGET
{
    // Use the same RollNoise parameters that we used in DisplacedVS except with a different number of octaves
    float3 NoisePos = In.WorldPos;
    float NoiseOut2 = RollNoise( 6, NoisePos, In.TextureUV, fRollDirectionSign );
    
    // Scaling is different based upon the direction... just for looks.
    float scale = 0.4;
    if( fRollDirectionSign > 0 )
    {
        scale = ( 1 - In.OldTextureUV.y ) * 0.8;
    }
        
    float3 BumpedWorld = In.WorldPos + normalize(In.WorldNorm) * NoiseOut2 * 0.5 * scale;
        
    if( bNormalPass )
    {
        // If we're doing a normal pass, just output the bumped position
        return float4( BumpedWorld, 0 );
    }
    else
    {	
        // If we're not doing a normal pass, either get the normal using ddx/ddy or sample it from the normal texture
        float3 bump;
        if( g_bUseDDXDDY )
            bump = FindNormal( BumpedWorld );
        else
            bump = g_NormalDepthTexture.SampleLevel( g_samPoint, In.Position.xy / g_vRenderTargetSize.xy, 0 );
            
        float wrapfactor = 0.2;
        float ambient = 0.1;
        float lighting = ( dot( bump, g_vLightDir) + wrapfactor ) / ( 1 + wrapfactor );
        lighting = max( saturate(lighting), ambient);
        
        // Create some turbulent noise for the fire part
        float4 NoiseFireIn = float4( In.OldWorldPos * 4.0, g_fTime * 0.3 );
        float FireBlend = fBm( NoiseFireIn, 4, 2, 0.5, NT_SIMPLEX4D_TURB );
    
        // The fire color is a lerp between bright orange/yellow and red
        float4 FireColor = lerp( float4( 0.9, 0.55, 0, 1 ), float4( 1.0, 0.0, 0,0 ), FireBlend );
        
        // Use a different noise to blend different smoke colors together
        float4 NoiseSmokeIn = float4( In.OldWorldPos, g_fTime * 0.1 );
        float SmokeBlend = fBm( NoiseSmokeIn, 1, 2, 0.5, NT_SIMPLEX4D_TURB );
        
        // Get normalized height
        float NormHeight = ( NoiseOut2 * 0.5 + 0.5 );
        
        float Smoke3Start = 0.5;
        float Smoke3Amount = NormHeight / Smoke3Start;
        Smoke3Amount = pow( Smoke3Amount, 6 );
        Smoke3Amount = min( 1, Smoke3Amount );
        
        // Lerp between several different smoke colors for more variation
        float4 SmokeColor1 = float4( 0.3, 0.3, 0.3, 1 );
        float4 SmokeColor2 = float4( 0.4, 0.3, 0.2, 1 );
        float4 SmokeColor3 = float4( 0.1, 0.1, 0.1, 1 );
        float4 SmokeColor = lerp( SmokeColor1, SmokeColor2, SmokeBlend );
        SmokeColor = lerp( SmokeColor3, SmokeColor, Smoke3Amount );
        SmokeColor *= lighting;
    
        // Slider values change the appearance and amount of smoke vs fire
        float SmokeStart = g_fSliderVal[0];
        float FireToSmoke = NormHeight / SmokeStart;
        if( fRollDirectionSign > 0 )
        {
            FireToSmoke += max( 0, ( 1 - In.OldWorldPos.y ) - 0.3 );
        }
        FireToSmoke = pow( FireToSmoke, g_fSliderVal[1] );
        FireToSmoke = min( 1, FireToSmoke );
        
        // Blend smoke and fire for the final output
        return lerp( FireColor, SmokeColor, FireToSmoke );
    }
}


//--------------------------------------------------------------------------------------
//--------------------------------------------------------------------------------------
// Techniques
//--------------------------------------------------------------------------------------
//--------------------------------------------------------------------------------------


//--------------------------------------------------------------------------------------
// Creates the fullscreen normal buffer from the fullscreen position buffer.
// This must be the first technique in this file.
//--------------------------------------------------------------------------------------
technique10 CreateNormals
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, RenderQuadVS( ) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, CreateNormalsPS( ) ) );

        SetDepthStencilState( DisableDepth, 0 );
        SetBlendState( DisableBlending, float4(0,0,0,0), 0xFFFFFFFF );
        SetRasterizerState( CullBack );
    }
}

//--------------------------------------------------------------------------------------
// Combines lava-like seas with still land masses
//--------------------------------------------------------------------------------------
technique10 RenderLavaPlanet
{
    pass NORMALPASS
    {
        SetVertexShader( CompileShader( vs_4_0, RenderSceneVS( ) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, RenderLavaPlanetPS( true ) ) );

        SetDepthStencilState( EnableDepth, 0 );
        SetBlendState( DisableBlending, float4(0,0,0,0), 0xFFFFFFFF );
        SetRasterizerState( CullBack );
    }
    pass DIFFUSEPASS
    {
        SetVertexShader( CompileShader( vs_4_0, RenderSceneVS( ) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, RenderLavaPlanetPS( false ) ) );

        SetDepthStencilState( EnableDepth, 0 );
        SetBlendState( DisableBlending, float4(0,0,0,0), 0xFFFFFFFF );
        SetRasterizerState( CullBack );
    }
}

//--------------------------------------------------------------------------------------
// Renders non-staggered bricks
//--------------------------------------------------------------------------------------
technique10 RenderStraightBlocks
{
    pass NORMALPASS
    {
        SetVertexShader( CompileShader( vs_4_0, RenderSceneVS( ) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, RenderBlocks( false, false, true ) ) );

        SetDepthStencilState( EnableDepth, 0 );
        SetBlendState( DisableBlending, float4(0,0,0,0), 0xFFFFFFFF );
        SetRasterizerState( CullBack );
    }
     pass DIFFUSEPASS
    {
        SetVertexShader( CompileShader( vs_4_0, RenderSceneVS( ) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, RenderBlocks( false, false, false ) ) );

        SetDepthStencilState( EnableDepth, 0 );
        SetBlendState( DisableBlending, float4(0,0,0,0), 0xFFFFFFFF );
        SetRasterizerState( CullBack );
    }
}

//--------------------------------------------------------------------------------------
// A standard brick wall shader
//--------------------------------------------------------------------------------------
technique10 RenderStagBlocks
{
    pass NORMALPASS
    {
        SetVertexShader( CompileShader( vs_4_0, RenderSceneVS( ) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, RenderBlocks( true, false, true ) ) );

        SetDepthStencilState( EnableDepth, 0 );
        SetBlendState( DisableBlending, float4(0,0,0,0), 0xFFFFFFFF );
        SetRasterizerState( CullBack );
    }
    pass DIFFUSEPASS
    {
        SetVertexShader( CompileShader( vs_4_0, RenderSceneVS( ) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, RenderBlocks( true, false, false ) ) );

        SetDepthStencilState( EnableDepth, 0 );
        SetBlendState( DisableBlending, float4(0,0,0,0), 0xFFFFFFFF );
        SetRasterizerState( CullBack );
    }
}

//--------------------------------------------------------------------------------------
// Renders red blocks with pertubed texture coordinates which makes the block swim
//--------------------------------------------------------------------------------------
technique10 RenderWigglyBlocks
{
    pass NORMALPASS
    {
        SetVertexShader( CompileShader( vs_4_0, RenderSceneVS( ) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, RenderBlocks( true, true, true ) ) );

        SetDepthStencilState( EnableDepth, 0 );
        SetBlendState( DisableBlending, float4(0,0,0,0), 0xFFFFFFFF );
        SetRasterizerState( CullBack );
    }
     pass DIFFUSEPASS
    {
        SetVertexShader( CompileShader( vs_4_0, RenderSceneVS( ) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, RenderBlocks( true, true, false ) ) );

        SetDepthStencilState( EnableDepth, 0 );
        SetBlendState( DisableBlending, float4(0,0,0,0), 0xFFFFFFFF );
        SetRasterizerState( CullBack );
    }
}

//--------------------------------------------------------------------------------------
// Renders red blocks with holes in them
//--------------------------------------------------------------------------------------
technique10 RenderBlocksDestroyed
{
    pass NORMALPASS
    {
        SetVertexShader( CompileShader( vs_4_0, RenderSceneVS( ) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, RenderBlocksHole( true ) ) );

        SetDepthStencilState( EnableDepth, 0 );
        SetBlendState( DisableBlending, float4(0,0,0,0), 0xFFFFFFFF );
        SetRasterizerState( CullNone );
    }
     pass DIFFUSEPASS
    {
        SetVertexShader( CompileShader( vs_4_0, RenderSceneVS( ) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, RenderBlocksHole( false ) ) );

        SetDepthStencilState( EnableDepth, 0 );
        SetBlendState( DisableBlending, float4(0,0,0,0), 0xFFFFFFFF );
        SetRasterizerState( CullNone );
    }
}

//--------------------------------------------------------------------------------------
// Wood shader that also includes the lines between wooden planks
//--------------------------------------------------------------------------------------
technique10 RenderWood
{
    pass NORMALPASS
    {
        SetVertexShader( CompileShader( vs_4_0, RenderSceneVS( ) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, RenderRadialWood( true ) ) );

        SetDepthStencilState( EnableDepth, 0 );
        SetBlendState( DisableBlending, float4(0,0,0,0), 0xFFFFFFFF );
        SetRasterizerState( CullBack );
    }
    pass DIFFUSEPASS
    {
        SetVertexShader( CompileShader( vs_4_0, RenderSceneVS( ) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, RenderRadialWood( false ) ) );

        SetDepthStencilState( EnableDepth, 0 );
        SetBlendState( DisableBlending, float4(0,0,0,0), 0xFFFFFFFF );
        SetRasterizerState( CullBack );
    }
}

//--------------------------------------------------------------------------------------
// Renders a regular grid of colors
//--------------------------------------------------------------------------------------
technique10 RenderGridColors
{
    pass NORMALPASS
    {
        // No lighting for this technique
    }
    pass DIFFUSEPASS
    {
        SetVertexShader( CompileShader( vs_4_0, RenderSceneVS( ) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, RenderGridColor( ) ) );

        SetDepthStencilState( EnableDepth, 0 );
        SetBlendState( DisableBlending, float4(0,0,0,0), 0xFFFFFFFF );
        SetRasterizerState( CullBack );
    }
}

//--------------------------------------------------------------------------------------
// Renders offset blocks of colors
//--------------------------------------------------------------------------------------
technique10 RenderStagGridColors
{
    pass NORMALPASS
    {
        // No lighting for this technique
    }
    pass DIFFUSEPASS
    {
        SetVertexShader( CompileShader( vs_4_0, RenderSceneVS( ) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, RenderStagGridColor( ) ) );

        SetDepthStencilState( EnableDepth, 0 );
        SetBlendState( DisableBlending, float4(0,0,0,0), 0xFFFFFFFF );
        SetRasterizerState( CullBack );
    }
}

//--------------------------------------------------------------------------------------
// Renders a single layer of voronoi noise using the 3x3x3 method
//--------------------------------------------------------------------------------------
technique10 RenderVoronoi3
{
    pass NORMALPASS
    {
        // No lighting for this technique
    }
    pass DIFFUSEPASS
    {
        SetVertexShader( CompileShader( vs_4_0, RenderSceneVS( ) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, CellNoise( NT_VORONOI3, false, false ) ) );

        SetDepthStencilState( EnableDepth, 0 );
        SetBlendState( DisableBlending, float4(0,0,0,0), 0xFFFFFFFF );
        SetRasterizerState( CullBack );
    }
}

//--------------------------------------------------------------------------------------
// Renders a single layer of voronoi noise using the 3x3x3 second distance method
//--------------------------------------------------------------------------------------
technique10 RenderVoronoi3_2
{
    pass NORMALPASS
    {
        // No lighting for this technique
    }
    pass DIFFUSEPASS
    {
        SetVertexShader( CompileShader( vs_4_0, RenderSceneVS( ) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, CellNoise( NT_VORONOI3_2, false, false ) ) );

        SetDepthStencilState( EnableDepth, 0 );
        SetBlendState( DisableBlending, float4(0,0,0,0), 0xFFFFFFFF );
        SetRasterizerState( CullBack );
    }
}

//--------------------------------------------------------------------------------------
// Renders a single layer of voronoi noise using the 3x3x3 second distance method and bump maps it
//--------------------------------------------------------------------------------------
technique10 RenderVoronoi3_2Bump
{
    pass NORMALPASS
    {
        SetVertexShader( CompileShader( vs_4_0, RenderSceneVS( ) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, CellNoise( NT_VORONOI3_2, true, true ) ) );

        SetDepthStencilState( EnableDepth, 0 );
        SetBlendState( DisableBlending, float4(0,0,0,0), 0xFFFFFFFF );
        SetRasterizerState( CullBack );
    }
    pass DIFFUSEPASS
    {
        SetVertexShader( CompileShader( vs_4_0, RenderSceneVS( ) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, CellNoise( NT_VORONOI3_2, true, false ) ) );

        SetDepthStencilState( EnableDepth, 0 );
        SetBlendState( DisableBlending, float4(0,0,0,0), 0xFFFFFFFF );
        SetRasterizerState( CullBack );
    }
}

//--------------------------------------------------------------------------------------
// Renders a single layer of voronoi noise using the 2x2x2 method
//--------------------------------------------------------------------------------------
technique10 RenderVoronoi2
{
    pass NORMALPASS
    {
        // No lighting for this technique
    }
    pass DIFFUSEPASS
    {
        SetVertexShader( CompileShader( vs_4_0, RenderSceneVS( ) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, CellNoise( NT_VORONOI2, false, false ) ) );

        SetDepthStencilState( EnableDepth, 0 );
        SetBlendState( DisableBlending, float4(0,0,0,0), 0xFFFFFFFF );
        SetRasterizerState( CullBack );
    }
}

//--------------------------------------------------------------------------------------
// Renders a single layer of voronoi noise using the 2x2x2 second distance method
//--------------------------------------------------------------------------------------
technique10 RenderVoronoi2_2
{
    pass NORMALPASS
    {
        // No lighting for this technique
    }
    pass DIFFUSEPASS
    {
        SetVertexShader( CompileShader( vs_4_0, RenderSceneVS( ) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, CellNoise( NT_VORONOI2_2, false, false ) ) );

        SetDepthStencilState( EnableDepth, 0 );
        SetBlendState( DisableBlending, float4(0,0,0,0), 0xFFFFFFFF );
        SetRasterizerState( CullBack );
    }
}

//--------------------------------------------------------------------------------------
// Renders a single layer of voronoi noise using the 2x2x2 second distance method and bump maps it
//--------------------------------------------------------------------------------------
technique10 RenderVoronoi2_2Bump
{
    pass NORMALPASS
    {
        SetVertexShader( CompileShader( vs_4_0, RenderSceneVS( ) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, CellNoise( NT_VORONOI2_2, true, true ) ) );

        SetDepthStencilState( EnableDepth, 0 );
        SetBlendState( DisableBlending, float4(0,0,0,0), 0xFFFFFFFF );
        SetRasterizerState( CullBack );
    }
    pass DIFFUSEPASS
    {
        SetVertexShader( CompileShader( vs_4_0, RenderSceneVS( ) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, CellNoise( NT_VORONOI2_2, true, false ) ) );

        SetDepthStencilState( EnableDepth, 0 );
        SetBlendState( DisableBlending, float4(0,0,0,0), 0xFFFFFFFF );
        SetRasterizerState( CullBack );
    }
}

//--------------------------------------------------------------------------------------
// Renders octaves of voronoi noise summed together
//--------------------------------------------------------------------------------------
technique10 RenderFractalVoronoi
{
    pass NORMALPASS
    {
        // No lighting for this technique
    }
    pass DIFFUSEPASS
    {
        SetVertexShader( CompileShader( vs_4_0, RenderSceneVS( ) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, FractalVoronoi( NT_VORONOI3, false, false ) ) );

        SetDepthStencilState( EnableDepth, 0 );
        SetBlendState( DisableBlending, float4(0,0,0,0), 0xFFFFFFFF );
        SetRasterizerState( CullBack );
    }
}

//--------------------------------------------------------------------------------------
// Renders octaves of voronoi noise summed together and used as a bump map
//--------------------------------------------------------------------------------------
technique10 RenderFractalVoronoiBump
{
    pass NORMALPASS
    {
        SetVertexShader( CompileShader( vs_4_0, RenderSceneVS( ) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, FractalVoronoi( NT_VORONOI3, true, true ) ) );

        SetDepthStencilState( EnableDepth, 0 );
        SetBlendState( DisableBlending, float4(0,0,0,0), 0xFFFFFFFF );
        SetRasterizerState( CullBack );
    }
    pass DIFFUSEPASS
    {
        SetVertexShader( CompileShader( vs_4_0, RenderSceneVS( ) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, FractalVoronoi( NT_VORONOI3, true, false ) ) );

        SetDepthStencilState( EnableDepth, 0 );
        SetBlendState( DisableBlending, float4(0,0,0,0), 0xFFFFFFFF );
        SetRasterizerState( CullBack );
    }
}

//--------------------------------------------------------------------------------------
// Shows the grid function alone
//--------------------------------------------------------------------------------------
technique10 RenderGridTest
{
    pass NORMALPASS
    {
        // No lighting for this technique
    }
    pass DIFFUSEPASS
    {
        SetVertexShader( CompileShader( vs_4_0, RenderSceneVS( ) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, GridTest( ) ) );

        SetDepthStencilState( EnableDepth, 0 );
        SetBlendState( DisableBlending, float4(0,0,0,0), 0xFFFFFFFF );
        SetRasterizerState( CullBack );
    }
}

//--------------------------------------------------------------------------------------
// Simple 3D simplex noise test
//--------------------------------------------------------------------------------------
technique10 RenderNoiseTest
{
    pass NORMALPASS
    {
        // No lighting for this technique
    }
    pass DIFFUSEPASS
    {
        SetVertexShader( CompileShader( vs_4_0, RenderSceneVS( ) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, NoiseTest( ) ) );

        SetDepthStencilState( EnableDepth, 0 );
        SetBlendState( DisableBlending, float4(0,0,0,0), 0xFFFFFFFF );
        SetRasterizerState( CullBack );
    }
}

//--------------------------------------------------------------------------------------
// Renders clouds that drift along the x axis
//--------------------------------------------------------------------------------------
technique10 RenderClouds
{
    pass NORMALPASS
    {

    }
    pass DIFFUSEPASS
    {
        SetVertexShader( CompileShader( vs_4_0, RenderSceneVS( ) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, NoiseClouds( ) ) );

        SetDepthStencilState( EnableDepth, 0 );
        SetBlendState( DisableBlending, float4(0,0,0,0), 0xFFFFFFFF );
        SetRasterizerState( CullNone );
    }
}

//--------------------------------------------------------------------------------------
// Renders a displaced fireball mixed with rolling smoke.
// Load a mesh with two subsets to see two different types of rolling smoke.
// For example, try loading mcloud.sdkmesh from the media\misc folder.
//--------------------------------------------------------------------------------------
technique10 RenderDisplaced
{
    pass NORMALPASS
    {
        SetVertexShader( CompileShader( vs_4_0, RenderDisplacedVS( 1 ) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, DisplacedPS( true,  1 ) ) );

        SetDepthStencilState( EnableDepth, 0 );
        SetBlendState( DisableBlending, float4(0,0,0,0), 0xFFFFFFFF );
        SetRasterizerState( CullBack );
    }
    pass DIFFUSEPASS
    {
        SetVertexShader( CompileShader( vs_4_0, RenderDisplacedVS( 1 ) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, DisplacedPS( false, 1 ) ) );

        SetDepthStencilState( EnableDepth, 0 );
        SetBlendState( DisableBlending, float4(0,0,0,0), 0xFFFFFFFF );
        SetRasterizerState( CullBack );
    }
    
    pass NORMALPASS2
    {
        SetVertexShader( CompileShader( vs_4_0, RenderDisplacedVS( -1 ) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, DisplacedPS( true,  -1 ) ) );

        SetDepthStencilState( EnableDepth, 0 );
        SetBlendState( DisableBlending, float4(0,0,0,0), 0xFFFFFFFF );
        SetRasterizerState( CullBack );
    }
    pass DIFFUSEPASS2
    {
        SetVertexShader( CompileShader( vs_4_0, RenderDisplacedVS( -1 ) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, DisplacedPS( false, -1 ) ) );

        SetDepthStencilState( EnableDepth, 0 );
        SetBlendState( DisableBlending, float4(0,0,0,0), 0xFFFFFFFF );
        SetRasterizerState( CullBack );
    }
}