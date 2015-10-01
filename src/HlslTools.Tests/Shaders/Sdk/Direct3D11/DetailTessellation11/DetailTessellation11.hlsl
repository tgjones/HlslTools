//--------------------------------------------------------------------------------------
// File: DetailTessellation.hlsl
//
// HLSL file containing shader functions for detail tessellation.
//
// Copyright (c) Microsoft Corporation. All rights reserved.
//--------------------------------------------------------------------------------------
#include "shader_include.h"
#include "AdaptiveTessellation.h"

//--------------------------------------------------------------------------------------
// External defines
//--------------------------------------------------------------------------------------
//#define DENSITY_BASED_TESSELLATION 0
//#define DISTANCE_ADAPTIVE_TESSELLATION 0
//#define SCREEN_SPACE_ADAPTIVE_TESSELLATION 0

//--------------------------------------------------------------------------------------
// Internal defines
//--------------------------------------------------------------------------------------
#define PERPIXEL_DIFFUSE_LIGHTING 1
#define DEBUG_VIEW 0

//--------------------------------------------------------------------------------------
// Textures
//--------------------------------------------------------------------------------------
Texture2D g_DensityTexture : register( t2 );      // Height map density (only used for debug view)
                        
//--------------------------------------------------------------------------------------
// Buffer
//--------------------------------------------------------------------------------------
Buffer<float4> g_DensityBuffer : register( t0 );  // Density vertex buffer

//--------------------------------------------------------------------------------------
// Structures
//--------------------------------------------------------------------------------------
struct VS_INPUT
{
    float3 inPositionOS   : POSITION;
    float2 inTexCoord     : TEXCOORD0;
    float3 vInNormalOS    : NORMAL;
    float3 vInBinormalOS  : BINORMAL;
    float3 vInTangentOS   : TANGENT;
    
    uint   uVertexID      : SV_VERTEXID;
};

struct VS_OUTPUT_NO_TESSELLATION
{
    float2 texCoord  : TEXCOORD0;
    
#if PERPIXEL_DIFFUSE_LIGHTING==1    
    float3 vLightTS  : LIGHTVECTORTS;

#if ADD_SPECULAR==1
    float3 vViewTS   : VIEWVECTORTS;
#endif

#endif

#if PERPIXEL_DIFFUSE_LIGHTING==0 || DEBUG_VIEW>0
    float3 vDiffuseColor : COLOR;
#endif

    float4 vPosition : SV_POSITION;
};

struct VS_OUTPUT_HS_INPUT
{
    float3 vWorldPos : WORLDPOS;
    float3 vNormal   : NORMAL;
            
#if DISTANCE_ADAPTIVE_TESSELLATION==1
    float  fVertexDistanceFactor : VERTEXDISTANCEFACTOR;
#endif

    float2 texCoord  : TEXCOORD0;
    float3 vLightTS  : LIGHTVECTORTS;
    
#if ADD_SPECULAR==1
    float3 vViewTS   : VIEWVECTORTS;
#endif
};

struct HS_CONSTANT_DATA_OUTPUT
{
    float    Edges[3]         : SV_TessFactor;
    float    Inside           : SV_InsideTessFactor;
    
#if DENSITY_BASED_TESSELLATION==1 && DEBUG_VIEW>0
    float    VertexDensity[3] : VERTEX_DENSITY;
#endif
};

struct HS_CONTROL_POINT_OUTPUT
{
    float3 vWorldPos : WORLDPOS;
    float3 vNormal   : NORMAL;
    float2 texCoord  : TEXCOORD0;
    float3 vLightTS  : LIGHTVECTORTS;
#if ADD_SPECULAR==1
    float3 vViewTS   : VIEWVECTORTS; 
#endif
};


struct DS_OUTPUT
{
    float2 texCoord          : TEXCOORD0;

#if PERPIXEL_DIFFUSE_LIGHTING==1    
    float3 vLightTS          : LIGHTVECTORTS;

#if ADD_SPECULAR==1
    float3 vViewTS           : VIEWVECTORTS;
#endif

#endif

#if PERPIXEL_DIFFUSE_LIGHTING==0 || DEBUG_VIEW>0
    float3 vDiffuseColor     : COLOR;
#endif

    float4 vPosition         : SV_POSITION;
};

struct PS_INPUT
{
   float2 texCoord           : TEXCOORD0;

#if PERPIXEL_DIFFUSE_LIGHTING==1    
   float3 vLightTS           : LIGHTVECTORTS;

#if ADD_SPECULAR==1
   float3 vViewTS            : VIEWVECTORTS;
#endif

#endif

#if PERPIXEL_DIFFUSE_LIGHTING==0 || DEBUG_VIEW>0
    float3 vDiffuseColor     : COLOR;
#endif
};


//--------------------------------------------------------------------------------------
// Vertex shader: no tessellation
//--------------------------------------------------------------------------------------
VS_OUTPUT_NO_TESSELLATION VS_NoTessellation( VS_INPUT i )
{
    VS_OUTPUT_NO_TESSELLATION Out;
        
    // Compute position in world space
    float4 vPositionWS = mul( i.inPositionOS.xyz, g_mWorld );
                 
    // Compute denormalized light vector in world space
    float3 vLightWS = g_LightPosition.xyz - vPositionWS.xyz;
    // Need to invert Z for correct lighting
    vLightWS.z = -vLightWS.z;
    
    // Propagate texture coordinate through:
    Out.texCoord = i.inTexCoord * g_fBaseTextureRepeat.x;

    // Transform normal, tangent and binormal vectors from object 
    // space to homogeneous projection space and normalize them
    float3 vNormalWS   = mul( i.vInNormalOS,   (float3x3) g_mWorld );
    float3 vBinormalWS = mul( i.vInBinormalOS, (float3x3) g_mWorld );
    float3 vTangentWS  = mul( i.vInTangentOS,  (float3x3) g_mWorld );
    
    // Normalize tangent space vectors
    vNormalWS   = normalize( vNormalWS );
    vBinormalWS = normalize( vBinormalWS );
    vTangentWS  = normalize( vTangentWS );
    
    // Calculate tangent basis
    float3x3 mWorldToTangent = float3x3( vTangentWS, vBinormalWS, vNormalWS );
    
    // Calculate tangent space light vector
    float3 vLightTS = mul( mWorldToTangent, vLightWS.xyz );
    
#if PERPIXEL_DIFFUSE_LIGHTING==1
    // Per-pixel lighting
    
    // Propagate the light vector (in tangent space)
    Out.vLightTS = vLightTS;
    
#if ADD_SPECULAR==1
    // Compute and output the world view vector (unnormalized)
    float3 vViewWS = g_vEye - vPositionWS;
    Out.vViewTS  = mul( mWorldToTangent, vViewWS  );
#endif

#else
    // Per-vertex lighting

    // Calculate MIP level to fetch from
    float fHeightMapMIPLevel = clamp( ( distance( vPositionWS.xyz, g_vEye ) - 100.0f ) / 100.0f, 0.0f, 6.0f );

    // Fetch normal from normal map
    float4 vNormalHeight = g_nmhTexture.SampleLevel( g_samLinear, Out.texCoord, fHeightMapMIPLevel );
    
    // Calculate diffuse lighting
    float3 vNormalTS = normalize( vNormalHeight.xyz * 2.0 - 1.0 );
    vLightTS = normalize( vLightTS );
    Out.vDiffuseColor = saturate( dot( vNormalTS, vLightTS ) ) * g_materialDiffuseColor;
#endif    

    // Compute position in clipping space
    Out.vPosition = mul( float4( i.inPositionOS.xyz, 1.0 ), g_mWorldViewProjection );
    
    return Out;
}   


//--------------------------------------------------------------------------------------
// Vertex shader: detail tessellation
//--------------------------------------------------------------------------------------
VS_OUTPUT_HS_INPUT VS( VS_INPUT i )
{
    VS_OUTPUT_HS_INPUT Out;
    
    // Compute position in world space
    float4 vPositionWS = mul( i.inPositionOS.xyz, g_mWorld );
                 
    // Compute denormalized light vector in world space
    float3 vLightWS = g_LightPosition.xyz - vPositionWS.xyz;
    // Need to invert Z for correct lighting
    vLightWS.z = -vLightWS.z;
    
    // Propagate texture coordinate through:
    Out.texCoord = i.inTexCoord * g_fBaseTextureRepeat.x;

    // Transform normal, tangent and binormal vectors from object 
    // space to homogeneous projection space and normalize them
    float3 vNormalWS   = mul( i.vInNormalOS,   (float3x3) g_mWorld );
    float3 vTangentWS  = mul( i.vInTangentOS,  (float3x3) g_mWorld );
    float3 vBinormalWS = mul( i.vInBinormalOS, (float3x3) g_mWorld );
    
    // Normalize tangent space vectors
    vNormalWS   = normalize( vNormalWS );
    vTangentWS  = normalize( vTangentWS );
    vBinormalWS = normalize( vBinormalWS );
    
    // Output normal
    Out.vNormal = vNormalWS;
    
    // Calculate tangent basis
    float3x3 mWorldToTangent = float3x3( vTangentWS, vBinormalWS, vNormalWS );
        
    // Propagate the light vector (in tangent space)
    Out.vLightTS = mul( mWorldToTangent, vLightWS );
    
#if ADD_SPECULAR==1
    // Compute and output the world view vector (unnormalized)
    float3 vViewWS = g_vEye - vPositionWS;
    Out.vViewTS  = mul( mWorldToTangent, vViewWS  );
#endif

    // Write world position
    Out.vWorldPos = float3( vPositionWS.xyz );
    
#if DISTANCE_ADAPTIVE_TESSELLATION==1
    // Min and max distance should be chosen according to scene quality requirements
    const float fMinDistance = 20.0f;
    const float fMaxDistance = 250.0f;  

    // Calculate distance between vertex and camera, and a vertex distance factor issued from it
    float fDistance = distance( vPositionWS.xyz, g_vEye );
    Out.fVertexDistanceFactor = 1.0 - clamp( ( ( fDistance - fMinDistance ) / ( fMaxDistance - fMinDistance ) ), 
                                             0.0, 1.0 - g_vTessellationFactor.z/g_vTessellationFactor.x);
#endif

    return Out;
}   


//--------------------------------------------------------------------------------------
// Hull shader
//--------------------------------------------------------------------------------------
HS_CONSTANT_DATA_OUTPUT ConstantsHS( InputPatch<VS_OUTPUT_HS_INPUT, 3> p, uint PatchID : SV_PrimitiveID )
{
    HS_CONSTANT_DATA_OUTPUT output  = (HS_CONSTANT_DATA_OUTPUT)0;
    float4 vEdgeTessellationFactors = g_vTessellationFactor.xxxy;
    
#if DISTANCE_ADAPTIVE_TESSELLATION==1

    // Calculate edge scale factor from vertex scale factor: simply compute 
    // average tess factor between the two vertices making up an edge
    vEdgeTessellationFactors.x = 0.5 * ( p[1].fVertexDistanceFactor + p[2].fVertexDistanceFactor );
    vEdgeTessellationFactors.y = 0.5 * ( p[2].fVertexDistanceFactor + p[0].fVertexDistanceFactor );
    vEdgeTessellationFactors.z = 0.5 * ( p[0].fVertexDistanceFactor + p[1].fVertexDistanceFactor );
    vEdgeTessellationFactors.w = vEdgeTessellationFactors.x;

    // Multiply them by global tessellation factor
    vEdgeTessellationFactors *= g_vTessellationFactor.xxxy;
    
#elif SCREEN_SPACE_ADAPTIVE_TESSELLATION==1

    // Get the screen space position of each control point
    float2 f2EdgeScreenPosition0 = 
        GetScreenSpacePosition( p[0].vWorldPos.xyz, g_mViewProjection, g_vScreenResolution.x, g_vScreenResolution.y );
    float2 f2EdgeScreenPosition1 = 
        GetScreenSpacePosition( p[1].vWorldPos.xyz, g_mViewProjection, g_vScreenResolution.x, g_vScreenResolution.y );
    float2 f2EdgeScreenPosition2 = 
        GetScreenSpacePosition( p[2].vWorldPos.xyz, g_mViewProjection, g_vScreenResolution.x, g_vScreenResolution.y );
    
    // Calculate edge tessellation factors based on desired screen space tessellation value
    vEdgeTessellationFactors.x = g_vTessellationFactor.w * distance(f2EdgeScreenPosition2, f2EdgeScreenPosition1);
    vEdgeTessellationFactors.y = g_vTessellationFactor.w * distance(f2EdgeScreenPosition2, f2EdgeScreenPosition0);
    vEdgeTessellationFactors.z = g_vTessellationFactor.w * distance(f2EdgeScreenPosition0, f2EdgeScreenPosition1);
    vEdgeTessellationFactors.w = 0.33 * ( vEdgeTessellationFactors.x + vEdgeTessellationFactors.y + vEdgeTessellationFactors.z );
    
#endif

#if DENSITY_BASED_TESSELLATION==1

    // Retrieve edge density from edge density buffer (swizzle required to match vertex ordering)
    vEdgeTessellationFactors *= g_DensityBuffer.Load( PatchID ).yzxw;
    
#endif
    
    // Assign tessellation levels
    output.Edges[0] = vEdgeTessellationFactors.x;
    output.Edges[1] = vEdgeTessellationFactors.y;
    output.Edges[2] = vEdgeTessellationFactors.z;
    output.Inside   = vEdgeTessellationFactors.w;
    
#if FRUSTUM_CULLING_OPTIMIZATION==1
    // View frustum culling
    bool bViewFrustumCull = ViewFrustumCull( p[0].vWorldPos, p[1].vWorldPos, p[2].vWorldPos, g_vFrustumPlaneEquation,
                                             g_vDetailTessellationHeightScale.x );
    if (bViewFrustumCull)
    {
        // Set all tessellation factors to 0 if frustum cull test succeeds
        output.Edges[0] = 0.0;
        output.Edges[1] = 0.0;
        output.Edges[2] = 0.0;
        output.Inside   = 0.0;
    }
#endif

    
    return output;
}

[domain("tri")]
[partitioning("fractional_odd")]
[outputtopology("triangle_cw")]
[outputcontrolpoints(3)]
[patchconstantfunc("ConstantsHS")]
[maxtessfactor(15.0)]
HS_CONTROL_POINT_OUTPUT HS( InputPatch<VS_OUTPUT_HS_INPUT, 3> inputPatch, uint uCPID : SV_OutputControlPointID )
{
    HS_CONTROL_POINT_OUTPUT    output = (HS_CONTROL_POINT_OUTPUT)0;
    
    // Copy inputs to outputs
    output.vWorldPos = inputPatch[uCPID].vWorldPos.xyz;
    output.vNormal =   inputPatch[uCPID].vNormal;
    output.texCoord =  inputPatch[uCPID].texCoord;
    output.vLightTS =  inputPatch[uCPID].vLightTS;
#if ADD_SPECULAR==1
    output.vViewTS =   inputPatch[uCPID].vViewTS;
#endif

    return output;
}


//--------------------------------------------------------------------------------------
// Domain Shader
//--------------------------------------------------------------------------------------
[domain("tri")]
DS_OUTPUT DS( HS_CONSTANT_DATA_OUTPUT input, float3 BarycentricCoordinates : SV_DomainLocation, 
             const OutputPatch<HS_CONTROL_POINT_OUTPUT, 3> TrianglePatch )
{
    DS_OUTPUT output = (DS_OUTPUT)0;

    // Interpolate world space position with barycentric coordinates
    float3 vWorldPos = BarycentricCoordinates.x * TrianglePatch[0].vWorldPos + 
                       BarycentricCoordinates.y * TrianglePatch[1].vWorldPos + 
                       BarycentricCoordinates.z * TrianglePatch[2].vWorldPos;
    
    // Interpolate world space normal and renormalize it
    float3 vNormal = BarycentricCoordinates.x * TrianglePatch[0].vNormal + 
                     BarycentricCoordinates.y * TrianglePatch[1].vNormal + 
                     BarycentricCoordinates.z * TrianglePatch[2].vNormal;
    vNormal = normalize( vNormal );
    
    // Interpolate other inputs with barycentric coordinates
    output.texCoord = BarycentricCoordinates.x * TrianglePatch[0].texCoord + 
                      BarycentricCoordinates.y * TrianglePatch[1].texCoord + 
                      BarycentricCoordinates.z * TrianglePatch[2].texCoord;
    float3 vLightTS = BarycentricCoordinates.x * TrianglePatch[0].vLightTS + 
                      BarycentricCoordinates.y * TrianglePatch[1].vLightTS + 
                      BarycentricCoordinates.z * TrianglePatch[2].vLightTS;

    // Calculate MIP level to fetch normal from
    float fHeightMapMIPLevel = clamp( ( distance( vWorldPos, g_vEye ) - 100.0f ) / 100.0f, 0.0f, 3.0f);
    
    // Sample normal and height map
    float4 vNormalHeight = g_nmhTexture.SampleLevel( g_samLinear, output.texCoord, fHeightMapMIPLevel );
    
    // Displace vertex along normal
    vWorldPos += vNormal * ( g_vDetailTessellationHeightScale.x * ( vNormalHeight.w-1.0 ) );
    
    // Transform world position with viewprojection matrix
    output.vPosition = mul( float4( vWorldPos.xyz, 1.0 ), g_mViewProjection );
    
#if PERPIXEL_DIFFUSE_LIGHTING==1
    // Per-pixel lighting: pass tangent space light vector to pixel shader
    output.vLightTS = vLightTS;
    
#if ADD_SPECULAR==1
    // Also pass tangent space view vector
    output.vViewTS  = BarycentricCoordinates.x * TrianglePatch[0].vViewTS + 
                      BarycentricCoordinates.y * TrianglePatch[1].vViewTS + 
                      BarycentricCoordinates.z * TrianglePatch[2].vViewTS;
#endif

#else

    // Per-vertex lighting
    float3 vNormalTS = normalize( vNormalHeight.xyz * 2.0 - 1.0 );
    vLightTS = normalize( vLightTS );
    output.vDiffuseColor = saturate( dot( vNormalTS, vLightTS ) ) * g_materialDiffuseColor;

#endif

#if DENSITY_BASED_TESSELLATION==1 && DEBUG_VIEW==1 && PERPIXEL_DIFFUSE_LIGHTING==1
    output.vDiffuseColor = BarycentricCoordinates.x*input.VertexDensity[0] + 
                           BarycentricCoordinates.y*input.VertexDensity[1] + 
                           BarycentricCoordinates.z*input.VertexDensity[2];
#endif

#if DEBUG_VIEW==2
    float fRed =            saturate(   fHeightMapMIPLevel             );
    float fGreen =          saturate( ( fHeightMapMIPLevel-1.0 ) / 2.0 );
    float fBlue =           saturate( ( fHeightMapMIPLevel-2.0 ) / 4.0 );
    output.vDiffuseColor =  float4(fRed, fGreen, fBlue,0);
#endif
    
    return output;
}


//--------------------------------------------------------------------------------------
// Bump mapping shader
//--------------------------------------------------------------------------------------
float4 BumpMapPS( PS_INPUT i ) : SV_TARGET
{ 
    float4 cResultColor = float4( 0, 0, 0, 1 );
    float3 vViewTS = float3( 0, 0, 0 );
    
#if PERPIXEL_DIFFUSE_LIGHTING==1

    // Normalize tangent space light vector
    float3 vLightTS = normalize( i.vLightTS );

#if ADD_SPECULAR==1
    // Normalize tangent space view vector
    vViewTS = normalize( i.vViewTS );
#endif

    // Compute resulting color for the pixel:
    cResultColor = ComputeIllumination( i.texCoord, vLightTS, vViewTS );
   
#else

    // Sample base map:
    float4 cBaseColor = g_baseTexture.Sample( g_samLinear, i.texCoord );
    
    // Composite the final color:
    float3 cFinalColor = ( g_materialAmbientColor + i.vDiffuseColor ) * cBaseColor; 

    // Per tessellated vertex diffuse lighting: simply interpolate diffuse color at each tessellated vertex
    cResultColor = float4( cFinalColor, 1.0 );
    
#endif

#if DENSITY_BASED_TESSELLATION==1 && DEBUG_VIEW==1
    float fDensity = g_DensityTexture.SampleLevel( g_samLinear, i.texCoord, 0 ).x;
    cResultColor.xyz = cResultColor.xyz*g_vDebugColorMultiply.xyz + fDensity*g_vDebugColorAdd.xyz;
#endif

#if DEBUG_VIEW==2
    cResultColor.xyz = cResultColor.xyz*g_vDebugColorMultiply.xyz + i.vDiffuseColor.xyz*g_vDebugColorAdd.xyz;
#endif
   
   // Return color
   return cResultColor;
}   


