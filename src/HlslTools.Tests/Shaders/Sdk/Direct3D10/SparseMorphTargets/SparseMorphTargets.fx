//--------------------------------------------------------------------------------------
// File: SparseMorphTargets.fx
//
// Copyright (c) Microsoft Corporation. All rights reserved.
//--------------------------------------------------------------------------------------
struct VSQuadStackIn
{
    float3 pos          : POSITION;         
    float2 tex          : TEXTURE;          
};

struct VSSceneRefIn
{
    uint uiVertexRef    : REFERENCE;        
    float2 tex          : TEXTURE;          
    
    float4 redblock     : COEFFSET0;
    float4 greenblock   : COEFFSET1;
    float4 blueblock    : COEFFSET2;
    float4 rgrest       : COEFFSET3;
    float2 brest        : COEFFSET4;
};

struct VSSceneIn
{
    float3 pos          : POSITION;         
    float3 norm         : NORMAL;           
    float2 tex          : TEXTURE;          
};

struct VSQuadOut
{
    float4 pos          : SV_POSITION;     
    float3 tex          : TEXCOORD;     
};

struct GSQuadOut
{
    float4 pos          : SV_POSITION;     
    float3 tex          : TEXCOORD;      
    float3 maxdelta		: TEXCOORD1; 
    uint RTIndex        : SV_RenderTargetArrayIndex;
};

struct PSGridIn
{
    float4 pos          : SV_Position;
    float2 tex          : TEXTURE0;
};

struct GSRefMeshIn
{
    float4 pos          : SV_Position;
    float2 tex          : TEXTURE0;
    float3 wTan         : TEXTURE1;
    float3 wNorm        : TEXTURE2;
    float3 posOrig      : TEXTURE3;
    
    float4 redblock     : TEXTURE4;
    float4 greenblock   : TEXTURE5;
    float4 blueblock    : TEXTURE6;
    float4 rgrest       : TEXTURE7;
    float2 brest        : TEXTURE8;
};

struct PSRefMeshIn
{
    float4 pos          : SV_Position;
    float2 tex          : TEXTURE0;
    float3 wTan         : TEXTURE1;
    float3 wNorm        : TEXTURE2;
    float wrinkle       : WRINKLE;
    
    float4 redblock     : TEXTURE3;
    float4 greenblock   : TEXTURE4;
    float4 blueblock    : TEXTURE5;
    float4 rgrest       : TEXTURE6;
    float2 brest        : TEXTURE7;
};

struct PSSceneIn
{
    float4 pos          : SV_Position;      
    float3 norm         : NORMAL;               
    float2 tex          : TEXTURE;          
};

cbuffer cbOnce
{
    uint g_DataTexSize;     // Size of the data textures
    float3 g_vLightDir = float3(0.0f,0.707f,-0.707f);
};

cbuffer cbPerFrame
{
    float4x4 g_mWorldViewProj;
    float4x4 g_mWorld;
    float3 g_vCameraPos;
    float g_fOily;
};

cbuffer cbManyPerFrame
{
    float g_fBlendAmt;
    float3 g_vMaxDeltas[3];
    uint g_RT;
};

cbuffer SHLights
{
    // light coefficients
    float4 RLight[9];
    float4 GLight[9];
    float4 BLight[9];
};

//data textures
Texture2DArray<float3> g_txVertData;
Texture2DArray<float3> g_txVertDataOrig;

//prt textures
TextureCube CLinBF;   // bf for first 4 - could be done in shader

TextureCube QuadBF;   // bf 4-
TextureCube CubeBFA;  // includes one quadratic BF
TextureCube CubeBFB;  // final 4 cubics

TextureCube QuarBFA;  // first four quartics
TextureCube QuarBFB;  // next four quartics
TextureCube QuarBFC;  // final quartic + 1st 3 quintics

TextureCube QuinBFA;  // quintics
TextureCube QuinBFB;

//visual textures
Texture2D g_txDiffuse;
Texture2D g_txNormal;
TextureCube g_txEnvMap;

SamplerState g_samPointCube
{
    Filter = MIN_MAG_MIP_POINT;
    AddressU = Clamp;
    AddressV = Clamp;
};

SamplerState g_samLinearCube
{
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = Clamp;
    AddressV = Clamp;
};

#if USE_POINT_CUBE_SAMPLING == 1
#define g_samLDPRTFilter g_samLinearCube
#else
#define g_samLDPRTFilter g_samPointCube
#endif

SamplerState g_samLinearClamp
{
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = Clamp;
    AddressV = Clamp;
};

SamplerState g_samPointClamp
{
    Filter = MIN_MAG_MIP_POINT;
    AddressU = Clamp;
    AddressV = Clamp;
};

SamplerState g_samPointWrap
{
    Filter = MIN_MAG_MIP_POINT;
    AddressU = Wrap;
    AddressV = Wrap;
};

SamplerState g_samLinearWrap
{
    Filter = ANISOTROPIC;
    AddressU = Wrap;
    AddressV = Wrap;
};

BlendState AdditiveBlending
{
    AlphaToCoverageEnable = FALSE;
    BlendEnable[0] = TRUE;
    BlendEnable[1] = TRUE;
    BlendEnable[2] = TRUE;
    SrcBlend = SRC_ALPHA;
    DestBlend = ONE;
    BlendOp = ADD;
    SrcBlendAlpha = ZERO;
    DestBlendAlpha = ZERO;
    BlendOpAlpha = ADD;
    RenderTargetWriteMask[0] = 0x0F;
    RenderTargetWriteMask[1] = 0x0F;
    RenderTargetWriteMask[2] = 0x0F;
};

BlendState AdditiveAlphaBlending
{
    AlphaToCoverageEnable = FALSE;
    BlendEnable[0] = TRUE;
    BlendEnable[1] = TRUE;
    BlendEnable[2] = TRUE;
    SrcBlend = SRC_ALPHA;
    DestBlend = ONE;
    BlendOp = ADD;
    SrcBlendAlpha = ZERO;
    DestBlendAlpha = ZERO;
    BlendOpAlpha = ADD;
    RenderTargetWriteMask[0] = 0x0F;
    RenderTargetWriteMask[1] = 0x0F;
    RenderTargetWriteMask[2] = 0x0F;
};

BlendState NoBlending
{
    AlphaToCoverageEnable = FALSE;
    BlendEnable[0] = FALSE;
    BlendEnable[1] = FALSE;
    BlendEnable[2] = FALSE;
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

RasterizerState DisableCulling
{
    CullMode = NONE;
};

RasterizerState EnableCulling
{
    CullMode = BACK;
};

//
// VSGrid
//
VSQuadOut VSGrid( VSQuadStackIn input )
{
    VSQuadOut output = (VSQuadOut)0.0;

    //output our final position
    output.pos.xy = input.pos.xy;   // Input comes in [-1..1] range
    output.pos.z = 0.5f;
    output.pos.w = 1;
    
    output.tex = float3(input.tex,0);
    return output;
}

//
// VS2DShow
//
VSQuadOut VSG2DShow( VSQuadStackIn input )
{
    VSQuadOut output = (VSQuadOut)0.0;

    //output our final position
    output.pos.xy = input.pos.xy;   // Input comes in [-1..1] range
    output.pos.z = 0.5f;
    output.pos.w = 1;
    
    output.tex = float3(input.tex,g_RT);
    return output;
}


//
// VS for drawing a mesh from a set of textures containing the vertex data
//
GSRefMeshIn VSRefScenemain(VSSceneRefIn input)
{
    GSRefMeshIn output = (GSRefMeshIn)0.0;

    // Find out which texel holds our data
    uint iYCoord = input.uiVertexRef / g_DataTexSize;
    uint iXCoord = input.uiVertexRef%g_DataTexSize;
    float3 dataTexcoord = float3( iXCoord, iYCoord, 0 );
    dataTexcoord += float4(0.5,0.5,0,0);
    dataTexcoord.x /= (float)g_DataTexSize;
    dataTexcoord.y /= (float)g_DataTexSize;
    
    // Find our original position (used later for the wrinkle map)
    float3 OrigPos = g_txVertDataOrig.SampleLevel( g_samPointClamp, dataTexcoord, 0 );
    dataTexcoord.y = 1.0f - dataTexcoord.y;
    
    // Find our position, normal, and tangent
    float3 pos = g_txVertData.SampleLevel( g_samPointClamp, dataTexcoord, 0 );
    dataTexcoord.z = 1.0f;
    float3 norm = g_txVertData.SampleLevel( g_samPointClamp, dataTexcoord, 0 );
    dataTexcoord.z = 2.0f;
    float3 tangent = g_txVertData.SampleLevel( g_samPointClamp, dataTexcoord, 0 );
    
    // Output our final positions in clipspace
    output.pos = mul( float4( pos, 1 ), g_mWorldViewProj );
    output.posOrig = mul( float4( OrigPos, 1 ), g_mWorldViewProj );
    
    // Normal and tangent in world space
    output.wNorm = normalize( mul( norm, (float3x3)g_mWorld ) );
    output.wTan = normalize( mul( tangent, (float3x3)g_mWorld ) );
    
    // Just copy ldprt coefficients
    output.redblock = input.redblock;
    output.greenblock = input.greenblock;
    output.blueblock = input.blueblock;
    output.rgrest = input.rgrest;
    output.brest = input.brest;
    
    // Prop texture coordinates  
    output.tex = input.tex;
    
    return output;
}

// VS for rendering the scene
PSSceneIn VSScenemain( VSSceneIn input )
{
    PSSceneIn output;
    
    output.pos = mul( float4(input.pos,1), g_mWorldViewProj );
    output.norm = mul( input.norm, g_mWorld );
    output.tex = input.tex;
    
    return output;
}

[maxvertexcount(9)]
void GSReplicateRTs( triangle VSQuadOut input[3], inout TriangleStream<GSQuadOut> QuadStream )
{
    for(uint rt=0; rt<3; rt++)
    {
        for(int v=0; v<3; v++)
        {
            GSQuadOut output;
            output.pos = input[v].pos;
            output.tex = input[v].tex;
            output.RTIndex = rt;
            output.tex.z = rt;
            output.maxdelta = g_vMaxDeltas[rt];
            QuadStream.Append(output);
        }
        QuadStream.RestartStrip();
    }
}

[maxvertexcount(3)]
void GSCalcWrinkles( triangle GSRefMeshIn input[3], inout TriangleStream<PSRefMeshIn> RefStream )
{
    // Find the area of our triangle
    float3 vortho = cross( input[1].pos-input[0].pos, input[2].pos-input[0].pos );
    float areaNow = length( vortho ) / 2.0;
    
    vortho = cross( input[1].posOrig-input[0].posOrig, input[2].posOrig-input[0].posOrig );
    float areaOrig = length( vortho ) / 2.0;
    
    for(int v=0; v<3; v++)
    {
        PSRefMeshIn output;
        output.pos = input[v].pos;
        output.tex = input[v].tex;
        output.wTan = input[v].wTan;
        output.wNorm = input[v].wNorm;
        output.redblock = input[v].redblock;
        output.greenblock = input[v].greenblock;
        output.blueblock = input[v].blueblock;
        output.rgrest = input[v].rgrest;
        output.brest = input[v].brest;
    
        float w = ((areaOrig-areaNow)/ areaOrig)*1.0;
        if( w < 0 )
            w *= 0.005f;
        output.wrinkle = saturate( 0.3 + w );
        
        RefStream.Append(output);
    }
    
    RefStream.RestartStrip();
}

//
// PS for 2D render to texture operations
//
float4 PS2DRTT(GSQuadOut input) : SV_Target
{   
    float3 texel = g_txVertData.Sample( g_samPointClamp, input.tex );
    texel.x *= input.maxdelta.x;
    texel.y *= input.maxdelta.y;
    texel.z *= input.maxdelta.z;
    float4 color = float4(texel, g_fBlendAmt);
    return color;
}

//
// PS for showing a 2d texture
//
float4 PSShow2D(VSQuadOut input) : SV_Target
{   
    float3 color = g_txVertData.Sample( g_samPointClamp, input.tex ) / g_fBlendAmt;
    return float4(color,1);
}

float4 GetLDPRTColor( float3 wNorm, float4 redblock, float4 greenblock, float4 blueblock, float4 rgrest, float2 brest )
{
    float4 clrOut=1; // output color
    
    //float4 vNrm = float4(normalize(In.Normal),1);
    float4 CLin = CLinBF.Sample(g_samLDPRTFilter,wNorm);
    
    clrOut.r = dot(CLin*redblock.xyyy,RLight[0]);
    clrOut.g = dot(CLin*greenblock.xyyy,GLight[0]);
    clrOut.b = dot(CLin*blueblock.xyyy,BLight[0]); 
    
    float4 QuadCubeA,QuadCubeB,QuadCubeC;
    
    // sample the cube maps for quadratic/cubic...
    QuadCubeA = QuadBF.Sample(g_samLDPRTFilter,wNorm);
    QuadCubeB = CubeBFA.Sample(g_samLDPRTFilter,wNorm);
    QuadCubeC = CubeBFB.Sample(g_samLDPRTFilter,wNorm);
    
    clrOut.r += dot(QuadCubeA*redblock.z,RLight[1]);
    clrOut.g += dot(QuadCubeA*greenblock.z,GLight[1]);
    clrOut.b += dot(QuadCubeA*blueblock.z,BLight[1]);
    
    clrOut.r += dot(QuadCubeB*redblock.zwww,RLight[2]);
    clrOut.g += dot(QuadCubeB*greenblock.zwww,GLight[2]);
    clrOut.b += dot(QuadCubeB*greenblock.zwww,BLight[2]);
    
    clrOut.r += dot(QuadCubeC*redblock.w,RLight[3]);
    clrOut.g += dot(QuadCubeC*greenblock.w,GLight[3]);
    clrOut.b += dot(QuadCubeC*blueblock.w,BLight[3]);    

    clrOut.a = 1.0;
    return clrOut; 
}

//
// PS for rendering the reference mesh
//
float4 PSRefMeshmain( uniform bool bNdotL, PSRefMeshIn input ) : SV_Target
{       
    //diffuse
    float4 diffuse = g_txDiffuse.Sample( g_samLinearWrap, input.tex );
    
    //bump
    float3 bump = g_txNormal.Sample( g_samLinearWrap, input.tex );
    bump.xyz *= 2.0;
    bump.xyz -= float3(1,1,1);
    
    //move bump into world space for LDPRT lighting
    float3 binorm = normalize( cross( input.wNorm, input.wTan ) );
    float3x3 wtanMatrix = float3x3( binorm, input.wTan, input.wNorm );
    bump = mul( bump, wtanMatrix ); //world space bump
    
    //now lerp between world normal and bump map norml using the wrinkle amount
    float3 norm = lerp( input.wNorm, bump, input.wrinkle );
    
    //lighting
    float4 color;
    float4 specular;
    float specMap = diffuse.a;
    
    if(bNdotL)
    {
        color = dot(g_vLightDir,norm);
        float3 H = normalize( g_vLightDir + normalize(g_vCameraPos) );
        specular = specMap*saturate( pow( dot(H, norm ), 64 ) );
    }
    else
    {
        color = GetLDPRTColor( norm, input.redblock, input.greenblock, input.blueblock, input.rgrest, input.brest );
        float3 I = -normalize(g_vCameraPos);
        float3 wR = I - 2.0f * dot( I, norm ) * norm;
        specular = saturate(g_fOily*specMap*g_txEnvMap.SampleLevel( g_samLinearCube, wR, 6 )); 
    }
    
    
    
    //combined
    return diffuse*(color + specular);
}

//
// PS for scene
//
float4 PSScenemain(PSSceneIn input) : SV_Target
{       
    //normal
    float3 norm = input.norm;
    
    //lighting
    float4 color = dot(g_vLightDir,norm);
    
    //specular
    float3 I = -normalize(g_vCameraPos);
    float3 wR = I - 2.0f * dot( I, norm ) * norm;
    float4 specular = 0.2f*saturate(g_txEnvMap.Sample( g_samLinearCube, wR ));
    
    //diffuse
    float4 diffuse = g_txDiffuse.Sample( g_samLinearWrap, input.tex );
    
    //etc
    return diffuse*color + specular;
}

//
// RenderReferencedObject
//
technique10 RenderReferencedObject
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, VSRefScenemain() ) );
        SetGeometryShader( CompileShader( gs_4_0, GSCalcWrinkles() ) );
        SetPixelShader( CompileShader( ps_4_0, PSRefMeshmain( false ) ) );
        
        SetBlendState( NoBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( EnableDepth, 0 );
        SetRasterizerState( EnableCulling );
    }  
}

//
// Render2DQuad render a 2d quad for render to texture operations
//
technique10 Render2DQuad
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, VSGrid() ) );
        SetGeometryShader( CompileShader( gs_4_0, GSReplicateRTs() ) );
        SetPixelShader( CompileShader( ps_4_0, PS2DRTT() ) );
        
        SetBlendState( AdditiveBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( DisableDepth, 0 );
        SetRasterizerState( DisableCulling );
    }  
}

//
// Render2DQuad render a 2d quad for render to texture operations
//
technique10 Render2DQuadNoAlpha
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, VSGrid() ) );
        SetGeometryShader( CompileShader( gs_4_0, GSReplicateRTs() ) );
        SetPixelShader( CompileShader( ps_4_0, PS2DRTT() ) );
        
        SetBlendState( NoBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( DisableDepth, 0 );
        SetRasterizerState( DisableCulling );
    }  
}

//
// Show2DQuad show the result
//
technique10 Show2DQuad
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, VSG2DShow() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PSShow2D() ) );
        
        SetBlendState( NoBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( DisableDepth, 0 );
        SetRasterizerState( DisableCulling );
    }  
}

//
// RenderScene
//
technique10 RenderScene
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, VSScenemain() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PSScenemain() ) );
        
        SetBlendState( NoBlending, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
        SetDepthStencilState( EnableDepth, 0 );
        SetRasterizerState( EnableCulling );
    }  
}

