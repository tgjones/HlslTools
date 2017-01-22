//--------------------------------------------------------------------------------------
// File: MotionBlur10.fx
//
// The effect file for the SoftShadow sample.
//
// Copyright (c) Microsoft Corporation. All rights reserved.
//--------------------------------------------------------------------------------------

#define MAX_TIME_STEPS 3
#define MID_TIME_STEP 1
#define MAX_BONE_MATRICES 100
//These are good settings
#define MIN_FADEANISO 0.1
#define MIN_FADEGEOM 0.1

cbuffer cbPerDraw
{
    matrix g_mWorldViewProj;
    matrix g_mWorldView;
    matrix g_mViewProj;
    float g_fFadeDist;
};

cbuffer cbTimeMatrices
{
    matrix g_mBlurViewProj[MAX_TIME_STEPS];
    matrix g_mBlurWorld[MAX_TIME_STEPS];
    
    matrix g_mBoneWorld[MAX_TIME_STEPS*MAX_BONE_MATRICES];
};

cbuffer cbPerFrame
{
    float g_fFrameTime;
};

cbuffer cbPerUser
{
    uint g_iNumSteps = 3;
    float g_fTextureSmear = 0.5f;
    float3 g_vLightDir = float3(0,0.707f,-0.707f);
};

Texture2D g_txDiffuse;

SamplerState g_samLinear
{
    Filter = ANISOTROPIC;
    MaxAnisotropy = 8;
    AddressU = Wrap;
    AddressV = Wrap;
};

SamplerState g_samPoint
{
    Filter = MIN_MAG_MIP_POINT;
    AddressU = Clamp;
    AddressV = Clamp;
};

SamplerState g_samLinearClamp
{
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = Clamp;
    AddressV = Clamp;
};


RasterizerState RasNoCull
{
    CullMode = NONE;
    MultiSampleEnable = FALSE;
};

RasterizerState RasMultiSampleNone
{
    CullMode = NONE;
    MultiSampleEnable = TRUE;
};

RasterizerState RasMultiSampleBack
{
    CullMode = BACK;
    MultiSampleEnable = TRUE;
};

RasterizerState RasBack
{
    CullMode = BACK;
    MultiSampleEnable = FALSE;
};


BlendState AdditiveBlend
{
    SrcBlend = One;
    DestBlend = One;
    BlendOp = Add;
};

DepthStencilState DepthTestWithoutDepthWrite
{
    DepthEnable = true;
    DepthWriteMask = ZERO;
    DepthFunc = LESS;
    StencilEnable = false;
};

DepthStencilState DepthTestNormal
{
    DepthEnable = true;
    DepthWriteMask = ALL;
    DepthFunc = LESS;
    StencilEnable = false;
    StencilReadMask = 0;
    StencilWriteMask = 0;
};

DepthStencilState DepthTestLessEqual
{
    DepthEnable = true;
    DepthWriteMask = ALL;
    DepthFunc = LESS_EQUAL;
    StencilEnable = false;
    StencilReadMask = 0;
    StencilWriteMask = 0;
};

BlendState NoBlending
{
    AlphaToCoverageEnable = FALSE;
    BlendEnable[0] = FALSE;
    SrcBlend = ONE;
    DestBlend = ONE;
    BlendOp = MIN;
    SrcBlendAlpha = ONE;
    DestBlendAlpha = ZERO;
    BlendOpAlpha = ADD;
    RenderTargetWriteMask[0] = 0x0F;
};

BlendState SrcAlphaBlending
{
    AlphaToCoverageEnable = TRUE;
    BlendEnable[0] = FALSE;
    SrcBlend = SRC_ALPHA;
    DestBlend = INV_SRC_ALPHA;
    BlendOp = ADD;
    SrcBlendAlpha = ZERO;
    DestBlendAlpha = ZERO;
    BlendOpAlpha = ADD;
    RenderTargetWriteMask[0] = 0x0F;
};


//--------------------------------------------------------------------------------------
// Rendering Geometry with Texture Motion Blur
//--------------------------------------------------------------------------------------
struct VSSceneIn
{
    float3 Pos : POSITION;
    float3 Normal : NORMAL;
    float2 Tex : TEXCOORD;
    float3 Tan : TANGENT;
};

struct VSSceneOut
{
    float4 Pos : SV_POSITION;
    float4 Color : COLOR0;
    float2 Tex : TEXCOORD;
    float2 Aniso : ANISOTROPY;
};

float ComputeMotionAlpha( float3 Pos1, float3 Pos2, float minFade )
{
    float dist = length( Pos1-Pos2 );
    float Alpha = max( minFade, 1.0f - (dist/g_fFadeDist) );
    return Alpha;
}

float4 ComputeLighting( float3 normal )
{
    float4 color = saturate( dot( normal, g_vLightDir ) );
    color += float4(0.5,0.5,0.5,0.0);
    return color;
}

VSSceneOut VSSceneMain( VSSceneIn Input )
{
    VSSceneOut Output = (VSSceneOut)0;
    
    // Normal transformation and lighting for the middle position
    matrix mWorldNow = g_mBlurWorld[ MID_TIME_STEP ];
    matrix mViewProjNow = g_mBlurViewProj[ MID_TIME_STEP ];
    
    Output.Pos = mul( float4(Input.Pos,1), mWorldNow );
    Output.Pos = mul( Output.Pos, mViewProjNow );
    float3 wNormal = mul( Input.Normal, (float3x3)mWorldNow );
    Output.Color = ComputeLighting( wNormal );
    Output.Tex = Input.Tex;
    
    // Find our direction of motion in clip space
    float4 nextPos = mul( float4(Input.Pos,1), g_mBlurWorld[ MID_TIME_STEP+1 ] );
    float4 prevPos = mul( float4(Input.Pos,1), g_mBlurWorld[ MID_TIME_STEP-1 ] );
    nextPos = mul( nextPos, g_mBlurViewProj[ MID_TIME_STEP+1 ] );
    prevPos = mul( prevPos, g_mBlurViewProj[ MID_TIME_STEP-1 ] );
    Output.Color.a = ComputeMotionAlpha( nextPos.xyz/nextPos.w, prevPos.xyz/prevPos.w, MIN_FADEANISO );
    
    float3 clipMotionDir = nextPos.xyz/nextPos.w - prevPos.xyz/prevPos.w;
    
    // Find our tangent, and bitangent in clip space
    float3 clipBiTangent = cross( Input.Tan, Input.Normal );
    clipBiTangent = mul( clipBiTangent, (float3x3)mWorldNow );
    clipBiTangent = normalize( mul( clipBiTangent, (float3x3)mViewProjNow ) );
    float3 clipTangent = mul( Input.Tan, (float3x3)mWorldNow );
    clipTangent = normalize( mul( clipTangent, (float3x3)mViewProjNow ) );
    
    // Find the projection of our motion into our tangent/texture space
    Output.Aniso.y = max( 0.0001, abs( g_fTextureSmear*dot( clipTangent, clipMotionDir ) ) );
    Output.Aniso.x = max( 0.0001, abs( g_fTextureSmear*dot( clipBiTangent, clipMotionDir ) ) );
    
    return Output;
}

float4 PSSceneMain( VSSceneOut Input ) : SV_TARGET
{
    float2 ddx = Input.Aniso;
    float2 ddy = Input.Aniso;
    
    float4 diff = g_txDiffuse.SampleGrad( g_samLinear, Input.Tex, ddx, ddy);
    diff.a = 1;
    return diff*Input.Color;
}

//--------------------------------------------------------------------------------------
// Rendering Geometry with GS Motion Blur
//--------------------------------------------------------------------------------------
struct VSMotionBlurIn
{
    float3 Pos : POSITION;
    float3 Normal : NORMAL;
    float2 Tex : TEXCOORD;
};

struct VSMotionBlurOut
{
    float4 Pos : SV_POSITION;
    float4 viewPos : VIEWPOS;
    float3 Norm : NORMAL;
    float4 Color : COLOR0;
    float2 Tex : TEXCOORD;
};

struct GSMotionBlurOut
{
    float4 Pos : SV_POSITION;
    float4 Color : COLOR0;
    float2 Tex : TEXCOORD;
};

VSMotionBlurOut VSMotionBlurMain( VSMotionBlurIn Input )
{
    VSMotionBlurOut Output = (VSMotionBlurOut)0;
    
    Output.Pos = float4(Input.Pos,1);
    Output.viewPos = mul( float4( Input.Pos, 1 ), g_mWorldView );
    Output.Norm = Input.Normal;
    Output.Tex = Input.Tex;
    
    float4 nextPos = mul( float4(Input.Pos,1), g_mBlurWorld[ MID_TIME_STEP+1 ] );
    float4 prevPos = mul( float4(Input.Pos,1), g_mBlurWorld[ MID_TIME_STEP-1 ] );
    nextPos = mul( nextPos, g_mBlurViewProj[ MID_TIME_STEP+1 ] );
    prevPos = mul( prevPos, g_mBlurViewProj[ MID_TIME_STEP-1 ] );
    Output.Color.a = ComputeMotionAlpha( nextPos.xyz/nextPos.w, prevPos.xyz/prevPos.w, MIN_FADEGEOM );
    
    return Output;
}

void ExtrudeEdge( VSMotionBlurOut v1, 
                  VSMotionBlurOut v2, 
                  uniform uint iStep,
                  inout TriangleStream<GSMotionBlurOut> SceneTriangleStream )
{
    GSMotionBlurOut oV1;
    GSMotionBlurOut oV2;
    oV1.Tex = v1.Tex;
    oV2.Tex = v2.Tex;
    float4 Pos1 = v1.Pos;
    float4 Pos2 = v2.Pos;
    
    float3 Norm1 = v1.Norm;
	float3 Norm2 = v2.Norm;
	float fA1 = v1.Color.a;
	float fA2 = v2.Color.a;
	float3 wNorm;
	uint iStep2 = g_iNumSteps;
	float fAlpha = ((float)iStep - MID_TIME_STEP)/(float)MID_TIME_STEP;
    
	matrix mWorld = g_mBlurWorld[ iStep ];
	matrix mViewProj = g_mBlurViewProj[ iStep ];
    
	wNorm = mul( Norm1, (float3x3)mWorld );
	oV1.Color = ComputeLighting( wNorm );
	wNorm = mul( Norm2, (float3x3)mWorld );
	oV2.Color = ComputeLighting( wNorm );
    
	oV1.Pos = mul( Pos1, mWorld );
	oV1.Pos = mul( oV1.Pos, mViewProj );
	oV2.Pos = mul( Pos2, mWorld );
	oV2.Pos = mul( oV2.Pos, mViewProj );
	oV1.Color.a = ( 1.0 - abs( fAlpha ) ) * fA1;
	oV2.Color.a = ( 1.0 - abs( fAlpha ) ) * fA2;
	SceneTriangleStream.Append( oV2 );
	SceneTriangleStream.Append( oV1 );
}

void ExtrudeBlurEdges( VSMotionBlurOut v1,
                      VSMotionBlurOut v2,
                      inout TriangleStream<GSMotionBlurOut> SceneTriangleStream,
                      uniform bool FrontSide )
{           
    uint iStart = 0;
    uint iEnd = MAX_TIME_STEPS;
    
    [unroll] for(uint i=iStart; i<iEnd; i++)
    {       
        ExtrudeEdge( v1, v2, i, SceneTriangleStream );
    }

    SceneTriangleStream.RestartStrip();
}

//output 2*3*MAX_TIME_STEPS
[maxvertexcount(30)]
void GSMotionBlurMain( triangle VSMotionBlurOut In[3], 
                       inout TriangleStream<GSMotionBlurOut> SceneTriangleStream, 
                       uniform bool bFrontSide )
{
    // find the triangle normal in view space
    float3 viewNorm = cross( In[1].viewPos - In[0].viewPos, In[2].viewPos - In[0].viewPos );
    
    // only extrude any of our edges if we're facing the camera
    if( dot( viewNorm, float3(0,0,-1) ) > 0.0 )
    {
        ExtrudeBlurEdges( In[0], In[1], SceneTriangleStream, bFrontSide );
        ExtrudeBlurEdges( In[1], In[2], SceneTriangleStream, bFrontSide );
        ExtrudeBlurEdges( In[2], In[0], SceneTriangleStream, bFrontSide );
    }
}

float4 PSMotionBlurMain( GSMotionBlurOut Input ) : SV_TARGET
{
    float4 diff = g_txDiffuse.SampleLevel( g_samLinear, Input.Tex, 0 );
    diff.a = 1;
    return diff*Input.Color;
}


//--------------------------------------------------------------------------------------
// Rendering Skinned Geometry with Texture Motion Blur
//--------------------------------------------------------------------------------------
struct VSSkinnedSceneIn
{
    float3 Pos : POSITION;
    float3 Normal : NORMAL;
    float2 Tex : TEXCOORD;
    float3 Tan : TANGENT;
    uint4 Bones : BONES;
    float4 Weights : WEIGHTS;
};

struct SkinnedInfo
{
    float4 Pos;
    float3 Norm;
};

SkinnedInfo SkinVert( VSSkinnedSceneIn Input, uint iTimeShift )
{
    SkinnedInfo Output = (SkinnedInfo)0;
    
    float4 pos = float4(Input.Pos,1);
    float3 norm = Input.Normal;
    
    uint iBone = Input.Bones.x;
    float fWeight = Input.Weights.x;
    //fWeight = 1.0f;
    matrix m = g_mBoneWorld[ iTimeShift*MAX_BONE_MATRICES + iBone ];
    Output.Pos += fWeight * mul( pos, m );
    Output.Norm += fWeight * mul( norm, m );
    
    iBone = Input.Bones.y;
    fWeight = Input.Weights.y;
    m = g_mBoneWorld[ iTimeShift*MAX_BONE_MATRICES + iBone ];
    Output.Pos += fWeight * mul( pos, m );
    Output.Norm += fWeight * mul( norm, m );

    iBone = Input.Bones.z;
    fWeight = Input.Weights.z;
    m = g_mBoneWorld[ iTimeShift*MAX_BONE_MATRICES + iBone ];
    Output.Pos += fWeight * mul( pos, m );
    Output.Norm += fWeight * mul( norm, m );
    
    iBone = Input.Bones.w;
    fWeight = Input.Weights.w;
    m = g_mBoneWorld[ iTimeShift*MAX_BONE_MATRICES + iBone ];
    Output.Pos += fWeight * mul( pos, m );
    Output.Norm += fWeight * mul( norm, m );
    
    return Output;
}

VSSceneOut VSSkinnedSceneMain( VSSkinnedSceneIn Input )
{
    VSSceneOut Output = (VSSceneOut)0;
    
    // Skin the vetex
    SkinnedInfo vSkinned = SkinVert( Input, MID_TIME_STEP );
    
    // ViewProj transform
    Output.Pos = mul( vSkinned.Pos, g_mBlurViewProj[ MID_TIME_STEP ] );
    
    // Lighting
    float3 blendNorm = vSkinned.Norm;
    Output.Color = ComputeLighting( blendNorm );
    Output.Tex = Input.Tex;
    
    matrix mWorldNow = g_mBlurWorld[ MID_TIME_STEP ];
    matrix mViewProjNow = g_mBlurViewProj[ MID_TIME_STEP ];
    
    // Find our direction of motion in clip space
    SkinnedInfo vNext = SkinVert( Input, MID_TIME_STEP+1 );
    SkinnedInfo vPrev = SkinVert( Input, MID_TIME_STEP-1 );
    float4 nextPos = mul( vNext.Pos, g_mBlurViewProj[ MID_TIME_STEP+1 ] );
    float4 prevPos = mul( vPrev.Pos, g_mBlurViewProj[ MID_TIME_STEP-1 ] );
    Output.Color.a = ComputeMotionAlpha( nextPos.xyz/nextPos.w, prevPos.xyz/prevPos.w, MIN_FADEANISO );
    
    float3 clipMotionDir = nextPos.xyz/nextPos.w - prevPos.xyz/prevPos.w;
    
    // Find our tangent, and bitangent in clip space
    float3 clipBiTangent = cross( Input.Tan, Input.Normal );
    clipBiTangent = mul( clipBiTangent, (float3x3)mWorldNow );
    clipBiTangent = normalize( mul( clipBiTangent, (float3x3)mViewProjNow ) );
    float3 clipTangent = mul( Input.Tan, (float3x3)mWorldNow );
    clipTangent = normalize( mul( clipTangent, (float3x3)mViewProjNow ) );
    
    // Find the projection of our motion into our tangent/texture space
    Output.Aniso.y = max( 0.0001, abs( g_fTextureSmear*dot( clipTangent, clipMotionDir ) ) );
    Output.Aniso.x = max( 0.0001, abs( g_fTextureSmear*dot( clipBiTangent, clipMotionDir ) ) );
    
    return Output;
}

//--------------------------------------------------------------------------------------
// Rendering Skinned Geometry with GS Motion Blur
//--------------------------------------------------------------------------------------
struct VSSkinnedMotionBlurOut
{
    float4 Pos : SV_POSITION;
    float4 viewPos : VIEWPOS;
    float3 Norm : NORMAL;
    float4 Color : COLOR0;
    float2 Tex : TEXCOORD;
    uint4 Bones : BONES;
    float4 Weights : WEIGHTS;
};

struct GSSkinnedMotionBlurOut
{
    float4 Pos : SV_POSITION;
    float4 Color : COLOR0;
    float2 Tex : TEXCOORD;
};


SkinnedInfo SkinVertBlur( VSSkinnedMotionBlurOut Input, uint iTimeShift )
{
    SkinnedInfo Output = (SkinnedInfo)0;
    
    float4 pos = Input.Pos;
    float3 norm = Input.Norm;
    
    uint iBone = Input.Bones.x;
    float fWeight = Input.Weights.x;
    matrix m = g_mBoneWorld[ iTimeShift*MAX_BONE_MATRICES + iBone ];
    Output.Pos += fWeight * mul( pos, m );
    Output.Norm += fWeight * mul( norm, m );
    
    iBone = Input.Bones.y;
    fWeight = Input.Weights.y;
    m = g_mBoneWorld[ iTimeShift*MAX_BONE_MATRICES + iBone ];
    Output.Pos += fWeight * mul( pos, m );
    Output.Norm += fWeight * mul( norm, m );
    
    iBone = Input.Bones.z;
    fWeight = Input.Weights.z;
    m = g_mBoneWorld[ iTimeShift*MAX_BONE_MATRICES + iBone ];
    Output.Pos += fWeight * mul( pos, m );
    Output.Norm += fWeight * mul( norm, m );
    
    iBone = Input.Bones.w;
    fWeight = Input.Weights.w;
    m = g_mBoneWorld[ iTimeShift*MAX_BONE_MATRICES + iBone ];
    Output.Pos += fWeight * mul( pos, m );
    Output.Norm += fWeight * mul( norm, m );
    
    return Output;
}

VSSkinnedMotionBlurOut VSSkinnedMotionBlurMain( VSSkinnedSceneIn Input )
{
    VSSkinnedMotionBlurOut Output = (VSSkinnedMotionBlurOut)0;
    
    Output.Pos = float4(Input.Pos,1);
    Output.viewPos = mul( float4( Input.Pos, 1 ), g_mWorldView );
    Output.Norm = Input.Normal;
    Output.Tex = Input.Tex;
    Output.Bones = Input.Bones;
    Output.Weights = Input.Weights;
    
    SkinnedInfo vNext = SkinVertBlur( Output, MID_TIME_STEP+1 );
    SkinnedInfo vPrev = SkinVertBlur( Output, MID_TIME_STEP-1 );
    float4 nextPos = mul( vNext.Pos, g_mBlurViewProj[ MID_TIME_STEP+1 ] );
    float4 prevPos = mul( vPrev.Pos, g_mBlurViewProj[ MID_TIME_STEP-1 ] );
    Output.Color.a = ComputeMotionAlpha( nextPos.xyz/nextPos.w, prevPos.xyz/prevPos.w, MIN_FADEGEOM );
    
    return Output;
}

void ExtrudeSkinnedEdge( VSSkinnedMotionBlurOut v1, 
                  VSSkinnedMotionBlurOut v2, 
                  uniform uint iStep,
                  inout TriangleStream<GSSkinnedMotionBlurOut> SceneTriangleStream )
{
    GSSkinnedMotionBlurOut oV1;
    GSSkinnedMotionBlurOut oV2;
    oV1.Tex = v1.Tex;
    oV2.Tex = v2.Tex;
    matrix mViewProj = g_mBlurViewProj[ iStep ];
    float fAlpha = ((float)iStep - MID_TIME_STEP)/(float)MID_TIME_STEP;
    float fA1 = v1.Color.a;
    float fA2 = v2.Color.a;
    
    SkinnedInfo one = SkinVertBlur( v1, iStep );
    oV1.Pos = mul( one.Pos, mViewProj );
    oV1.Color = ComputeLighting( one.Norm );
    
    SkinnedInfo two = SkinVertBlur( v2, iStep );
    oV2.Pos = mul( two.Pos, mViewProj );
    oV2.Color = ComputeLighting( two.Norm );
    
    oV1.Color.a = (1.0 - abs( fAlpha ) )*fA1;
    oV2.Color.a = (1.0 - abs( fAlpha ) )*fA2;
    SceneTriangleStream.Append( oV2 );
    SceneTriangleStream.Append( oV1 );
}

void ExtrudeSkinnedBlurEdges( VSSkinnedMotionBlurOut v1,
                              VSSkinnedMotionBlurOut v2,
                              inout TriangleStream<GSSkinnedMotionBlurOut> SceneTriangleStream,
                              uniform bool FrontSide,
                              uniform uint iSteps )
{
    uint iStart = 0;
    uint iEnd = MAX_TIME_STEPS;
    
    for(uint i=iStart; i<iEnd; i++)
    {       
        ExtrudeSkinnedEdge( v1, v2, i, SceneTriangleStream );
    }

    SceneTriangleStream.RestartStrip();
}

//output 2*3*MAX_TIME_STEPS
[maxvertexcount(18)]
void GSSkinnedMotionBlurMain( triangle VSSkinnedMotionBlurOut In[3], 
                       inout TriangleStream<GSSkinnedMotionBlurOut> SceneTriangleStream, 
                       uniform bool bFrontSide )
{
    // find the triangle normal in view space
    float3 viewNorm = cross( In[1].viewPos - In[0].viewPos, In[2].viewPos - In[0].viewPos );
    
    // only extrude any of our edges if we're facing the camera
    if( dot( viewNorm, float3(0,0,-1) ) > 0.0 )
    {
        ExtrudeSkinnedBlurEdges( In[0], In[1], SceneTriangleStream, bFrontSide, g_iNumSteps );
        ExtrudeSkinnedBlurEdges( In[1], In[1], SceneTriangleStream, bFrontSide, g_iNumSteps );
        ExtrudeSkinnedBlurEdges( In[1], In[0], SceneTriangleStream, bFrontSide, g_iNumSteps );
    }
}

float4 PSSkinnedMotionBlurMain( GSSkinnedMotionBlurOut Input ) : SV_TARGET
{
    float4 diff = g_txDiffuse.SampleLevel( g_samLinear, Input.Tex, 0 );
    diff.a = 1;
    return diff*Input.Color;
}

//--------------------------------------------------------------------------------------
// Techniques
//--------------------------------------------------------------------------------------


technique10 RenderScene
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, VSSceneMain() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PSSceneMain() ) );
        
        SetRasterizerState( RasMultiSampleBack );
        SetDepthStencilState( DepthTestNormal, 0 );
        SetBlendState( SrcAlphaBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
    }
};

technique10 RenderSkinnedScene
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, VSSkinnedSceneMain() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PSSceneMain() ) );
        
        SetRasterizerState( RasMultiSampleBack );
        SetDepthStencilState( DepthTestNormal, 0 );
        SetBlendState( SrcAlphaBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
    }
};

technique10 RenderMotionBlur
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, VSMotionBlurMain() ) );
        SetGeometryShader( CompileShader( gs_4_0, GSMotionBlurMain( true ) ) );
        SetPixelShader( CompileShader( ps_4_0, PSMotionBlurMain() ) );

        SetRasterizerState( RasMultiSampleNone );
        SetDepthStencilState( DepthTestNormal, 0 );
        SetBlendState( SrcAlphaBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
    }
};

technique10 RenderSkinnedMotionBlur
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, VSSkinnedMotionBlurMain() ) );
        SetGeometryShader( CompileShader( gs_4_0, GSSkinnedMotionBlurMain( true ) ) );
        SetPixelShader( CompileShader( ps_4_0, PSSkinnedMotionBlurMain() ) );

        SetRasterizerState( RasMultiSampleNone );
        SetDepthStencilState( DepthTestLessEqual, 0 );
        SetBlendState( SrcAlphaBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
    }
};