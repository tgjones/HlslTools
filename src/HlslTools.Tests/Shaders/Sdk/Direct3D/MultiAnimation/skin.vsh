//
// HLSL function for skinning a mesh.  In your shader, you can #define 
// MATRIX_PALETTE_SIZE if desired, and then #include this file.
// Copyright (c) 2000-2003 Microsoft Corporation. All rights reserved.
//


#ifndef VS_SKIN_VSH
#define VS_SKIN_VSH


//----------------------------------------------------------------------------
// Global parameters 
//----------------------------------------------------------------------------


// Declare the 4x3 matrix palette.  This is the array of bone matrices used in
// skinning vertices.

// The palette size is 26 by default.  This is sufficiently small for most 
// vs_1_1 shaders.  Shaders targeted at vs_2_0 and above can set this higher
// to accommondate more bones in a call.  For example, tiny_anim.x has 35
// bones, and so can be rendered in a single call if MATRIX_PALETTE_SIZE is
// set to 35 or more.

// An HLSL shader can set MATRIX_PALETTE_SIZE_DEFAULT to a different value.
// The calling app can also set it in the D3DXMACRO structure when compiling
// the shader.  The calling app can query the actual palette size by examining
// MATRIX_PALETTE_SIZE (but changing it after compilation will not change the
// palette size in the compiled shader, of course).


#ifndef MATRIX_PALETTE_SIZE_DEFAULT
#define MATRIX_PALETTE_SIZE_DEFAULT 26
#endif

const int MATRIX_PALETTE_SIZE = MATRIX_PALETTE_SIZE_DEFAULT;
float4x3 amPalette[ MATRIX_PALETTE_SIZE_DEFAULT ];


//----------------------------------------------------------------------------
// Shader body - VS_ Skin
//----------------------------------------------------------------------------

// define the inputs -- caller must fill this, usually right from the VB
struct VS_SKIN_INPUT
{
    float4      vPos;
    float3      vBlendWeights;
    float4      vBlendIndices;
    float3      vNor;
};

// return skinned position and normal
struct VS_SKIN_OUTPUT
{
    float4 vPos;
    float3 vNor;
};

// call this function to skin VB position and normal
VS_SKIN_OUTPUT VS_Skin( const VS_SKIN_INPUT vInput, int iNumBones )
{
    VS_SKIN_OUTPUT vOutput = (VS_SKIN_OUTPUT) 0;

    float fLastWeight = 1.0;
    float fWeight;
    float afBlendWeights[ 3 ] = (float[ 3 ]) vInput.vBlendWeights;
    int aiIndices[ 4 ] = (int[ 4 ]) D3DCOLORtoUBYTE4( vInput.vBlendIndices );
    
    for( int iBone = 0; (iBone < 3) && (iBone < iNumBones - 1); ++ iBone )
    {
        fWeight = afBlendWeights[ iBone ];
        fLastWeight -= fWeight;
        vOutput.vPos.xyz += mul( vInput.vPos, amPalette[ aiIndices[ iBone ] ] ) * fWeight;
        vOutput.vNor     += mul( vInput.vNor, amPalette[ aiIndices[ iBone ] ] ) * fWeight;
    }
    
    vOutput.vPos.xyz += mul( vInput.vPos, amPalette[ aiIndices[ iNumBones - 1 ] ] ) * fLastWeight;
    vOutput.vNor     += mul( vInput.vNor, amPalette[ aiIndices[ iNumBones - 1 ] ] ) * fLastWeight;

    return vOutput;
}


#endif // #ifndef VS_SKIN_VSH
