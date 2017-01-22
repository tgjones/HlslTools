/*********************************************************************NVMH3****
*******************************************************************************
$Revision: #3 $

Copyright NVIDIA Corporation 2008
TO THE MAXIMUM EXTENT PERMITTED BY APPLICABLE LAW, THIS SOFTWARE IS PROVIDED
*AS IS* AND NVIDIA AND ITS SUPPLIERS DISCLAIM ALL WARRANTIES, EITHER EXPRESS
OR IMPLIED, INCLUDING, BUT NOT LIMITED TO, IMPLIED WARRANTIES OF MERCHANTABILITY
AND FITNESS FOR A PARTICULAR PURPOSE.  IN NO EVENT SHALL NVIDIA OR ITS SUPPLIERS
BE LIABLE FOR ANY SPECIAL, INCIDENTAL, INDIRECT, OR CONSEQUENTIAL DAMAGES
WHATSOEVER (INCLUDING, WITHOUT LIMITATION, DAMAGES FOR LOSS OF BUSINESS PROFITS,
BUSINESS INTERRUPTION, LOSS OF BUSINESS INFORMATION, OR ANY OTHER PECUNIARY
LOSS) ARISING OUT OF THE USE OF OR INABILITY TO USE THIS SOFTWARE, EVEN IF
NVIDIA HAS BEEN ADVISED OF THE POSSIBILITY OF SUCH DAMAGES.

% A helpful tool for artists to optimize their texture sizes, so that
%    textures created by artists wont be too small (looking bad) or
%    too big (wasting artist time on texture detail that will never be seen).

% HOW TO USE UVDETECTIVE:
%    (1) Look for regions where desired texture reso is dominant.
%    (2) Set desired size in the "Reso" parameter.
%    (3) In the "TexRez" techniques

keywords: DirectX10
// Note that this version has twin versions of all techniques,
//   so that this single effect file can be used in *either*
//   DirectX9 or DirectX10

To learn more about shading, shaders, and to bounce ideas off other shader
    authors and users, visit the NVIDIA Shader Library Forums at:

    http://developer.nvidia.com/forums/

*******************************************************************************
******************************************************************************/

/*****************************************************************/
/*** HOST APPLICATION IDENTIFIERS ********************************/
/*** Potentially predefined by varying host environments *********/
/*****************************************************************/

// #define _XSI_		/* predefined when running in XSI */
// #define TORQUE		/* predefined in TGEA 1.7 and up */
// #define _3DSMAX_		/* predefined in 3DS Max */
#ifdef _3DSMAX_
int ParamID = 0x0003;		/* Used by Max to select the correct parser */
#endif /* _3DSMAX_ */
#ifdef _XSI_
#define Main Static		/* Technique name used for export to XNA */
#endif /* _XSI_ */

#ifndef FXCOMPOSER_VERSION	/* for very old versions */
#define FXCOMPOSER_VERSION 180
#endif /* FXCOMPOSER_VERSION */

#ifndef DIRECT3D_VERSION
#define DIRECT3D_VERSION 0x900
#endif /* DIRECT3D_VERSION */

#define FLIP_TEXTURE_Y	/* Different in OpenGL & DirectX */

/*****************************************************************/
/*** EFFECT-SPECIFIC CODE BEGINS HERE ****************************/
/*****************************************************************/
//
// Un-Comment the PROCEDURAL_TEXTURE macro to enable texture generation in
//      DirectX9 ONLY
// DirectX10 may not issue errors, but will generate no texture either
//
// #define PROCEDURAL_TEXTURE
//

/**** UNTWEAKABLES: Hidden & Automatically-Tracked Parameters **********/

// transform object vertices to world-space:
float4x4 gWorldXf : World < string UIWidget="None"; >;
// transform object normals, tangents, & binormals to world-space:
float4x4 gWorldITXf : WorldInverseTranspose < string UIWidget="None"; >;
// transform object vertices to view space and project them in perspective:
float4x4 gWvpXf : WorldViewProjection < string UIWidget="None"; >;
// provide tranform from "view" or "eye" coords back to world-space:
float4x4 gViewIXf : ViewInverse < string UIWidget="None"; >; 

/************************************************************/
/*** TWEAKABLES *********************************************/
/************************************************************/

int TexReso <
    string UIName = "Anticipated Texture Reso";
> = 128;

static float TargetDeriv = 1.0f/((float)TexReso);
static float HalfTD = (TargetDeriv*0.5);
static float TwoTD = (TargetDeriv*2.0);
static float Diagonal = sqrt(2.0*TargetDeriv*TargetDeriv);
static float HalfDiag = (Diagonal*0.5);
static float TwoDiag = (Diagonal*2.0);
float Scale : UNITSSCALE <
    string UIWidget = "slider";
    float UIMin = 1;
    float UIMax = 128.0;
    float UIStep = 1;
    string UIName = "'uvDerivatives' Brightness";
> = 64.0;

/************ Texture ***********/

#ifdef PROCEDURAL_TEXTURE
// for generated MipTex
#define MIP_TEX_RESO 512

static float _TexLevels = (log(MIP_TEX_RESO)/log(2.0));

float4 mip_colors(float2 Pos:POSITION,float2 dP:PSIZE) : COLOR
{
    float d = 1.0/((dP.x+dP.y)/2.0);
    float3 p = 0;
    if (d < 8) {
	p = 1;
    } else {
	float q = log(d)/log(2);
	q = floor(q+.5);
	if (fmod(q,3) == 0) { p.x = .5;}
	if (fmod(q,6) == 0) { p.x = 1;}
	if (fmod(q,2) == 0) { p.y = .5;}
	if (fmod(q,4) == 0) { p.y = 1;}
	q = q / _TexLevels;
	p.z = q;
    }
    return float4(p,1);
}

texture gProcMipTex  <
    string TextureType = "2D";
    string function = "mip_colors";
    string UIWidget = "None";
    string UIName = "Mip-Level-Color Texture";
    int width = MIP_TEX_RESO;
    int height = MIP_TEX_RESO;
>;
#else /* ! PROCEDURAL_TEXTURE */
texture gMipTex  <
    string ResourceName = "uvDetective_512.dds";
    string ResourceType = "2D";
    string UIName = "Mip-Level-Color Texture";
>;
#endif /* ! PROCEDURAL_TEXTURE */

sampler2D gMipSamp = sampler_state 
{
#ifdef PROCEDUAL_TEXTURE
	Texture = <gProcMipTex>;
#else /* ! PROCEDURAL_TEXTURE */
	Texture = <gMipTex>;
#endif /* ! PROCEDURAL_TEXTURE */
#if DIRECT3D_VERSION >= 0xa00
    Filter = MIN_MAG_MIP_LINEAR;
#else /* DIRECT3D_VERSION < 0xa00 */
    MinFilter = Linear;
    MipFilter = Linear;
    MagFilter = Linear;
#endif /* DIRECT3D_VERSION */
    AddressU = Wrap;
    AddressV = Wrap;
};

/************* DATA STRUCTS **************/

/* data from application vertex buffer */
struct appdata {
    float3 Pos    : POSITION;
    float4 UV        : TEXCOORD0;
};

/* data passed from vertex shader to pixel shader */
struct uvVertexOutput {
    float4 HPosition    : POSITION;
    float2 UV    : TEXCOORD0;
};

/*********** vertex shader for all ******/

uvVertexOutput uvDetVS(appdata IN,
    uniform float4x4 WorldITXf, // our four standard "untweakable" xforms
	uniform float4x4 WorldXf,
	uniform float4x4 ViewIXf,
	uniform float4x4 WvpXf
) {
    uvVertexOutput OUT;
    float4 Po = float4(IN.Pos,1.0);
    OUT.HPosition = mul(Po,WvpXf);
    OUT.UV = IN.UV.xy;
    return OUT;
}

/********* utility functions for pixel shaders ********/

float4 vecColor(float4 V) {
    float3 Nc = 0.5 * ((V.xyz) + ((1.0).xxx));
    return float4(Nc,1);
}

float4 vecColor(float3 V) {
    float3 Nc = 0.5 * ((V.xyz) + ((1.0).xxx));
    return float4(Nc,1);
}

float4 vecColorN(float4 V) {
    float3 Nc = 0.5 * (normalize(V.xyz) + ((1.0).xxx));
    return float4(Nc,1);
}

float4 vecColorN(float3 V) {
    float3 Nc = 0.5 * (normalize(V.xyz) + ((1.0).xxx));
    return float4(Nc,1);
}

/********* pixel shaders ********/

float4 uvcPS(uvVertexOutput IN) : COLOR { return float4(IN.UV,0,1); }

float4 uvDerivsPS(uvVertexOutput IN) : COLOR
{
    float2 dd = Scale * (abs(ddx(IN.UV)) + abs(ddy(IN.UV)));
    return float4(dd,0,1);
}


float4 dv_col(float2 d)
{
    float4 dd = float4(0,0,0,1);
    if (d.x > TwoTD) { dd.x = 1.0;}
    if (d.y > TwoTD) { dd.y = 1.0;}
    if (d.x < HalfTD) { dd.z = 1.0;}
    return(dd);
}

float4 texSizeXPS(uvVertexOutput IN) : COLOR
{
    return dv_col(abs(ddx(IN.UV)));
}

float4 texSizeYPS(uvVertexOutput IN) : COLOR
{
    return dv_col(abs(ddy(IN.UV)));
}

float4 texSizeDPS(uvVertexOutput IN) : COLOR
{
    float2 dx = ddx(IN.UV);
    float2 dy = ddy(IN.UV);
    float d = sqrt(dot(dx,dx) + dot(dy,dy));
    float4 yc = dv_col(abs(ddy(IN.UV)));
    float4 dd = float4(0,0,0,1);
    if (d > TwoDiag) { dd.x = 1.0;}
    if (d > TwoDiag) { dd.y = 1.0;}
    if (d < HalfDiag) { dd.z = 1.0;}
    return(dd);
}

float4 mtPS(uvVertexOutput IN,uniform sampler2D MipSamp) : COLOR { return tex2D(MipSamp,IN.UV); }

///////////////////////////////////////
/// TECHNIQUES ////////////////////////
///////////////////////////////////////

#if DIRECT3D_VERSION >= 0xa00
//
// Standard DirectX10 Material State Blocks
//
RasterizerState DisableCulling { CullMode = NONE; };
DepthStencilState DepthEnabling { DepthEnable = TRUE; };
DepthStencilState DepthDisabling {
	DepthEnable = FALSE;
	DepthWriteMask = ZERO;
};
BlendState DisableBlend { BlendEnable[0] = FALSE; };

technique10 texRezDiagonal10 <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        SetVertexShader( CompileShader( vs_4_0, uvDetVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, texSizeDPS() ) );
	    SetRasterizerState(DisableCulling);
	    SetDepthStencilState(DepthEnabling, 0);
	    SetBlendState(DisableBlend, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF);
    }
}

#endif /* DIRECT3D_VERSION >= 0xa00 */

technique texRezDiagonal <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        VertexShader = compile vs_3_0 uvDetVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 texSizeDPS();
    }
}

#if DIRECT3D_VERSION >= 0xa00

technique10 texRezXOnly10 <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        SetVertexShader( CompileShader( vs_4_0, uvDetVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, texSizeXPS() ) );
	    SetRasterizerState(DisableCulling);
	    SetDepthStencilState(DepthEnabling, 0);
	    SetBlendState(DisableBlend, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF);
    }
}

#endif /* DIRECT3D_VERSION >= 0xa00 */

technique texRezXOnly <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        VertexShader = compile vs_3_0 uvDetVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 texSizeXPS();
    }
}

#if DIRECT3D_VERSION >= 0xa00

technique10 texRezYOnly10 <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        SetVertexShader( CompileShader( vs_4_0, uvDetVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, texSizeYPS() ) );
	    SetRasterizerState(DisableCulling);
	    SetDepthStencilState(DepthEnabling, 0);
	    SetBlendState(DisableBlend, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF);
    }
}

#endif /* DIRECT3D_VERSION >= 0xa00 */

technique texRezYOnly <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        VertexShader = compile vs_3_0 uvDetVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 texSizeYPS();
    }
}

#if DIRECT3D_VERSION >= 0xa00

technique10 uvValues10 <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        SetVertexShader( CompileShader( vs_4_0, uvDetVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, uvcPS() ) );
	    SetRasterizerState(DisableCulling);
	    SetDepthStencilState(DepthEnabling, 0);
	    SetBlendState(DisableBlend, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF);
    }
}

#endif /* DIRECT3D_VERSION >= 0xa00 */

technique uvValues <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        VertexShader = compile vs_3_0 uvDetVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 uvcPS();
    }
}

#if DIRECT3D_VERSION >= 0xa00

technique10 uvDerivatives10 <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        SetVertexShader( CompileShader( vs_4_0, uvDetVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, uvDerivsPS() ) );
	    SetRasterizerState(DisableCulling);
	    SetDepthStencilState(DepthEnabling, 0);
	    SetBlendState(DisableBlend, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF);
    }
}

#endif /* DIRECT3D_VERSION >= 0xa00 */

technique uvDerivatives <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        VertexShader = compile vs_3_0 uvDetVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 uvDerivsPS();
    }
}

#if DIRECT3D_VERSION >= 0xa00

technique10 mipTexture10 <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        SetVertexShader( CompileShader( vs_4_0, uvDetVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, mtPS(gMipSamp) ) );
	    SetRasterizerState(DisableCulling);
	    SetDepthStencilState(DepthEnabling, 0);
	    SetBlendState(DisableBlend, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF);
    }
}

#endif /* DIRECT3D_VERSION >= 0xa00 */

technique mipTexture <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        VertexShader = compile vs_3_0 uvDetVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 mtPS(gMipSamp);
    }
}

/***************************** eof ***/
