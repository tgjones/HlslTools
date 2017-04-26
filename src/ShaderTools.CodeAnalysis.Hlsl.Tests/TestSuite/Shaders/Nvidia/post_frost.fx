/*********************************************************************NVMH3****
*******************************************************************************
$Revision: #4 $

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

% An imaging effect that looked like viewing through ice-frosted glass.

keywords: image_processing virtual_machine
date:070220


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

// shared-surface access supported in Cg version
#include <include\\Quad.fxh>

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "scene";
    string ScriptOrder = "postprocess";
    string ScriptOutput = "color";
    string Script = "Technique=Technique?Main:Main10;";
> = 0.8;

// color and depth used for full-screen clears

float4 gClearColor <
    string UIWidget = "Color";
    string UIName = "Background";
> = {0,0,0,0};

float gClearDepth <string UIWidget = "none";> = 1.0;

/************* TWEAKABLES **************/

float gPixelX <
    string UIName = "X Texel Steps";
    string UIWidget = "slider";
    float UIMin = 1.0f;
    float UIMax = 5.0f;
    float UIStep = 0.5f;
> = 1.5f;

float gPixelY <
    string UIName = "Y Texel Steps";
    string UIWidget = "slider";
    float UIMin = 1.0f;
    float UIMax = 5.0f;
    float UIStep = 0.5f;
> = 1.5f;

float gFreq <
	string UIName = "Frequency";
	string UIWidget = "slider";
	float UIMin = 0.0;
	float UIMax = 0.2;
	float UIStep = 0.001;
> = 0.115f;

// #define NOISE2D_SCALE 500
#define NOISE2D_SCALE 12
#define NOISE_SHEET_SIZE 128
#include <include\\noise_2d.fxh>

///////////////////////////////////////////////////////////
/////////////////////////////////////// Textures //////////
///////////////////////////////////////////////////////////

DECLARE_QUAD_TEX(gSceneTexture,gSceneSampler,"A8R8G8B8")
DECLARE_QUAD_DEPTH_BUFFER(DepthBuffer,"D24S8")

//////////////////////////////////////////////////////
/////////////////////////////////// pixel shader /////
//////////////////////////////////////////////////////

float4 spline(float x, float4 c1, float4 c2, float4 c3, float4 c4, float4 c5, float4 c6, float4 c7, float4 c8, float4 c9) {
    float w1, w2, w3, w4, w5, w6, w7, w8, w9;
    w1 = 0;
    w2 = 0;
    w3 = 0;
    w4 = 0;
    w5 = 0;
    w6 = 0;
    w7 = 0;
    w8 = 0;
    w9 = 0;
    float tmp = x * 8.0;
    if (tmp<=1.0) {
      w1 = 1.0 - tmp;
      w2 = tmp;
    }
    else if (tmp<=2.0) {
      tmp = tmp - 1.0;
      w2 = 1.0 - tmp;
      w3 = tmp;
    }
    else if (tmp<=3.0) {
      tmp = tmp - 2.0;
      w3 = 1.0-tmp;
      w4 = tmp;
    }
    else if (tmp<=4.0) {
      tmp = tmp - 3.0;
      w4 = 1.0-tmp;
      w5 = tmp;
    }
    else if (tmp<=5.0) {
      tmp = tmp - 4.0;
      w5 = 1.0-tmp;
      w6 = tmp;
    }
    else if (tmp<=6.0) {
      tmp = tmp - 5.0;
      w6 = 1.0-tmp;
      w7 = tmp;
    }
    else if (tmp<=7.0) {
      tmp = tmp - 6.0;
      w7 = 1.0 - tmp;
      w8 = tmp;
    }
    else {
      tmp = saturate(tmp - 7.0);
      w8 = 1.0-tmp;
      w9 = tmp;
    }
    return w1*c1 + w2*c2 + w3*c3 + w4*c4 + w5*c5 + w6*c6 + w7*c7 + w8*c8 + w9*c9;
}

float4 frostedPS(QuadVertexOutput IN,
		uniform sampler2D SceneSampler,
		uniform float PixelX,
		uniform float PixelY,
		uniform float Freq,
		uniform sampler2D Noise2DSamp
) : COLOR {
    float DeltaX = (PixelX/QuadScreenSize.x);
    float DeltaY = (PixelY/QuadScreenSize.y);
    float2 ox = float2(DeltaX,0.0);
    float2 oy = float2(0.0,DeltaY);
    float2 PP = IN.UV - oy;
    float4 C00 = tex2D(SceneSampler,PP - ox);
    float4 C01 = tex2D(SceneSampler,PP);
    float4 C02 = tex2D(SceneSampler,PP + ox);
	   PP = IN.UV;
    float4 C10 = tex2D(SceneSampler,PP - ox);
    float4 C11 = tex2D(SceneSampler,PP);
    float4 C12 = tex2D(SceneSampler,PP + ox);
	   PP = IN.UV + oy;
    float4 C20 = tex2D(SceneSampler,PP - ox);
    float4 C21 = tex2D(SceneSampler,PP);
    float4 C22 = tex2D(SceneSampler,PP + ox);

    float n = NOISE2D(Freq*IN.UV).x;
    n = fmod(n, 0.111111)/0.111111;
    float4 result = spline(n,C00,C01,C02,C10,C11,C12,C20,C21,C22);
    // this also looks pretty cool....
    // float4 result = float4(n,n,n,1.0);
    // float4 result = lerp(C00,C22,n);
    return result;
}

///////////////////////////////////////
/// TECHNIQUES ////////////////////////
///////////////////////////////////////

//
// DirectX10 Version
//
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

technique10 Main10 < string Script =
#ifndef SHARED_BG_IMAGE
    "RenderColorTarget0=gSceneTexture;"
    "RenderDepthStencilTarget=DepthBuffer;"
    "ClearSetColor=gClearColor;"
    "ClearSetDepth=gClearDepth;"
	"Clear=Color;"
	"Clear=Depth;"
    "ScriptExternal=Color;" // calls all "previous" techniques & materials
    "Pass=PostP0;";
#else /* defined(SHARED_BG_IMAGE)  - no nead to create one, COLLADA has done it for us  */
    "ClearSetColor=gClearColor;"
    "ClearSetDepth=gClearDepth;"
	"Clear=Color;"
	"Clear=Depth;"
    "Pass=PostP0;";
#endif /* SHARED_BG_IMAGE */
> {
    pass PostP0 < string Script =
	"RenderColorTarget0=;"
	"RenderDepthStencilTarget=;"
	"Draw=Buffer;";
    > {
        SetVertexShader( CompileShader( vs_4_0, ScreenQuadVS2(QuadTexelOffsets) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, frostedPS(gSceneSampler,
				gPixelX,gPixelY,gFreq,gNoise2DSamp) ) );
	    SetRasterizerState(DisableCulling);
	    SetDepthStencilState(DepthDisabling, 0);
	    SetBlendState(DisableBlend, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF);
    }
}
#endif /* DIRECT3D_VERSION >= 0xa00 */

//
// DirectX9 Version
//
technique Main < string Script =
#ifndef SHARED_BG_IMAGE
    "RenderColorTarget0=gSceneTexture;"
    "RenderDepthStencilTarget=DepthBuffer;"
    "ClearSetColor=gClearColor;"
    "ClearSetDepth=gClearDepth;"
	"Clear=Color;"
	"Clear=Depth;"
    "ScriptExternal=Color;" // calls all "previous" techniques & materials
    "Pass=PostP0;";
#else /* defined(SHARED_BG_IMAGE)  - no nead to create one, COLLADA has done it for us  */
    "ClearSetColor=gClearColor;"
    "ClearSetDepth=gClearDepth;"
	"Clear=Color;"
	"Clear=Depth;"
    "Pass=PostP0;";
#endif /* SHARED_BG_IMAGE */
> {
    pass PostP0 < string Script =
	"RenderColorTarget0=;"
	"RenderDepthStencilTarget=;"
	"Draw=Buffer;";
    > {
	VertexShader = compile vs_3_0 ScreenQuadVS2(QuadTexelOffsets);
		ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		CullMode = None;
	PixelShader = compile ps_3_0 frostedPS(gSceneSampler,
				gPixelX,gPixelY,gFreq,gNoise2DSamp);
    }
}

//////////////////////////////////////////////
/////////////////////////////////////// eof //
//////////////////////////////////////////////
