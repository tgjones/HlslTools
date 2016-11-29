// ShadowVolume10.fx
// Copyright (c) 2005 Microsoft Corporation. All rights reserved.
//

#define LIGHT_FALLOFF 1.2f
#define NOVERTEX 0xfffffffe

struct VSSceneIn
{
    float3 pos          : POSITION;    
    float3 norm         : NORMAL;  
    float2 tex          : TEXTURE0;
};

struct VSQuadIn
{
    float3 pos			: POSITION;
};

struct GSShadowIn
{
    float3 pos          : POS; 
    float3 norm         : TEXTURE0;
};

struct PSShadowIn
{
    float4 pos			: SV_Position;
};

struct PSQuadIn
{
    float4 pos			: SV_Position;
};

struct PSSceneIn
{
    float4 pos : SV_Position;
    float4 color : COLOR0; 
    float2 tex : TEXTURE0;
};

cbuffer cb1
{
    matrix g_mWorldViewProj;
    matrix g_mViewProj;
    matrix g_mWorld;
    float3 g_vLightPos;
    float4 g_vLightColor;
    float4 g_vAmbient;
    float g_fExtrudeAmt;
    float g_fExtrudeBias;
    float4 g_vShadowColor;
};

Texture2D g_txDiffuse;
SamplerState g_samLinear
{
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = Wrap;
    AddressV = Wrap;
};

DepthStencilState DisableDepth
{
    DepthEnable = FALSE;
    DepthWriteMask = ZERO;
};

DepthStencilState EnableDepth
{
    DepthEnable = TRUE;
    DepthWriteMask = ALL;
};

DepthStencilState TwoSidedStencil
{
    DepthEnable = true;
    DepthWriteMask = ZERO;
    DepthFunc = Less;
    
    // Setup stencil states
    StencilEnable = true;
    StencilReadMask = 0xFFFFFFFF;
    StencilWriteMask = 0xFFFFFFFF;
    
    BackFaceStencilFunc = Always;
    BackFaceStencilDepthFail = Incr;
    BackFaceStencilPass = Keep;
    BackFaceStencilFail = Keep;
    
    FrontFaceStencilFunc = Always;
    FrontFaceStencilDepthFail = Decr;
    FrontFaceStencilPass = Keep;
    FrontFaceStencilFail = Keep;
};

DepthStencilState VolumeComplexityStencil
{
    DepthEnable = true;
    DepthWriteMask = ZERO;
    DepthFunc = Less;
    
    // Setup stencil states
    StencilEnable = true;
    StencilReadMask = 0xFFFFFFFF;
    StencilWriteMask = 0xFFFFFFFF;
    
    BackFaceStencilFunc = Always;
    BackFaceStencilDepthFail = Incr;
    BackFaceStencilPass = Incr;
    BackFaceStencilFail = Incr;
    
    FrontFaceStencilFunc = Always;
    FrontFaceStencilDepthFail = Incr;
    FrontFaceStencilPass = Incr;
    FrontFaceStencilFail = Incr;
};

DepthStencilState ComplexityStencil
{
    DepthEnable = false;
    DepthWriteMask = ZERO;
    DepthFunc = Less;
    
    // Setup stencil states
    StencilEnable = true;
    StencilReadMask = 0xFFFFFFFF;
    StencilWriteMask = 0xFFFFFFFF;
    
    BackFaceStencilFunc = Always;
    BackFaceStencilDepthFail = Incr;
    BackFaceStencilPass = Keep;
    BackFaceStencilFail = Keep;
    
    FrontFaceStencilFunc = LESS_EQUAL;
    FrontFaceStencilDepthFail = Keep;
    FrontFaceStencilPass = Zero;
    FrontFaceStencilFail = Keep;
};

DepthStencilState RenderNonShadows
{
    DepthEnable = true;
    DepthWriteMask = ZERO;
    DepthFunc = Less_Equal;
    
    StencilEnable = true;
    StencilReadMask = 0xFFFFFFFF;
    StencilWriteMask = 0x0;
    
    FrontFaceStencilFunc = Equal;
    FrontFaceStencilPass = Keep;
    FrontFaceStencilFail = Zero;
    
    BackFaceStencilFunc = Never;
    BackFaceStencilPass = Zero;
    BackFaceStencilFail = Zero;
};

BlendState DisableFrameBuffer
{
    BlendEnable[0] = FALSE;
    RenderTargetWriteMask[0] = 0x0;
};

BlendState EnableFrameBuffer
{
    BlendEnable[0] = FALSE;
    RenderTargetWriteMask[0] = 0x0F;
};

BlendState NoBlending
{
    AlphaToCoverageEnable = FALSE;
    BlendEnable[0] = FALSE;
    RenderTargetWriteMask[0] = 0x0F;
};

BlendState AdditiveBlending
{
    AlphaToCoverageEnable = FALSE;
    BlendEnable[0] = TRUE;
    SrcBlend = ONE;
    DestBlend = ONE;
    BlendOp = ADD;
    SrcBlendAlpha = ZERO;
    DestBlendAlpha = ZERO;
    BlendOpAlpha = ADD;
    RenderTargetWriteMask[0] = 0x0F;
};

BlendState SrcAlphaBlending
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

RasterizerState DisableCulling
{
    CullMode = NONE;
};

RasterizerState EnableCulling
{
    CullMode = BACK;
};


//
// VS for rendering basic textured and lit objects
//
PSSceneIn VSScenemain( VSSceneIn input )
{
    PSSceneIn output = (PSSceneIn)0.0;

    //output our final position in clipspace
    output.pos = mul( float4( input.pos, 1 ), g_mWorldViewProj );
    
    //world space normal
    float3 norm = mul( input.norm, (float3x3)g_mWorld );

    //find the light dir
    float3 wpos = mul( input.pos, (float3x3)g_mWorld );
    
    float3 lightDir = normalize( g_vLightPos - wpos );
    float lightLenSq = dot( lightDir, lightDir );
        
    output.color = saturate(dot(lightDir,norm)) * 
                    (g_vLightColor/15.0f) *  
                    (( LIGHT_FALLOFF * LIGHT_FALLOFF) )/lightLenSq;
    
    //propogate the texture coordinate
    output.tex = input.tex;
    
    return output;
}

//
// VS for sending information to the shadow GS
//
GSShadowIn VSShadowmain( VSSceneIn input )
{
    GSShadowIn output = (GSShadowIn)0.0;

    //output our position in world space
    float4 pos = mul( float4(input.pos,1), g_mWorld );
    output.pos = pos.xyz;
    
    //world space normal
    output.norm = mul( input.norm, (float3x3)g_mWorld );
    
    return output;
}

//
// VS for rendering a fullscreen quad
//
PSQuadIn VSQuadMain( VSQuadIn input )
{
    PSQuadIn output;
    output.pos = float4(input.pos,1);
    return output;
}

//
// Helper to detect a silhouette edge and extrude a volume from it
//
void DetectAndProcessSilhouette( float3 N,         // Un-normalized triangle normal
                                 GSShadowIn v1,    // Shared vertex
                                 GSShadowIn v2,    // Shared vertex
                                 GSShadowIn vAdj,  // Adjacent triangle vertex
                                 inout TriangleStream<PSShadowIn> ShadowTriangleStream // triangle stream
                                 )
{    
    float3 NAdj = cross( v2.pos - vAdj.pos, v1.pos - vAdj.pos );
    
    float3 outpos[4];
    float3 extrude1 = normalize(v1.pos - g_vLightPos);
    float3 extrude2 = normalize(v2.pos - g_vLightPos);
        
    outpos[0] = v1.pos + g_fExtrudeBias*extrude1;
    outpos[1] = v1.pos + g_fExtrudeAmt*extrude1;
    outpos[2] = v2.pos + g_fExtrudeBias*extrude2;
    outpos[3] = v2.pos + g_fExtrudeAmt*extrude2;
        
    // Extrude silhouette to create two new triangles
    PSShadowIn Out;
    for(int v=0; v<4; v++)
    {
        Out.pos = mul( float4(outpos[v],1), g_mViewProj );
        ShadowTriangleStream.Append( Out );
    }
    ShadowTriangleStream.RestartStrip();
}

//
// GS for generating shadow volumes
//
[maxvertexcount(18)]
void GSShadowmain( triangleadj GSShadowIn In[6], inout TriangleStream<PSShadowIn> ShadowTriangleStream )
{
    // Compute un-normalized triangle normal
    float3 N = cross( In[2].pos - In[0].pos, In[4].pos - In[0].pos );
    
    // Compute direction from this triangle to the light
    float3 lightDir = g_vLightPos - In[0].pos;
    
    //if we're facing the light
    if( dot(N, lightDir) > 0.0f )
    {
        // For each edge of the triangle, determine if it is a silhouette edge
        DetectAndProcessSilhouette( lightDir, In[0], In[2], In[1], ShadowTriangleStream );
        DetectAndProcessSilhouette( lightDir, In[2], In[4], In[3], ShadowTriangleStream );
        DetectAndProcessSilhouette( lightDir, In[4], In[0], In[5], ShadowTriangleStream );
        
        //near cap
        PSShadowIn Out;
        for(int v=0; v<6; v+=2)
        {
            float3 extrude = normalize(In[v].pos - g_vLightPos);
            
            float3 pos = In[v].pos + g_fExtrudeBias*extrude;
            Out.pos = mul( float4(pos,1), g_mViewProj );
            ShadowTriangleStream.Append( Out );
        }
        ShadowTriangleStream.RestartStrip();
        
        //far cap (reverse the order)
        for(int v=4; v>=0; v-=2)
        {
            float3 extrude = normalize(In[v].pos - g_vLightPos);
        
            float3 pos = In[v].pos + g_fExtrudeAmt*extrude;
            Out.pos = mul( float4(pos,1), g_mViewProj );
            ShadowTriangleStream.Append( Out );
        }
        ShadowTriangleStream.RestartStrip();
    }
}

//
// PS for rendering lit and textured triangles
//
float4 PSScenemain(PSSceneIn input) : SV_Target
{   
    float4 diffuse = g_txDiffuse.Sample( g_samLinear, input.tex )*input.color;
    return diffuse;
}

//
// PS for rendering textured triangles
//
float4 PSTexmain(PSSceneIn input) : SV_Target
{   
    float4 diffuse = g_txDiffuse.Sample( g_samLinear, input.tex );
    return diffuse;
}

//
// PS for rendering ambient scene
//
float4 PSAmbientmain(PSSceneIn input) : SV_Target
{   
    float4 diffuse = g_txDiffuse.Sample( g_samLinear, input.tex );
    return diffuse*g_vAmbient;
}

//
// PS for rendering shadow scene
//
float4 PSShadowmain(PSShadowIn input) : SV_Target
{   
    return float4( g_vShadowColor.xyz, 0.1f );
}

//
// PS for rendering shadow complexity
//
float4 PSComplexity( uniform float4 Color ) : SV_Target
{
    return Color;
}

//
// RenderSceneTextured - renders textured primitives
//
technique10 RenderSceneTextured
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, VSScenemain() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PSTexmain() ) );
        
        SetBlendState( NoBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( EnableDepth, 0 );
        SetRasterizerState( EnableCulling );
    }  
}

//
// RenderSceneLit - renders textured primitives
//
technique10 RenderSceneLit
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, VSScenemain() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PSScenemain() ) );
        
        SetBlendState( AdditiveBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( RenderNonShadows, 0 ); //state, stencilref
        SetRasterizerState( EnableCulling );
    }  
}

//
// RenderShadow - extrudes shadows from geometry
//
technique10 RenderShadow
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, VSShadowmain() ) );
        SetGeometryShader( CompileShader( gs_4_0, GSShadowmain() ) );
        SetPixelShader( CompileShader( ps_4_0, PSShadowmain() ) );
        
        SetBlendState( DisableFrameBuffer, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( TwoSidedStencil, 1 ); //state, stencilref
        SetRasterizerState( DisableCulling );
    }  
}

//
// RenderSceneAmbient - renders the scene with ambient lighting
technique10 RenderSceneAmbient
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, VSScenemain() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PSAmbientmain() ) );
        
        SetBlendState( NoBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( EnableDepth, 0 );
        SetRasterizerState( EnableCulling );
    }  
}

technique10 ShowShadow
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, VSShadowmain() ) );
        SetGeometryShader( CompileShader( gs_4_0, GSShadowmain() ) );
        SetPixelShader( CompileShader( ps_4_0, PSShadowmain() ) );
        
        SetBlendState( SrcAlphaBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( TwoSidedStencil, 1 ); //state, stencilref
        SetRasterizerState( DisableCulling );
    }
}

technique10 RenderShadowVolumeComplexity
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, VSShadowmain() ) );
        SetGeometryShader( CompileShader( gs_4_0, GSShadowmain() ) );
        SetPixelShader( CompileShader( ps_4_0, PSShadowmain() ) );
        
        SetBlendState( NoBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( VolumeComplexityStencil, 1 ); //state, stencilref
        SetRasterizerState( DisableCulling );
    }
}

technique10 RenderComplexity
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, VSQuadMain() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PSComplexity( float4( 1.0f, 1.0f, 1.0f, 1.0f ) ) ) );
        
        SetBlendState( NoBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( ComplexityStencil, 71 );
        SetRasterizerState( EnableCulling );
    }
    pass p1
    {
        SetVertexShader( CompileShader( vs_4_0, VSQuadMain() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PSComplexity( float4( 1.0f, 0.0f, 0.0f, 1.0f ) ) ) );
        
        SetBlendState( NoBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( ComplexityStencil, 51 );
        SetRasterizerState( EnableCulling);
    }
    pass p2
    {
        SetVertexShader( CompileShader( vs_4_0, VSQuadMain() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PSComplexity( float4( 1.0f, 0.5f, 0.0f, 1.0f ) ) ) );
        
        SetBlendState( NoBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( ComplexityStencil, 41 );
        SetRasterizerState( EnableCulling );
    }
    pass p3
    {
        SetVertexShader( CompileShader( vs_4_0, VSQuadMain() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PSComplexity( float4( 1.0f, 1.0f, 0.0f, 1.0f ) ) ) );
        
        SetBlendState( NoBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( ComplexityStencil, 31 );
        SetRasterizerState( EnableCulling );
    }
    pass p4
    {
        SetVertexShader( CompileShader( vs_4_0, VSQuadMain() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PSComplexity( float4( 0.0f, 1.0f, 0.0f, 1.0f ) ) ) );
        
        SetBlendState( NoBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( ComplexityStencil, 21 );
        SetRasterizerState( EnableCulling );
    }
    pass p5
    {
        SetVertexShader( CompileShader( vs_4_0, VSQuadMain() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PSComplexity( float4( 0.0f, 1.0f, 1.0f, 1.0f ) ) ) );
        
        SetBlendState( NoBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( ComplexityStencil, 11 );
        SetRasterizerState( EnableCulling );
    }
    pass p6
    {
        SetVertexShader( CompileShader( vs_4_0, VSQuadMain() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PSComplexity( float4( 0.0f, 0.0f, 1.0f, 1.0f ) ) ) );
        
        SetBlendState( NoBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( ComplexityStencil, 6 );
        SetRasterizerState( EnableCulling );
    }
    pass p7
    {
        SetVertexShader( CompileShader( vs_4_0, VSQuadMain() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PSComplexity( float4( 1.0f, 0.0f, 1.0f, 1.0f ) ) ) );
        
        SetBlendState( NoBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( ComplexityStencil, 1 );
        SetRasterizerState( EnableCulling );
    }
    pass p8
    {
        SetVertexShader( CompileShader( vs_4_0, VSQuadMain() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PSComplexity( float4( 1.0f, 0.0f, 0.0f, 0.0f ) ) ) );
        
        SetBlendState( DisableFrameBuffer, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( DisableDepth, 0 );
        SetRasterizerState( DisableCulling );
    }
}

