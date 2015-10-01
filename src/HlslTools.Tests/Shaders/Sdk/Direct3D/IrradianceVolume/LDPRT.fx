//-----------------------------------------------------------------------------
// File: LDPRT.fx
//
// Desc: The technique RenderFullA renders the scene with convolution coefficients
// 
// Copyright (c) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// Global variables
//-----------------------------------------------------------------------------

float4x4 g_mWorldViewProjection;
float3x3 g_mNormalXform;

textureCUBE CLinBF;   // bf for first 4 - could be done in shader

textureCUBE QuadBF;   // bf 4-
textureCUBE CubeBFA;  // includes one quadratic BF
textureCUBE CubeBFB;  // final 4 cubics

textureCUBE QuarBFA;  // first four quartics
textureCUBE QuarBFB;  // next four quartics
textureCUBE QuarBFC;  // final quartic + 1st 3 quintics

textureCUBE QuinBFA;  // quintics
textureCUBE QuinBFB;

texture  AlbedoTexture;

sampler AlbedoSampler = 
sampler_state
{
    Texture = <AlbedoTexture>;
    MipFilter = LINEAR;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
};


sampler CLinBFSampler = sampler_state
{ 
    Texture = (CLinBF);
    MipFilter = LINEAR; 
    MinFilter = LINEAR;
    MagFilter = LINEAR;
};

sampler QuadBFSampler = sampler_state
{ 
    Texture = (QuadBF);
    MipFilter = LINEAR; 
    MinFilter = LINEAR;
    MagFilter = LINEAR;
};

sampler CubeBFASampler = sampler_state
{ 
    Texture = (CubeBFA);
    MipFilter = LINEAR; 
    MinFilter = LINEAR;
    MagFilter = LINEAR;
};

sampler CubeBFBSampler = sampler_state
{ 
    Texture = (CubeBFB);
    MipFilter = LINEAR; 
    MinFilter = LINEAR;
    MagFilter = LINEAR;
};

sampler QuarBFASampler = sampler_state
{ 
    Texture = (QuarBFA);
    MipFilter = LINEAR; 
    MinFilter = LINEAR;
    MagFilter = LINEAR;
};

sampler QuarBFBSampler = sampler_state
{ 
    Texture = (QuarBFB);
    MipFilter = LINEAR; 
    MinFilter = LINEAR;
    MagFilter = LINEAR;
};

sampler QuarBFCSampler = sampler_state
{ 
    Texture = (QuarBFC);
    MipFilter = LINEAR; 
    MinFilter = LINEAR;
    MagFilter = LINEAR;
};

sampler QuinBFASampler = sampler_state
{ 
    Texture = (QuinBFA);
    MipFilter = LINEAR; 
    MinFilter = LINEAR;
    MagFilter = LINEAR;
};

sampler QuinBFBSampler = sampler_state
{ 
    Texture = (QuinBFB);
    MipFilter = LINEAR; 
    MinFilter = LINEAR;
    MagFilter = LINEAR;
};

//-----------------------------------------------------------------------------
// Vertex shader output structure
//-----------------------------------------------------------------------------
struct VS_OUTPUT
{
    float4 Position   : POSITION;      // position of the vertex
    float3 Normal     : TEXCOORD0;     // normal in light space
    float2 TextureUV  : TEXCOORD1;     // typical texture coords stored here
    
    float4 RedBlock   : TEXCOORD2;     // red DC through cubics
    float4 GreenBlock : TEXCOORD3;     // green DC through cubics
    float4 BlueBlock  : TEXCOORD4;     // blue DC through cubics
    
    float4 RGRest     : TEXCOORD5;     // RG quartic/quintic bands
    float2 BRest      : TEXCOORD6;     // B quartic/quintic
};

struct VS_OUTPUT_CUBIC
{
    float4 Position   : POSITION;      // position of the vertex
    float3 Normal     : TEXCOORD0;     // normal in light space
    float2 TextureUV  : TEXCOORD1;     // typical texture coords stored here
    
    float4 RedBlock   : TEXCOORD2;     // red DC through cubics
    float4 GreenBlock : TEXCOORD3;     // green DC through cubics
    float4 BlueBlock  : TEXCOORD4;     // blue DC through cubics
};


VS_OUTPUT LDPRTVertexShader(float4 vPos  : POSITION,
                       float3 vNorm      : NORMAL,
                       float2 TextureUV  : TEXCOORD0,    
                       float4 RedBlock   : TEXCOORD1,    // red DC through cubics
                       float4 GreenBlock : TEXCOORD2,    // green DC through cubics
                       float4 BlueBlock  : TEXCOORD3,    // blue DC through cubics    
                       float4 RGRest     : TEXCOORD4,    // RG quartic/quintic bands
                       float2 BRest      : TEXCOORD5 )   // B quartic/quintic      
{
    VS_OUTPUT Output;

    Output.Position = mul(vPos, g_mWorldViewProjection);
    Output.Normal = mul(vNorm, g_mNormalXform);
    Output.RedBlock = RedBlock;
    Output.GreenBlock = GreenBlock;
    Output.BlueBlock = BlueBlock;
    Output.RGRest = RGRest;
    Output.BRest = BRest;
    Output.TextureUV = TextureUV;
    
    return Output;
}

VS_OUTPUT_CUBIC LDPRTCubicVS(float4 vPos  : POSITION,
                       float3 vNorm       : NORMAL,
                       float2 TextureUV   : TEXCOORD0,    
                       float4 RedBlock    : TEXCOORD1,    // red DC through cubics
                       float4 GreenBlock  : TEXCOORD2,    // green DC through cubics
                       float4 BlueBlock   : TEXCOORD3)    // blue DC through cubics    
{
    VS_OUTPUT_CUBIC Output;

    Output.Position = mul(vPos, g_mWorldViewProjection);
    Output.Normal = mul(vNorm, g_mNormalXform);
    Output.RedBlock = RedBlock;
    Output.GreenBlock = GreenBlock;
    Output.BlueBlock = BlueBlock;
    Output.TextureUV = TextureUV;
    
    return Output;
}

struct PS_OUTPUT
{
    float4 RGBColor : COLOR0;
};

// light coefficients
float4 RLight[9];
float4 GLight[9];
float4 BLight[9];

PS_OUTPUT LDPRTPixelShader( VS_OUTPUT In,
                            uniform bool bTexture )
{
    PS_OUTPUT outClr;
    
    float4 clrOut=1; // output color
    
    //float4 vNrm = float4(normalize(In.Normal),1);
    float4 CLin = texCUBE(CLinBFSampler,In.Normal);
    
    clrOut.r = dot(CLin*In.RedBlock.xyyy,RLight[0]);
    clrOut.g = dot(CLin*In.GreenBlock.xyyy,GLight[0]);
    clrOut.b = dot(CLin*In.BlueBlock.xyyy,BLight[0]); 
    
    float4 QuadCubeA,QuadCubeB,QuadCubeC;
    
    // sample the cube maps for quadratic/cubic...
    QuadCubeA = texCUBE(QuadBFSampler,In.Normal);
    QuadCubeB = texCUBE(CubeBFASampler,In.Normal);
    QuadCubeC = texCUBE(CubeBFBSampler,In.Normal);
    
    clrOut.r += dot(QuadCubeA*In.RedBlock.z,RLight[1]);
    clrOut.g += dot(QuadCubeA*In.GreenBlock.z,GLight[1]);
    clrOut.b += dot(QuadCubeA*In.BlueBlock.z,BLight[1]);
    
    clrOut.r += dot(QuadCubeB*In.RedBlock.zwww,RLight[2]);
    clrOut.g += dot(QuadCubeB*In.GreenBlock.zwww,GLight[2]);
    clrOut.b += dot(QuadCubeB*In.GreenBlock.zwww,BLight[2]);
    
    clrOut.r += dot(QuadCubeC*In.RedBlock.w,RLight[3]);
    clrOut.g += dot(QuadCubeC*In.GreenBlock.w,GLight[3]);
    clrOut.b += dot(QuadCubeC*In.BlueBlock.w,BLight[3]);    
 
    // cubics are done - now just sample the rest of the basis functions...
    
    float4 QuarQuinA,QuarQuinB,QuarQuinC,QuarQuinD,QuarQuinE;
    
    QuarQuinA = texCUBE( QuarBFASampler, In.Normal );
    QuarQuinB = texCUBE( QuarBFBSampler, In.Normal );
    QuarQuinC = texCUBE( QuarBFCSampler, In.Normal );
    QuarQuinD = texCUBE( QuinBFASampler, In.Normal );
    QuarQuinE = texCUBE( QuinBFBSampler, In.Normal );
    
    clrOut.r += dot(QuarQuinA*In.RGRest.x, RLight[4]);
    clrOut.g += dot(QuarQuinA*In.RGRest.z, GLight[4]);
    clrOut.b += dot(QuarQuinA*In.BRest.x, BLight[4]);
    
    clrOut.r += dot(QuarQuinB*In.RGRest.x, RLight[5]);
    clrOut.g += dot(QuarQuinB*In.RGRest.z, BLight[5]);
    clrOut.b += dot(QuarQuinB*In.BRest.x, BLight[5]);    

    clrOut.r += dot(QuarQuinC*In.RGRest.xyyy, RLight[6]);
    clrOut.g += dot(QuarQuinC*In.RGRest.zwww, BLight[6]);
    clrOut.b += dot(QuarQuinC*In.BRest.xyyy, BLight[6]);  

    clrOut.r += dot(QuarQuinD*In.RGRest.y, RLight[7]);
    clrOut.g += dot(QuarQuinD*In.RGRest.w, BLight[7]);
    clrOut.b += dot(QuarQuinD*In.BRest.y, BLight[7]); 
    
    clrOut.r += dot(QuarQuinE*In.RGRest.y, RLight[8]);
    clrOut.g += dot(QuarQuinE*In.RGRest.w, GLight[8]);
    clrOut.b += dot(QuarQuinE*In.BRest.y, BLight[8]);    

	if( bTexture )
		outClr.RGBColor = tex2D(AlbedoSampler, In.TextureUV) * clrOut;
	else
		outClr.RGBColor = clrOut;
    
    return outClr; 
}



PS_OUTPUT LDPRTCubicPS( VS_OUTPUT_CUBIC In,
                      uniform bool bTexture )
{
    PS_OUTPUT outClr;
    
    float4 clrOut=1; // output color
    
    //float4 vNrm = float4(normalize(In.Normal),1);
    
    float4 CLin = texCUBE(CLinBFSampler,In.Normal);
    
    clrOut.r = dot(CLin*In.RedBlock.xyyy,RLight[0]);
    clrOut.g = dot(CLin*In.GreenBlock.xyyy,GLight[0]);
    clrOut.b = dot(CLin*In.BlueBlock.xyyy,BLight[0]);     

    float4 QuadCubeA,QuadCubeB,QuadCubeC;
    
    // sample the cube maps for quadratic/cubic...
    QuadCubeA = texCUBE(QuadBFSampler,In.Normal);
    QuadCubeB = texCUBE(CubeBFASampler,In.Normal);
    QuadCubeC = texCUBE(CubeBFBSampler,In.Normal);
    
    clrOut.r += dot(QuadCubeA*In.RedBlock.z,RLight[1]);
    clrOut.g += dot(QuadCubeA*In.GreenBlock.z,GLight[1]);
    clrOut.b += dot(QuadCubeA*In.BlueBlock.z,BLight[1]);
    
    clrOut.r += dot(QuadCubeB*In.RedBlock.zwww,RLight[2]);
    clrOut.g += dot(QuadCubeB*In.GreenBlock.zwww,GLight[2]);
    clrOut.b += dot(QuadCubeB*In.BlueBlock.zwww,BLight[2]);
    
    clrOut.r += dot(QuadCubeC*In.RedBlock.w,RLight[3]);
    clrOut.g += dot(QuadCubeC*In.GreenBlock.w,GLight[3]);
    clrOut.b += dot(QuadCubeC*In.BlueBlock.w,BLight[3]);    
 
    // cubics are done - now just sample the rest of the basis functions...
	if( bTexture )
		outClr.RGBColor = tex2D(AlbedoSampler, In.TextureUV) * clrOut;
	else
		outClr.RGBColor = clrOut;    
    
    return outClr; 
}

technique RenderCubic
{
    pass p0
    {
        VertexShader = compile vs_2_0 LDPRTCubicVS();
        PixelShader  = compile ps_2_0 LDPRTCubicPS( false );
    }
}


technique RenderFullA
{
    pass p0
    {
        VertexShader = compile vs_2_0 LDPRTVertexShader();
        PixelShader  = compile ps_3_0 LDPRTPixelShader( false );
    }
}

technique RenderFullB
{
    pass p0
    {
        VertexShader = compile vs_2_0 LDPRTVertexShader();
        PixelShader  = compile ps_2_b LDPRTPixelShader( false );
    }
}

technique RenderCubicWithTexture
{
    pass p0
    {
        VertexShader = compile vs_2_0 LDPRTCubicVS();
        PixelShader  = compile ps_2_0 LDPRTCubicPS( true );
    }
}


technique RenderFullAWithTexture
{
    pass p0
    {
        VertexShader = compile vs_2_0 LDPRTVertexShader();
        PixelShader  = compile ps_3_0 LDPRTPixelShader( true );
    }
}

technique RenderFullBWithTexture
{
    pass p0
    {
        VertexShader = compile vs_2_0 LDPRTVertexShader();
        PixelShader  = compile ps_2_b LDPRTPixelShader( true );
    }
}



