//--------------------------------------------------------------------------------------
// File: SubD10.fx
//
// This effect file handles the reconstruction of the bicubic surface for the SubD10
// sample.
// 
// Copyright (c) Microsoft Corporation. All rights reserved.
//--------------------------------------------------------------------------------------
#include "CreateTangentPatch.fxh"

//--------------------------------------------------------------------------------------
// States
//--------------------------------------------------------------------------------------
DepthStencilState EnableDepth
{
    DepthEnable = TRUE;
    DepthWriteMask = ALL;
    DepthFunc = LESS_EQUAL;
};

RasterizerState CullNoneWire
{
    CullMode = NONE;
    FillMode = WIREFRAME;
};

RasterizerState CullBackWire
{
    CullMode = BACK;
    FillMode = WIREFRAME;
};

RasterizerState CullNoneSolid
{
    CullMode = NONE;
    FillMode = SOLID;
};

RasterizerState CullBackSolid
{
    CullMode = BACK;
    FillMode = SOLID;
};

RasterizerState CullFrontSolid
{
    CullMode = FRONT;
    FillMode = SOLID;
};

BlendState NoBlending
{
    BlendEnable[0] = FALSE;
};

//--------------------------------------------------------------------------------------
// Textures and buffers
//--------------------------------------------------------------------------------------
Texture2D       g_txHeight;           // Height and Bump texture
Buffer<float4>  g_bufPatchesB;        // Bicubic patch buffer
Buffer<float4>  g_bufPatchesUV;       // Tangent UV patch buffer
Buffer<float4>  g_bufControlPointsUV; // Control point buffer

//--------------------------------------------------------------------------------------
// Samplers
//--------------------------------------------------------------------------------------
SamplerState g_samLinear
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

//--------------------------------------------------------------------------------------
// constant buffers
//--------------------------------------------------------------------------------------
cbuffer cbPerFrame
{
    float    g_fTime;                   // App's time in seconds
};

cbuffer cbPerDraw
{
    float4x4 g_mWorld;                  // World matrix for object
    float4x4 g_mWorldViewProjection;    // World * View * Projection matrix
    float4   g_vEyeDir;
};

cbuffer cbUser
{
    float    g_fHeightAmount = 0.2f;
};

cbuffer cbConstantData
{
    const float4 g_DiffuseColor = float4( 0.8, 0.1, 0.15, 1 );
    const float3 g_vLightDir = float3( 0, 1, 0 );
};

cbuffer cbPerPatchType
{
    uint g_PatchIDOffset;
};

//--------------------------------------------------------------------------------------
// Vertex shader output structure
//--------------------------------------------------------------------------------------
struct VS_INPUT_PATCH
{
    float2 UV       : PATCHUV;          // Parametric coordinate for the precomputed patch
    float4 BasisU   : BASISU;           // U value of the Bernstein basis function at this UV
    float4 BasisV   : BASISV;           // V value of the Bernstein basis function at this UV
    float4 dBasisU  : DBASISU;          // U value of the derivative Bernstein basis function at this UV
    float4 dBasisV  : DBASISV;          // V value of the derivative Bernstein basis function at this UV
    uint InstanceID : SV_INSTANCEID;    // InstanceID is used to fetch the appropriate patch parameters
};

struct VS_OUTPUT
{
    float3 Normal     : NORMAL;
    float2 TextureUV  : TEXCOORD0;
    float3 Tangent    : TANGENT;
    float3 BiTangent  : BITANGENT;
    float4 Position   : SV_POSITION;
};

struct PS_INPUT
{
    float3 Normal     : NORMAL;
    float2 TextureUV  : TEXCOORD0;
    float3 Tangent    : TANGENT;
    float3 BiTangent  : BITANGENT;
};

//--------------------------------------------------------------------------------------
// Evaluates a cubic bezier patch from the precalc patch data passed in
//--------------------------------------------------------------------------------------
float3 EvaluateCubicBezierPatchPreCalc( in VS_INPUT_PATCH input, float3 ControlPoint[16] )
{
    float BU[4];
    float BV[4];
    
    BU[0] = input.BasisU.x;
    BU[1] = input.BasisU.y;
    BU[2] = input.BasisU.z;
    BU[3] = input.BasisU.w;
    BV[0] = input.BasisV.x;
    BV[1] = input.BasisV.y;
    BV[2] = input.BasisV.z;
    BV[3] = input.BasisV.w;
    
    float3 output = float3(0,0,0);
    output  = BV[0] * ( ControlPoint[0].xyz * BU[0] + ControlPoint[1].xyz * BU[1] + ControlPoint[2].xyz * BU[2] + ControlPoint[3].xyz * BU[3] );
    output += BV[1] * ( ControlPoint[4].xyz * BU[0] + ControlPoint[5].xyz * BU[1] + ControlPoint[6].xyz * BU[2] + ControlPoint[7].xyz * BU[3] );
    output += BV[2] * ( ControlPoint[8].xyz * BU[0] + ControlPoint[9].xyz * BU[1] + ControlPoint[10].xyz * BU[2] + ControlPoint[11].xyz * BU[3] );
    output += BV[3] * ( ControlPoint[12].xyz * BU[0] + ControlPoint[13].xyz * BU[1] + ControlPoint[14].xyz * BU[2] + ControlPoint[15].xyz * BU[3] );
    
    
    return output;
}

//--------------------------------------------------------------------------------------
// Evaluates a cubic bezier patch tangent from the precalc patch data passed in
//--------------------------------------------------------------------------------------
float3 EvaluateCubicBezierPatchTangentPreCalc( in VS_INPUT_PATCH input, float3 ControlPoint[16] )
{
    float BU[4];
    float BV[4];
    
    BU[0] = input.dBasisU.x;
    BU[1] = input.dBasisU.y;
    BU[2] = input.dBasisU.z;
    BU[3] = input.dBasisU.w;
    BV[0] = input.BasisV.x;
    BV[1] = input.BasisV.y;
    BV[2] = input.BasisV.z;
    BV[3] = input.BasisV.w;
    
    float3 output = float3(0,0,0);
    output  = BV[0] * ( ControlPoint[0].xyz * BU[0] + ControlPoint[1].xyz * BU[1] + ControlPoint[2].xyz * BU[2] + ControlPoint[3].xyz * BU[3] );
    output += BV[1] * ( ControlPoint[4].xyz * BU[0] + ControlPoint[5].xyz * BU[1] + ControlPoint[6].xyz * BU[2] + ControlPoint[7].xyz * BU[3] );
    output += BV[2] * ( ControlPoint[8].xyz * BU[0] + ControlPoint[9].xyz * BU[1] + ControlPoint[10].xyz * BU[2] + ControlPoint[11].xyz * BU[3] );
    output += BV[3] * ( ControlPoint[12].xyz * BU[0] + ControlPoint[13].xyz * BU[1] + ControlPoint[14].xyz * BU[2] + ControlPoint[15].xyz * BU[3] );
    
    
    return output;
}

//--------------------------------------------------------------------------------------
// Evaluates a cubic bezier patch bitangent from the precalc patch data passed in
//--------------------------------------------------------------------------------------
float3 EvaluateCubicBezierPatchBiTangentPreCalc( in VS_INPUT_PATCH input, float3 ControlPoint[16] )
{
    float BU[4];
    float BV[4];
    
    BU[0] = input.BasisU.x;
    BU[1] = input.BasisU.y;
    BU[2] = input.BasisU.z;
    BU[3] = input.BasisU.w;
    BV[0] = input.dBasisV.x;
    BV[1] = input.dBasisV.y;
    BV[2] = input.dBasisV.z;
    BV[3] = input.dBasisV.w;
    
    float3 output = float3(0,0,0);
    output  = BV[0] * ( ControlPoint[0].xyz * BU[0] + ControlPoint[1].xyz * BU[1] + ControlPoint[2].xyz * BU[2] + ControlPoint[3].xyz * BU[3] );
    output += BV[1] * ( ControlPoint[4].xyz * BU[0] + ControlPoint[5].xyz * BU[1] + ControlPoint[6].xyz * BU[2] + ControlPoint[7].xyz * BU[3] );
    output += BV[2] * ( ControlPoint[8].xyz * BU[0] + ControlPoint[9].xyz * BU[1] + ControlPoint[10].xyz * BU[2] + ControlPoint[11].xyz * BU[3] );
    output += BV[3] * ( ControlPoint[12].xyz * BU[0] + ControlPoint[13].xyz * BU[1] + ControlPoint[14].xyz * BU[2] + ControlPoint[15].xyz * BU[3] );
    
    return output;
}

//--------------------------------------------------------------------------------------
// Load a patch from a buffer
//--------------------------------------------------------------------------------------
void LoadPatch( in uint patchID, out float3 Bez[16], out float3 TanU[16], out float3 TanV[16], uniform bool bTangentPatches )
{
    // Load from the full form
    uint offset = 16*patchID;
    for( uint i=0; i<16; i++ )
    {
        Bez[i] =  g_bufPatchesB.Load( offset + i ).xyz;
    }
    
    if( bTangentPatches )
    {
        offset = 9*patchID;
        TanV[0]  = g_bufPatchesUV.Load( offset ).xyz;
        TanV[3]  = g_bufPatchesUV.Load( offset + 1 ).xyz;
        TanV[15] = g_bufPatchesUV.Load( offset + 2 ).xyz;
        TanV[12] = g_bufPatchesUV.Load( offset + 3 ).xyz;
        
        TanU[0]  = g_bufPatchesUV.Load( offset + 4).xyz;
        TanU[3]  = g_bufPatchesUV.Load( offset + 5 ).xyz;
        TanU[15] = g_bufPatchesUV.Load( offset + 6 ).xyz;
        TanU[12] = g_bufPatchesUV.Load( offset + 7 ).xyz;
        
        float4 weights = g_bufPatchesUV.Load( offset + 8 );
        float fCWts[4];
        fCWts[0] = weights.x;
        fCWts[1] = weights.y;
        fCWts[2] = weights.z;
        fCWts[3] = weights.w;

        float3 vCorner[4];
        float3 vCornerLocal[4];
        
        vCorner[0] = TanV[0];
        vCorner[1] = TanV[3];
        vCorner[2] = TanV[15];
        vCorner[3] = TanV[12];
        vCornerLocal[0] = TanU[0];
        vCornerLocal[1] = TanU[3];
        vCornerLocal[2] = TanU[12];
        vCornerLocal[3] = TanU[15];
    
        ComputeTanPatch(Bez,TanU,fCWts,vCorner,vCornerLocal,1,4);

        fCWts[3] = weights.y;
        fCWts[1] = weights.w;

        vCorner[0] = TanU[0];
        vCorner[3] = TanU[3];
        vCorner[2] = TanU[15];
        vCorner[1] = TanU[12];
        vCornerLocal[0] = TanV[0];
        vCornerLocal[1] = TanV[12];
        vCornerLocal[2] = TanV[3];
        vCornerLocal[3] = TanV[15];

        ComputeTanPatch(Bez,TanV,fCWts,vCorner,vCornerLocal,4,1);
    }
}

//--------------------------------------------------------------------------------------
// Evaluates the patch.  This involves loading the patch and evaluating both tangent and
// bitangent.
//--------------------------------------------------------------------------------------
void EvaluatePatchPreCalc( in VS_INPUT_PATCH input, 
                           out float3 WorldPos, out float3 Tangent, out float3 BiTangent, 
                           uniform bool bTangentPatches )
{
    // get the coefficients for the cubic bezier patch
    float3 ControlPoints[16];
    float3 ControlPointsU[16];
    float3 ControlPointsV[16];
    
    LoadPatch( input.InstanceID + g_PatchIDOffset, ControlPoints, ControlPointsU, ControlPointsV, bTangentPatches );
               
    float2 UV = input.UV;
    
    // evaluate the patch
    WorldPos = EvaluateCubicBezierPatchPreCalc( input, ControlPoints ).xyz;
    
    if( bTangentPatches )
    {
        Tangent = EvaluateCubicBezierPatchPreCalc( input, ControlPointsU ).xyz;
        BiTangent = EvaluateCubicBezierPatchPreCalc( input, ControlPointsV ).xyz;
    }
    else
    {
        Tangent = normalize( EvaluateCubicBezierPatchTangentPreCalc( input, ControlPoints ) ).xyz;
        BiTangent = normalize( EvaluateCubicBezierPatchBiTangentPreCalc( input, ControlPoints ) ).xyz;
    }
}

//--------------------------------------------------------------------------------------
// Render bezier patches and optionally displaces and textures
//--------------------------------------------------------------------------------------
VS_OUTPUT RenderSceneVS( VS_INPUT_PATCH input, 
                         uniform bool bDisplacement, 
                         uniform bool bTangentPatches, 
                         uniform bool bTexture, 
                         uniform bool bTangent )
{
    float2 UV = input.UV;
    uint patchID = input.InstanceID + g_PatchIDOffset;

    float3 WorldPos;
    float3 Tangent;
    float3 BiTangent;
    
    EvaluatePatchPreCalc( input, 
                          WorldPos, Tangent, BiTangent, 
                          bTangentPatches );
         
    float3 norm = normalize( cross( Tangent, BiTangent ) );

    // Tangent vectors have a stride of 5 since UV data and tangent data are stored in the same buffer
    uint patchOffset = patchID * 5;
    
    VS_OUTPUT Output;
    Output.Normal = mul( norm, (float3x3)g_mWorld );
    
    if( bTangent )
    {	
        // Evalulate the tangent vectors through bilinear interpolation.
        // These tangents are the texture-space tangents.  They should not be confused with the parametric
        // tangents that we use to get the normals for the bicubic patch.
        float4 TextureTanU0 = g_bufControlPointsUV.Load( patchOffset + 2 );
        float4 TextureTanU1 = g_bufControlPointsUV.Load( patchOffset + 3 );
        float4 TextureTanU2 = g_bufControlPointsUV.Load( patchOffset + 4 );
        float4 TextureTanU3 = float4( TextureTanU0.w, TextureTanU1.w, TextureTanU2.w, 1 );
        
        float4 UVbottom = lerp( TextureTanU0, TextureTanU1, UV.x );
        float4 UVtop = lerp( TextureTanU3, TextureTanU2, UV.x );
        float4 Tan = lerp( UVbottom, UVtop, UV.y );
    
        Output.Tangent = mul( Tan.xyz, (float3x3)g_mWorld );

        // This is an optimization.  We assume that the UV mapping of the mesh will result in a "relatively" orthogonal
        // tangent basis.  If we assume this, then we can avoid fetching and bilerping the BiTangent along with the tangent.
        Output.BiTangent = cross( Output.Normal, Output.Tangent );
    }
    
    if( bTexture )
    {
        // Evalulate the texture coordinates through bilinear interpolation
        float4 tex01 = g_bufControlPointsUV.Load( patchOffset     );
        float4 tex23 = g_bufControlPointsUV.Load( patchOffset + 1 );
        
        float2 tex0 = tex01.xy;
        float2 tex1 = tex01.zw;
        float2 tex2 = tex23.xy;
        float2 tex3 = tex23.zw;
        
        float2 bottom = lerp( tex0, tex1, UV.x );
        float2 top = lerp( tex3, tex2, UV.x );
        UV = lerp( bottom, top, UV.y );
        UV.y = 1 - UV.y;  
    }
    
    if( bDisplacement )
    {
        // On this sample displacement can go into or out of the mesh.  This is why we bias the heigh amount.
        float height = g_fHeightAmount*(g_txHeight.SampleLevel( g_samPoint, UV, 0 ).a * 2 - 1);
        float3 WorldPosMiddle = norm * height;
        WorldPos += WorldPosMiddle;
    }
    Output.Position = mul( float4(WorldPos,1), g_mWorldViewProjection );
    Output.TextureUV = UV;
    
    return Output;    
}

//--------------------------------------------------------------------------------------
// Generic pixel shader.
//--------------------------------------------------------------------------------------
float4 RenderScenePS( PS_INPUT In, uniform bool Wires, uniform bool bTexture, uniform bool bTangent ) : SV_TARGET
{ 
    if( Wires )
    {
        return float4(0,0,0,1);
    }
    else
    {			
        float3 NormNormal = normalize( In.Normal );
        float4 diffuse = g_DiffuseColor;
            
        if(bTexture)
        {
            diffuse = g_txHeight.Sample( g_samLinear, In.TextureUV );	
        }
        
        if(bTangent)
        {	
            float3 NormTangent = normalize( In.Tangent );
            float3 NormBiTangent = normalize( In.BiTangent );
            
            float3x3 TBN = float3x3( NormTangent, NormBiTangent, NormNormal );
            
            float3 bump = diffuse.xyz * 2 - 1;
            
            NormNormal = normalize( mul( bump, TBN ) );
            diffuse = g_DiffuseColor;
        }

        float3 LightDir = g_vLightDir;
        
        float lighting = max( dot( NormNormal, LightDir ), 0.3);
        float3 halfAngle = normalize( -g_vEyeDir.xyz + LightDir );
        float specular = pow( max(0,dot( halfAngle, NormNormal )), 16 );
        
        float4 color = diffuse * lighting;
        
        if( !bTangent )
        {
            color += float4(specular.rrr,0);
        }
        
        return color;
    }
}


//--------------------------------------------------------------------------------------
// Renders the scene using the basic ACC conversion without the tangent patches.
// The shading will likely have discontinuities at the edges of extraordinary patches.
//--------------------------------------------------------------------------------------
technique10 RenderScene
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, RenderSceneVS( false, false, false, false ) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, RenderScenePS( false, false, false ) ) );

        SetBlendState( NoBlending, float4(0,0,0,0), 0xffffffff );
        SetDepthStencilState( EnableDepth, 0 );
        SetRasterizerState( CullBackSolid );
    }
    
    pass P1
    {
        SetVertexShader( CompileShader( vs_4_0, RenderSceneVS( false, false, false, false ) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, RenderScenePS( true, false, false ) ) );

        SetBlendState( NoBlending, float4(0,0,0,0), 0xffffffff );
        SetDepthStencilState( EnableDepth, 0 );
        SetRasterizerState( CullBackWire );
    }
}

//--------------------------------------------------------------------------------------
// Renders the scene using the ACC conversion with the tangent patches.
// The shading will be smooth at the edges of extraordinary patches.
//--------------------------------------------------------------------------------------
technique10 RenderSceneTan
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, RenderSceneVS( false, true, false, false ) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, RenderScenePS( false, false, false ) ) );

        SetBlendState( NoBlending, float4(0,0,0,0), 0xffffffff );
        SetDepthStencilState( EnableDepth, 0 );
        SetRasterizerState( CullBackSolid );
    }
    
    pass P1
    {
        SetVertexShader( CompileShader( vs_4_0, RenderSceneVS( false, true, false, false ) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, RenderScenePS( true, false, false ) ) );

        SetBlendState( NoBlending, float4(0,0,0,0), 0xffffffff );
        SetDepthStencilState( EnableDepth, 0 );
        SetRasterizerState( CullBackWire );
    }
}

//--------------------------------------------------------------------------------------
// Renders the scene with displacement using the basic ACC conversion without the 
// tangent patches. The shading will likely have discontinuities at the edges of 
// extraordinary patches.
//--------------------------------------------------------------------------------------
technique10 RenderSceneBump
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, RenderSceneVS( true, false, true, true ) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, RenderScenePS( false, true, true ) ) );

        SetBlendState( NoBlending, float4(0,0,0,0), 0xffffffff );
        SetDepthStencilState( EnableDepth, 0 );
        SetRasterizerState( CullBackSolid );
    }
    
    pass P1
    {
        SetVertexShader( CompileShader( vs_4_0, RenderSceneVS( true, false, true, true ) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, RenderScenePS( true, false, false ) ) );

        SetBlendState( NoBlending, float4(0,0,0,0), 0xffffffff );
        SetDepthStencilState( EnableDepth, 0 );
        SetRasterizerState( CullBackWire );
    }
}

//--------------------------------------------------------------------------------------
// Renders the scene with displacement using the ACC conversion with the tangent patches.
// The shading will be smooth at the edges of extraordinary patches.
//--------------------------------------------------------------------------------------
technique10 RenderSceneBumpTan
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, RenderSceneVS( true, true, true, true ) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, RenderScenePS( false, true, true ) ) );

        SetBlendState( NoBlending, float4(0,0,0,0), 0xffffffff );
        SetDepthStencilState( EnableDepth, 0 );
        SetRasterizerState( CullBackSolid );
    }
    
    pass P1
    {
        SetVertexShader( CompileShader( vs_4_0, RenderSceneVS( true, true, true, true ) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, RenderScenePS( true, false, false ) ) );

        SetBlendState( NoBlending, float4(0,0,0,0), 0xffffffff );
        SetDepthStencilState( EnableDepth, 0 );
        SetRasterizerState( CullBackWire );
    }
}