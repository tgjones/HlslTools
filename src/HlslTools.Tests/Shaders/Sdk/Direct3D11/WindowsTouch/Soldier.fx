//--------------------------------------------------------------------------------------
// File: Soldier.fx
//
// The effect file for the Skinning10 sample.  
// 
// Copyright (c) Microsoft Corporation. All rights reserved.
//--------------------------------------------------------------------------------------

//--------------------------------------------------------------------------------------
// defines
//--------------------------------------------------------------------------------------
#define MAX_BONE_MATRICES 255 
#define FT_CONSTANTBUFFER 0
#define FT_TEXTUREBUFFER 1
#define FT_TEXTURE 2
#define FT_BUFFER 3




//--------------------------------------------------------------------------------------
// Input/Output structures
//--------------------------------------------------------------------------------------
struct VSSkinnedIn
{
    float3 Pos                      : POSITION;             //Position
    float4 Weights                  : WEIGHTS;              //Bone weights
    uint4  Bones                    : BONES;                //Bone indices
    float3 Norm                     : NORMAL;               //Normal
    float2 Tex                      : TEXCOORD;             //Texture coordinate
    float3 Tan                      : TANGENT;              //Normalized Tangent vector
};

struct VSNotanimatedIn
{
    float3 Pos                      : POSITION;             //Position
    float3 Norm                     : NORMAL;               //Normal
    float2 Tex                      : TEXCOORD;             //Texture coordinate
    float3 Tan                      : TANGENT;              //Normalized Tangent vector
};

struct PSSkinnedIn
{
    float4 Pos                      : SV_Position;          //Position
    float3 vPos                     : POSWORLD;             //world space Pos
    float3 Norm                     : NORMAL;               //Normal
    float2 Tex                      : TEXCOORD;             //Texture coordinate
    float3 Tangent                  : TANGENT;              //Normalized Tangent vector
};

//--------------------------------------------------------------------------------------
// Constant buffers
//--------------------------------------------------------------------------------------
cbuffer CBMatrices : register( b0 )
{
    float4x4 m_matWorldViewProj;
    float4x4 m_matWorld;
};

cbuffer cbUserChange : register (b1)
{
    float4 m_vLightPos;
    float4 m_vEyePt;
    float4 m_vSelected ;
};

cbuffer cbImmutable 
{
    float4 m_vDirectional = float4(1.0,1.0,1.0,1.0);
    float4 m_vAmbient = float4(0.1,0.1,0.1,0.0);
    float4 m_vSpecular = float4(1.0,1.0,1.0,1.0);
};

// Constant buffer for bone matrices
cbuffer cbAnimMatrices : register (b2)
{
    matrix m_matConstBoneWorld[MAX_BONE_MATRICES];
};



//--------------------------------------------------------------------------------------
// Textures
//--------------------------------------------------------------------------------------
Texture2D g_txDiffuse;
Texture2D g_txNormal;
// Texture for bone matrices
Texture1D g_txTexBoneWorld;

//--------------------------------------------------------------------------------------
// Buffers (this is the buffer object for bone matrices)
//--------------------------------------------------------------------------------------
Buffer<float4> g_BufferBoneWorld;

//--------------------------------------------------------------------------------------
// Texture samplers
//--------------------------------------------------------------------------------------
SamplerState g_samLinear
{
    Filter = ANISOTROPIC;
    AddressU = Clamp;
    AddressV = Clamp;
};

SamplerState g_samPoint
{
    Filter = MIN_MAG_MIP_POINT;
    AddressU = Clamp;
    AddressV = Clamp;
};

//--------------------------------------------------------------------------------------
// State
//--------------------------------------------------------------------------------------
DepthStencilState DisableDepth
{
    DepthEnable = FALSE;
    DepthWriteMask = 0;
};

DepthStencilState EnableDepth
{
    DepthEnable = TRUE;
    DepthWriteMask = ALL;
};

//--------------------------------------------------------------------------------------
// Helper struct for passing back skinned vertex information
//--------------------------------------------------------------------------------------
struct SkinnedInfo
{
    float4 Pos;
    float3 Norm;
    float3 Tan;
};

//--------------------------------------------------------------------------------------
// FetchBoneTransform fetches a bone transformation using one of several methods
//
//        FT_CONSTANTBUFFER:
//            With this approach, the bone matrices are stored in a constant buffer.
//            The shader will index into the constant buffer to grab the correct
//            transformation matrices for each vertex.
//--------------------------------------------------------------------------------------
matrix FetchBoneTransform( uint iBone )
{
    matrix mret;
       mret = m_matConstBoneWorld[ iBone ];    
    return mret;
}

//--------------------------------------------------------------------------------------
// SkinVert skins a single vertex
//--------------------------------------------------------------------------------------
SkinnedInfo SkinVert( VSSkinnedIn Input)
{
    SkinnedInfo Output = (SkinnedInfo)0;
    
    float4 Pos = float4(Input.Pos,1);
    float3 Norm = Input.Norm;
    float3 Tan = Input.Tan;
    Matrix m = 
        FetchBoneTransform (Input.Bones.x) * Input.Weights.x
        + FetchBoneTransform (Input.Bones.y) * Input.Weights.y
        + FetchBoneTransform (Input.Bones.z) * Input.Weights.z
        + FetchBoneTransform (Input.Bones.w) * Input.Weights.w;
    Output.Pos = mul (Pos,m);
    Output.Norm = mul (Norm,(float3x3)m);
    Output.Tan = mul (Tan,(float3x3)m);
    
    return Output;
}

//--------------------------------------------------------------------------------------
// Vertex shader used for skinning the mesh for immediate render
//--------------------------------------------------------------------------------------
PSSkinnedIn VSSkinnedmain(VSSkinnedIn input)
{
    PSSkinnedIn output;
    SkinnedInfo vSkinned = SkinVert( input);
    output.Pos = mul( vSkinned.Pos, m_matWorldViewProj );
    output.vPos = mul( vSkinned.Pos, m_matWorld );
    output.Norm = normalize( mul( vSkinned.Norm, (float3x3)m_matWorld ) );
    output.Tangent = normalize( mul( vSkinned.Tan, (float3x3)m_matWorld ) );
    output.Tex = input.Tex;
    
    return output;
}

PSSkinnedIn VSmain(VSNotanimatedIn input)
{
    PSSkinnedIn output;
    float4 Pos = float4(input.Pos,1);
    output.Pos = mul( Pos, m_matWorldViewProj );
    output.vPos = mul( Pos, m_matWorld );
    output.Norm = normalize( mul( input.Norm, (float3x3)m_matWorld ) );
    output.Tangent = normalize( mul( input.Tan, (float3x3)m_matWorld ) );
    output.Tex = input.Tex;
    
    return output;
}



//--------------------------------------------------------------------------------------
// Pixel shader that performs bump mapping on the final vertex
//--------------------------------------------------------------------------------------
float4 PSSkinnedmain(PSSkinnedIn input) : SV_Target
{    

    float4 diffuse = g_txDiffuse.Sample( g_samLinear, input.Tex );
    float3 Norm = g_txNormal.Sample( g_samLinear, input.Tex );
    Norm *= 2.0;
    Norm -= float3(1,1,1);
    
    // Create TBN matrix
    float3 lightDir = normalize( m_vLightPos.xyz - input.vPos );
    float3 viewDir = normalize( m_vEyePt.xyz - input.vPos );
    float3 BiNorm = normalize( cross( input.Norm, input.Tangent ) );
    float3x3 BTNMatrix = float3x3( BiNorm, input.Tangent, input.Norm );
    Norm = normalize( mul( Norm, BTNMatrix ) ); //world space bump
    
    //diffuse lighting
    float lightAmt = saturate( dot( lightDir, Norm ) );
    float4 lightColor = lightAmt.xxxx * m_vDirectional + m_vAmbient;

    // Calculate specular power
    float3 halfAngle = normalize( viewDir + lightDir );
    float4 vSpecular = pow( saturate(dot( halfAngle, Norm )), 64 );
        
    // Return combined lighting
    return ( lightColor * diffuse + vSpecular * m_vSpecular * diffuse.a) + m_vSelected;
}




