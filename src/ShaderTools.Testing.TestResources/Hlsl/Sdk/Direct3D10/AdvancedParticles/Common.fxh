//--------------------------------------------------------------------------------------
// File: Meshes.fx
//
// Copyright (c) Microsoft Corporation. All rights reserved.
//--------------------------------------------------------------------------------------
#define MAX_BONE_MATRICES 255

struct VSAnimIn
{
	float3 Pos				: POSITION;
	float4 Weights			: WEIGHTS;
	uint4  Bones			: BONES;
	float3 Norm 			: NORMAL;
	float2 Tex				: TEXCOORD0;
	float3 Tan				: TANGENT;
	uint   InstanceID		: SV_INSTANCEID;
};

struct VSAnimOut
{
	float4 Pos				: SV_POSITION;
	float3 vPos				: POSVIEW;
	float3 Norm  			: NORMAL;
	float2 Tex				: TEXCOORD0;
	float3 Tan				: TANGENT;
};

cbuffer cbAnimMatrices
{
    matrix g_mBoneWorld[MAX_BONE_MATRICES];
    matrix g_mBonePrev[MAX_BONE_MATRICES];
};


struct SkinnedInfo
{
    float4 Pos;
    float3 Norm;
    float3 Tan;
};


//
// Helper for skinning a vertex
//
SkinnedInfo SkinVert( VSAnimIn Input )
{
    SkinnedInfo Output = (SkinnedInfo)0;
    
    float4 Pos = float4(Input.Pos,1);
    float3 Norm = Input.Norm;
    float3 Tan = Input.Tan;
    
    uint iBone = Input.Bones.x;
    float fWeight = Input.Weights.x;
    matrix m = g_mBoneWorld[ iBone ];
    Output.Pos += fWeight * mul( Pos, m );
    Output.Norm += fWeight * mul( Norm, m );
    Output.Tan += fWeight * mul( Tan, (float3x3)m );
    
    iBone = Input.Bones.y;
    fWeight = Input.Weights.y;
    m = g_mBoneWorld[ iBone ];
    Output.Pos += fWeight * mul( Pos, m );
    Output.Norm += fWeight * mul( Norm, m );
    Output.Tan += fWeight * mul( Tan, (float3x3)m );
    
    iBone = Input.Bones.z;
    fWeight = Input.Weights.z;
    m = g_mBoneWorld[ iBone ];
    Output.Pos += fWeight * mul( Pos, m );
    Output.Norm += fWeight * mul( Norm, m );
    Output.Tan += fWeight * mul( Tan, (float3x3)m );
    
    iBone = Input.Bones.w;
    fWeight = Input.Weights.w;
    m = g_mBoneWorld[ iBone ];
    Output.Pos += fWeight * mul( Pos, m );
    Output.Norm += fWeight * mul( Norm, m );
    Output.Tan += fWeight * mul( Tan, (float3x3)m );
    
    return Output;
}


//
// Helper for skinning a vertex
//
SkinnedInfo SkinVertPrev( VSAnimIn Input )
{
    SkinnedInfo Output = (SkinnedInfo)0;
    
    float4 Pos = float4(Input.Pos,1);
    
    uint iBone = Input.Bones.x;
    float fWeight = Input.Weights.x;
    matrix m = g_mBonePrev[ iBone ];
    Output.Pos += fWeight * mul( Pos, m );
    
    iBone = Input.Bones.y;
    fWeight = Input.Weights.y;
    m = g_mBonePrev[ iBone ];
    Output.Pos += fWeight * mul( Pos, m );
    
    iBone = Input.Bones.z;
    fWeight = Input.Weights.z;
    m = g_mBonePrev[ iBone ];
    Output.Pos += fWeight * mul( Pos, m );
    
    iBone = Input.Bones.w;
    fWeight = Input.Weights.w;
    m = g_mBonePrev[ iBone ];
    Output.Pos += fWeight * mul( Pos, m );
    
    return Output;
}