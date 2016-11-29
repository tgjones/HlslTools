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

% Same as "scene_edgeDetect," but with separated R, G, and B

keywords: image_processing
date: 070206


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

float gNPixels <
    string UIName = "Texel Steps";
    string UIWidget = "slider";
    float UIMin = 1.0f;
    float UIMax = 5.0f;
    float UIStep = 0.5f;
> = 1.5f;

float gThreshhold <
    string UIWidget = "slider";
    float UIMin = 0.01f;
    float UIMax = 0.5f;
    float UIStep = 0.01f;
> = 0.2;

///////////////////////////////////////////////////////////
/////////////////////////////////////// Textures //////////
///////////////////////////////////////////////////////////

DECLARE_QUAD_TEX(gSceneTexture,gSceneSampler,"A8R8G8B8")
DECLARE_QUAD_DEPTH_BUFFER(DepthBuffer,"D24S8")

//////////////////////////////////////////////////////
/////////////////////////////////// struct ///////////
//////////////////////////////////////////////////////

struct EdgeVertexOutput
{
    QUAD_REAL4 Position	: POSITION;
    QUAD_REAL2 UV00	: TEXCOORD0;
    QUAD_REAL2 UV01	: TEXCOORD1;
    QUAD_REAL2 UV02	: TEXCOORD2;
    QUAD_REAL2 UV10	: TEXCOORD3;
    QUAD_REAL2 UV12	: TEXCOORD4;
    QUAD_REAL2 UV20	: TEXCOORD5;
    QUAD_REAL2 UV21	: TEXCOORD6;
    QUAD_REAL2 UV22	: TEXCOORD7;
};

//////////////////////////////////////////////////////
/////////////////////////////////// vertex shader ////
//////////////////////////////////////////////////////

EdgeVertexOutput edgeVS(
		QUAD_REAL3 Position : POSITION, 
		QUAD_REAL3 TexCoord : TEXCOORD0,
		uniform float NPixels
) {
    EdgeVertexOutput OUT;
    OUT.Position = QUAD_REAL4(Position.xyz, 1);
    QUAD_REAL2 ctr = TexCoord.xy + QuadTexelOffsets;
    QUAD_REAL2 ox = QUAD_REAL2(NPixels/QuadScreenSize.x,0.0);
    QUAD_REAL2 oy = QUAD_REAL2(0.0,NPixels/QuadScreenSize.y);
    OUT.UV00 = ctr - ox - oy;
    OUT.UV01 = ctr - oy;
    OUT.UV02 = ctr + ox - oy;
    OUT.UV10 = ctr - ox;
    OUT.UV12 = ctr + ox;
    OUT.UV20 = ctr - ox + oy;
    OUT.UV21 = ctr + oy;
    OUT.UV22 = ctr + ox + oy;
    return OUT;
}

//////////////////////////////////////////////////////
/////////////////////////////////// pixel shader /////
//////////////////////////////////////////////////////


QUAD_REAL4 edgeDetectPS(EdgeVertexOutput IN,
	uniform sampler2D SceneSampler,
	uniform float NPixels,
	uniform float Threshhold
) : COLOR {
    float T2 = (Threshhold*Threshhold);
    QUAD_REAL4 cc00 = tex2D(SceneSampler,IN.UV00);
    QUAD_REAL4 cc01 = tex2D(SceneSampler,IN.UV01);
    QUAD_REAL4 cc02 = tex2D(SceneSampler,IN.UV02);
    QUAD_REAL4 cc10 = tex2D(SceneSampler,IN.UV10);
    QUAD_REAL4 cc12 = tex2D(SceneSampler,IN.UV12);
    QUAD_REAL4 cc20 = tex2D(SceneSampler,IN.UV20);
    QUAD_REAL4 cc21 = tex2D(SceneSampler,IN.UV21);
    QUAD_REAL4 cc22 = tex2D(SceneSampler,IN.UV22);
    QUAD_REAL4 sx = 0;
    sx -= cc00;
    sx -= cc01 * 2;
    sx -= cc02;
    sx += cc20;
    sx += cc21 * 2;
    sx += cc22;
    QUAD_REAL4 sy = 0;
    sy -= cc00;
    sy += cc02;
    sy -= cc10 * 2;
    sy += cc12 * 2;
    sy -= cc20;
    sy += cc22;
    QUAD_REAL4 dist = (sx*sx+sy*sy);	// per-channel
    QUAD_REAL4 result = 1;
    result = float4((dist.x<=T2),(dist.y<=T2),(dist.z<=T2),(dist.w<=T2));
    return 1-result;
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
        SetPixelShader( CompileShader( ps_4_0, edgeDetectPS(gSceneSampler,
						gNPixels,gThreshhold) ) );
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
	PixelShader = compile ps_3_0 edgeDetectPS(gSceneSampler,
						gNPixels,gThreshhold);
    }
}

//////////////////////////////////////////////
/////////////////////////////////////// eof //
//////////////////////////////////////////////
