//--------------------------------------------------------------------------------------
// File: SubDtoBezier.fx
//
// This effect file handles the conversion from a Catmull-Clark subdivision surface
// representation to a bicubic patch representation. 
// 
// Copyright (c) Microsoft Corporation. All rights reserved.
//--------------------------------------------------------------------------------------
#include "GenPatch.fxh"

//--------------------------------------------------------------------------------------
// State objects
//--------------------------------------------------------------------------------------
BlendState NoBlending
{
    BlendEnable[0] = FALSE;
};

DepthStencilState DisableDepth
{
    DepthEnable = FALSE;
    DepthWriteMask = ALL;
    DepthFunc = LESS_EQUAL;
};

RasterizerState CullNoneSolid
{
    CullMode = NONE;
    FillMode = SOLID;
};

//--------------------------------------------------------------------------------------
// Constant buffers
//--------------------------------------------------------------------------------------
cbuffer cbPerFrame
{
    float g_fTime;
};

#define MAX_BONE_MATRICES 4
cbuffer cbPerMesh
{
    matrix g_mConstBoneWorld[MAX_BONE_MATRICES];
};

//--------------------------------------------------------------------------------------
// Input buffers for point data and bone data
//--------------------------------------------------------------------------------------
Buffer<float4> g_bufPoints;
Buffer<uint4> g_bufBones;

//--------------------------------------------------------------------------------------
// Vertex struct definitions
//--------------------------------------------------------------------------------------
struct VS_INPUT_EXTRA
{
    uint4 Verts[MAX_POINTS/4]   : CONTROLPOINTS;
    uint4 Valences              : VALENCES;
    uint4 Prefixes              : PREFIXES;
};

struct VS_INPUT_REG
{
    float4 Verts[16] : CONTROLPOINTS;
};

struct VS_OUTPUT
{
    float4 Points[PATCH_STRIDE]	: POINTS;
};

struct VS_UVOUTPUT
{
    float4 Points[9]            : POINTS;
};

//--------------------------------------------------------------------------------------
// Skins a single vertex with very simple two-bone skinning
//--------------------------------------------------------------------------------------
float4 SkinVert( float4 Pos, uint4 BoneNWeight )
{
    float4 OutputPos;
    
    //Bone0
    uint iBone = BoneNWeight.x;
    float fWeight = (float)BoneNWeight.z/255.0;
    matrix m = g_mConstBoneWorld[ iBone ];
    OutputPos = fWeight * mul( Pos, m );
    
    //Bone1
    iBone = BoneNWeight.y;
    fWeight = (float)BoneNWeight.w/255.0;
    m = g_mConstBoneWorld[ iBone ];
    OutputPos += fWeight * mul( Pos, m );
    
    return OutputPos;
}

//--------------------------------------------------------------------------------------
// Loads vertices from a extraordinary sub-d patch
//--------------------------------------------------------------------------------------
void LoadVerts_Extra( in VS_INPUT_EXTRA input, 
                out float4 verts[MAX_POINTS], 
                out uint val[4], 
                out uint pref[4],
                uniform bool bSkinVerts )
{
    uint index = 0;    
    uint4 BoneNWeight[MAX_POINTS];

    for( uint i=0; i<MAX_POINTS/4; i++ )
    {
        // For extraodinary patches, the data is loaded indirectly from the g_bufPoints buffer.
        // This is necessary, because we can only load 16 elements from the vertex stream and
        // there may be MAX_POINTS points associated with this patch.
        verts[index  ] = g_bufPoints.Load( input.Verts[i].x ); 
        verts[index+1] = g_bufPoints.Load( input.Verts[i].y ); 
        verts[index+2] = g_bufPoints.Load( input.Verts[i].z ); 
        verts[index+3] = g_bufPoints.Load( input.Verts[i].w ); 
        
        if( bSkinVerts )
        {
            BoneNWeight[index  ] = g_bufBones.Load( input.Verts[i].x );
            BoneNWeight[index+1] = g_bufBones.Load( input.Verts[i].y );
            BoneNWeight[index+2] = g_bufBones.Load( input.Verts[i].z );
            BoneNWeight[index+3] = g_bufBones.Load( input.Verts[i].w );
            
            verts[index  ] = SkinVert( verts[index  ], BoneNWeight[index  ] );
            verts[index+1] = SkinVert( verts[index+1], BoneNWeight[index+1] );
            verts[index+2] = SkinVert( verts[index+2], BoneNWeight[index+2] );
            verts[index+3] = SkinVert( verts[index+3], BoneNWeight[index+3] );
        }

        index += 4;
    }
    
    val[0] = input.Valences.x;
    val[1] = input.Valences.y;
    val[2] = input.Valences.z;
    val[3] = input.Valences.w;
    
    pref[0] = input.Prefixes.x;
    pref[1] = input.Prefixes.y;
    pref[2] = input.Prefixes.z;
    pref[3] = input.Prefixes.w;
}

//--------------------------------------------------------------------------------------
// Loads vertices from a regular sub-d patch (all valence 4 vertices)
//--------------------------------------------------------------------------------------
void LoadVerts_Reg( in VS_INPUT_REG input, 
                    out float4 verts[MAX_POINTS], uniform bool bSkinVerts )
{
    uint index = 0;
    uint4 BoneNWeight[16];

    for( uint i=0; i<4; i++ )
    {
        // For regular patches, most of the data comes straight from the vertex stream
        verts[index  ] = input.Verts[index  ];
        verts[index+1] = input.Verts[index+1];
        verts[index+2] = input.Verts[index+2];
        verts[index+3] = input.Verts[index+3];
    
        if( bSkinVerts ) 
        {
            // Since the IA only allows us to load 16 elements, we have to pack the bone data into the W
            // coordinate of the patch vertex.
            [unroll] for( uint v=0; v<4; v++ )
            {
                uint bonenweight = asuint(input.Verts[index+v].w);
                verts[index+v].w = 1;
                
                // Pack bone Number and Weight into a single uint
                BoneNWeight[index+v].x = (bonenweight >> 24);
                BoneNWeight[index+v].y = (bonenweight >> 16) & 255;
                BoneNWeight[index+v].z = (bonenweight >>  8) & 255;
                BoneNWeight[index+v].w = (bonenweight      ) & 255;
            }

            // Skin the vertices if necessary
            verts[index  ] = SkinVert( verts[index  ], BoneNWeight[index  ] );
            verts[index+1] = SkinVert( verts[index+1], BoneNWeight[index+1] );
            verts[index+2] = SkinVert( verts[index+2], BoneNWeight[index+2] );
            verts[index+3] = SkinVert( verts[index+3], BoneNWeight[index+3] );
        }

        index += 4;
    }
    
    // Patchup so we don't have to have separate conversion functions in GenPatch.fx
    // for regular (16 vertices) and extraordinary (up to 32 vertices) patches.  
    // Hopefully, the compiler will nop this out.
    for( uint v=16; v<MAX_POINTS; v++ )
        verts[v] = float4(0,0,0,0);
}


//------------------------------------------------------------
// Convert an extraordinary patch to a bicubic patch
//------------------------------------------------------------
VS_OUTPUT VSConvertToBezierB_Extra( VS_INPUT_EXTRA input, uniform bool bSkinVerts )
{
    VS_OUTPUT output;
    
    float4 verts[MAX_POINTS];
    uint val[4];
    uint pref[4];
    
    LoadVerts_Extra( input, verts, val, pref, bSkinVerts );
    GenPatchB( verts, val, pref, output.Points );
    
    return output;
}

//------------------------------------------------------------
// Convert an extraordinary patch to a tangent UV patch
//------------------------------------------------------------
VS_UVOUTPUT VSConvertToBezierUV_Extra( VS_INPUT_EXTRA input, uniform bool bSkinVerts )
{
    VS_UVOUTPUT output;
    
    float4 verts[MAX_POINTS];
    uint val[4];
    uint pref[4];
    
    LoadVerts_Extra( input, verts, val, pref, bSkinVerts );
    GenPatchTanCompact( verts, val, pref, output.Points );
              
    return output;
}

//------------------------------------------------------------
// Convert a regular patch to a bicubic patch
//------------------------------------------------------------
VS_OUTPUT VSConvertToBezierB_Reg( VS_INPUT_REG input, uniform bool bSkinVerts )
{
    VS_OUTPUT output;
    
    float4 verts[MAX_POINTS];
    // Valence and Prefixes are well known for regular patches.
    // This will simplify the code and allow for better optimization.
    const uint val[4] = {4,4,4,4};
    const uint pref[4] = {7,10,13,16};
    
    LoadVerts_Reg( input, verts, bSkinVerts );
    GenPatchB( verts, val, pref, output.Points );
    
    return output;
}

//------------------------------------------------------------
// Convert a regular patch to a tangent UV patch
//------------------------------------------------------------
VS_UVOUTPUT VSConvertToBezierUV_Reg( VS_INPUT_REG input, uniform bool bSkinVerts )
{
    VS_UVOUTPUT output;
    
    float4 verts[MAX_POINTS];
    // Valence and Prefixes are well known for regular patches.
    // This will simplify the code and allow for better optimization.
    const uint val[4] = {4,4,4,4};
    const uint pref[4] = {7,10,13,16};
    
    LoadVerts_Reg( input, verts, bSkinVerts );
    GenPatchTanCompact( verts, val, pref, output.Points );
              
    return output;
}

//--------------------------------------------------------------------------------------
// Techniques
//--------------------------------------------------------------------------------------

//------------------------------------------------------------
// Convert an extraordinary patch to a bicubic patch
//
// We create a geometry shader from the vertex shader below
// and bind both in order to trick FX into letting us stream-
// out from the vertex shader without a geometry shader.
//------------------------------------------------------------
VertexShader vsSubDToBezierB_Extra = CompileShader( vs_4_0, VSConvertToBezierB_Extra(false) );
GeometryShader vsSubDToBezierBSO = ConstructGSWithSO( vsSubDToBezierB_Extra, "POINTS0.xyzw; POINTS1.xyzw; POINTS2.xyzw; POINTS3.xyzw; POINTS4.xyzw; POINTS5.xyzw; POINTS6.xyzw; POINTS7.xyzw; POINTS8.xyzw; POINTS9.xyzw; POINTS10.xyzw; POINTS11.xyzw; POINTS12.xyzw; POINTS13.xyzw; POINTS14.xyzw; POINTS15.xyzw" );

technique10 SubDToBezierB_Extra
{
    pass C0
    {
        SetVertexShader( vsSubDToBezierB_Extra );
        SetGeometryShader( vsSubDToBezierBSO );
        SetPixelShader( NULL );

        SetBlendState( NoBlending, float4(0,0,0,0), 0xffffffff );
        SetDepthStencilState( DisableDepth, 0 );
        SetRasterizerState( CullNoneSolid );
    }
}

//------------------------------------------------------------
// Convert an extraordinary patch to a tangent UV patch
//
// We create a geometry shader from the vertex shader below
// and bind both in order to trick FX into letting us stream-
// out from the vertex shader without a geometry shader.
//------------------------------------------------------------
VertexShader vsSubDToBezierUV_Extra = CompileShader( vs_4_0, VSConvertToBezierUV_Extra(false) );
GeometryShader vsSubDToBezierUVSO = ConstructGSWithSO( vsSubDToBezierUV_Extra, "POINTS0.xyzw; POINTS1.xyzw; POINTS2.xyzw; POINTS3.xyzw; POINTS4.xyzw; POINTS5.xyzw; POINTS6.xyzw; POINTS7.xyzw; POINTS8.xyzw" );

technique10 SubDToBezierUV_Extra
{
    pass C0
    {
        SetVertexShader( vsSubDToBezierUV_Extra );
        SetGeometryShader( vsSubDToBezierUVSO );
        SetPixelShader( NULL );

        SetBlendState( NoBlending, float4(0,0,0,0), 0xffffffff );
        SetDepthStencilState( DisableDepth, 0 );
        SetRasterizerState( CullNoneSolid );
    }
}

//------------------------------------------------------------
// Convert a regular patch to a bicubic patch
//
// We create a geometry shader from the vertex shader below
// and bind both in order to trick FX into letting us stream-
// out from the vertex shader without a geometry shader.
//------------------------------------------------------------
VertexShader vsSubDToBezierB_Reg = CompileShader( vs_4_0, VSConvertToBezierB_Reg(false) );
GeometryShader vsSubDToBezierBSO_Reg = ConstructGSWithSO( vsSubDToBezierB_Reg, "POINTS0.xyzw; POINTS1.xyzw; POINTS2.xyzw; POINTS3.xyzw; POINTS4.xyzw; POINTS5.xyzw; POINTS6.xyzw; POINTS7.xyzw; POINTS8.xyzw; POINTS9.xyzw; POINTS10.xyzw; POINTS11.xyzw; POINTS12.xyzw; POINTS13.xyzw; POINTS14.xyzw; POINTS15.xyzw" );

technique10 SubDToBezierB_Reg
{
    pass C0
    {
        SetVertexShader( vsSubDToBezierB_Reg );
        SetGeometryShader( vsSubDToBezierBSO_Reg );
        SetPixelShader( NULL );

        SetBlendState( NoBlending, float4(0,0,0,0), 0xffffffff );
        SetDepthStencilState( DisableDepth, 0 );
        SetRasterizerState( CullNoneSolid );
    }
}

//------------------------------------------------------------
// Convert a regular patch to a tangent UV patch
//
// We create a geometry shader from the vertex shader below
// and bind both in order to trick FX into letting us stream-
// out from the vertex shader without a geometry shader.
//------------------------------------------------------------
VertexShader vsSubDToBezierUV_Reg = CompileShader( vs_4_0, VSConvertToBezierUV_Reg(false) );
GeometryShader vsSubDToBezierUVSO_Reg = ConstructGSWithSO( vsSubDToBezierUV_Reg, "POINTS0.xyzw; POINTS1.xyzw; POINTS2.xyzw; POINTS3.xyzw; POINTS4.xyzw; POINTS5.xyzw; POINTS6.xyzw; POINTS7.xyzw; POINTS8.xyzw" );

technique10 SubDToBezierUV_Reg
{
    pass C0
    {
        SetVertexShader( vsSubDToBezierUV_Reg );
        SetGeometryShader( vsSubDToBezierUVSO_Reg );
        SetPixelShader( NULL );

        SetBlendState( NoBlending, float4(0,0,0,0), 0xffffffff );
        SetDepthStencilState( DisableDepth, 0 );
        SetRasterizerState( CullNoneSolid );
    }
}


//------------------------------------------------------------
// Convert an extraordinary patch to a bicubic patch with skinning
//
// We create a geometry shader from the vertex shader below
// and bind both in order to trick FX into letting us stream-
// out from the vertex shader without a geometry shader.
//------------------------------------------------------------
VertexShader vsSubDToBezierB_Extra_Skin = CompileShader( vs_4_0, VSConvertToBezierB_Extra(true) );
GeometryShader vsSubDToBezierBSO_Skin = ConstructGSWithSO( vsSubDToBezierB_Extra_Skin, "POINTS0.xyzw; POINTS1.xyzw; POINTS2.xyzw; POINTS3.xyzw; POINTS4.xyzw; POINTS5.xyzw; POINTS6.xyzw; POINTS7.xyzw; POINTS8.xyzw; POINTS9.xyzw; POINTS10.xyzw; POINTS11.xyzw; POINTS12.xyzw; POINTS13.xyzw; POINTS14.xyzw; POINTS15.xyzw" );

technique10 SubDToBezierB_Extra_Skin
{
    pass C0
    {
        SetVertexShader( vsSubDToBezierB_Extra_Skin );
        SetGeometryShader( vsSubDToBezierBSO_Skin );
        SetPixelShader( NULL );

        SetBlendState( NoBlending, float4(0,0,0,0), 0xffffffff );
        SetDepthStencilState( DisableDepth, 0 );
        SetRasterizerState( CullNoneSolid );
    }
}

//------------------------------------------------------------
// Convert an extraordinary patch to a tangent UV patch with skinning
//
// We create a geometry shader from the vertex shader below
// and bind both in order to trick FX into letting us stream-
// out from the vertex shader without a geometry shader.
//------------------------------------------------------------
VertexShader vsSubDToBezierUV_Extra_Skin = CompileShader( vs_4_0, VSConvertToBezierUV_Extra(true) );
GeometryShader vsSubDToBezierUVSO_Skin = ConstructGSWithSO( vsSubDToBezierUV_Extra_Skin, "POINTS0.xyzw; POINTS1.xyzw; POINTS2.xyzw; POINTS3.xyzw; POINTS4.xyzw; POINTS5.xyzw; POINTS6.xyzw; POINTS7.xyzw; POINTS8.xyzw" );

technique10 SubDToBezierUV_Extra_Skin
{
    pass C0
    {
        SetVertexShader( vsSubDToBezierUV_Extra_Skin );
        SetGeometryShader( vsSubDToBezierUVSO_Skin );
        SetPixelShader( NULL );

        SetBlendState( NoBlending, float4(0,0,0,0), 0xffffffff );
        SetDepthStencilState( DisableDepth, 0 );
        SetRasterizerState( CullNoneSolid );
    }
}

//------------------------------------------------------------
// Convert a regular patch to a bicubic patch with skinning
//
// We create a geometry shader from the vertex shader below
// and bind both in order to trick FX into letting us stream-
// out from the vertex shader without a geometry shader.
//------------------------------------------------------------
VertexShader vsSubDToBezierB_Reg_Skin = CompileShader( vs_4_0, VSConvertToBezierB_Reg(true) );
GeometryShader vsSubDToBezierBSO_Reg_Skin = ConstructGSWithSO( vsSubDToBezierB_Reg_Skin, "POINTS0.xyzw; POINTS1.xyzw; POINTS2.xyzw; POINTS3.xyzw; POINTS4.xyzw; POINTS5.xyzw; POINTS6.xyzw; POINTS7.xyzw; POINTS8.xyzw; POINTS9.xyzw; POINTS10.xyzw; POINTS11.xyzw; POINTS12.xyzw; POINTS13.xyzw; POINTS14.xyzw; POINTS15.xyzw" );

technique10 SubDToBezierB_Reg_Skin
{
    pass C0
    {
        SetVertexShader( vsSubDToBezierB_Reg_Skin );
        SetGeometryShader( vsSubDToBezierBSO_Reg_Skin );
        SetPixelShader( NULL );

        SetBlendState( NoBlending, float4(0,0,0,0), 0xffffffff );
        SetDepthStencilState( DisableDepth, 0 );
        SetRasterizerState( CullNoneSolid );
    }
}

//------------------------------------------------------------
// Convert a regular patch to a tangent UV patch with skinning
//
// We create a geometry shader from the vertex shader below
// and bind both in order to trick FX into letting us stream-
// out from the vertex shader without a geometry shader.
//------------------------------------------------------------
VertexShader vsSubDToBezierUV_Reg_Skin = CompileShader( vs_4_0, VSConvertToBezierUV_Reg(true) );
GeometryShader vsSubDToBezierUVSO_Reg_Skin = ConstructGSWithSO( vsSubDToBezierUV_Reg_Skin, "POINTS0.xyzw; POINTS1.xyzw; POINTS2.xyzw; POINTS3.xyzw; POINTS4.xyzw; POINTS5.xyzw; POINTS6.xyzw; POINTS7.xyzw; POINTS8.xyzw" );

technique10 SubDToBezierUV_Reg_Skin
{
    pass C0
    {
        SetVertexShader( vsSubDToBezierUV_Reg_Skin );
        SetGeometryShader( vsSubDToBezierUVSO_Reg_Skin );
        SetPixelShader( NULL );

        SetBlendState( NoBlending, float4(0,0,0,0), 0xffffffff );
        SetDepthStencilState( DisableDepth, 0 );
        SetRasterizerState( CullNoneSolid );
    }
}