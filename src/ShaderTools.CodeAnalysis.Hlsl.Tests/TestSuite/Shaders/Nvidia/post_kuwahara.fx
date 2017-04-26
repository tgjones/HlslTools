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

% Kuwahara edge enhancement is a staple of image processing.

keywords: image_processing color_conversion
date: 070419


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

///////////////////////////////////////////////////////////
/////////////////////////////////////// Tweakables ////////
///////////////////////////////////////////////////////////

half NPixels <
    string UIName = "Texel Steps";
    string UIWidget = "slider";
    half UIMin = 1.0f;
    half UIMax = 5.0f;
    half UIStep = 0.5f;
> = 1.5f;

half Threshhold <
    string UIWidget = "slider";
    half UIMin = 0.01f;
    half UIMax = 0.5f;
    half UIStep = 0.01f;
> = 0.2;

///////////////////////////////////////////////////////////
/////////////////////////////////////// Textures //////////
///////////////////////////////////////////////////////////

DECLARE_QUAD_TEX(gSceneTexture,gSceneSampler,"A8R8G8B8")
DECLARE_QUAD_DEPTH_BUFFER(DepthBuffer,"D24S8")

//////////////////////////////////////////////////////
/////////////////////////////////// pixel shader /////
//////////////////////////////////////////////////////

half getGray(half4 c)
{
    return(dot(c.rgb,((0.33333).xxx)));
}

#define GrabPix(n,a) half3 n = tex2D(SceneSampler,(a)).xyz;

half4 kuwaharaPS(QuadVertexOutput IN,
		    uniform sampler2D SceneSampler) : COLOR {
	half2 ox = half2(NPixels/QuadScreenSize.x,0.0);
	half2 oy = half2(0.0,NPixels/QuadScreenSize.y);
	float2 uv = IN.UV.xy;
	half2 ox2 = 2 * ox;
	half2 oy2 = 2 * oy;
	half2 PP = uv - oy2;
	GrabPix(c00,PP-ox2)
	GrabPix(c01,PP-ox)
	GrabPix(c02,PP)
	GrabPix(c03,PP+ox)
	GrabPix(c04,PP+ox2)
	PP = uv - oy;
	GrabPix(c10,PP-ox2)
	GrabPix(c11,PP-ox)
	GrabPix(c12,PP)
	GrabPix(c13,PP+ox)
	GrabPix(c14,PP+ox2)
	PP = uv;
	GrabPix(c20,PP-ox2)
	GrabPix(c21,PP-ox)
	GrabPix(c22,PP)
	GrabPix(c23,PP+ox)
	GrabPix(c24,PP+ox2)
	half3 m00 = (c00+c01+c02 + c10+c11+c12 + c20+c21+c22)/9;
	half3 d = (c00 - m00); half v00 = dot(d,d);
	d = (c01 - m00); v00 += dot(d,d);
	d = (c02 - m00); v00 += dot(d,d);
	d = (c10 - m00); v00 += dot(d,d);
	d = (c11 - m00); v00 += dot(d,d);
	d = (c12 - m00); v00 += dot(d,d);
	d = (c20 - m00); v00 += dot(d,d);
	d = (c21 - m00); v00 += dot(d,d);
	d = (c12 - m00); v00 += dot(d,d);
	half3 m01 = (c02+c03+c04 + c12+c13+c14 + c22+c23+c24)/9;
	d = (c02 - m01); half v01 = dot(d,d);
	d = (c03 - m01); v01 += dot(d,d);
	d = (c04 - m01); v01 += dot(d,d);
	d = (c12 - m01); v01 += dot(d,d);
	d = (c13 - m01); v01 += dot(d,d);
	d = (c14 - m01); v01 += dot(d,d);
	d = (c22 - m01); v01 += dot(d,d);
	d = (c23 - m01); v01 += dot(d,d);
	d = (c14 - m01); v01 += dot(d,d);
	PP = uv + oy;
	GrabPix(c30,PP-ox2)
	GrabPix(c31,PP-ox)
	GrabPix(c32,PP)
	GrabPix(c33,PP+ox)
	GrabPix(c34,PP+ox2)
	PP = uv + oy;
	GrabPix(c40,PP-ox2)
	GrabPix(c41,PP-ox)
	GrabPix(c42,PP)
	GrabPix(c43,PP+ox)
	GrabPix(c44,PP+ox2)
	half3 m10 = (c20+c21+c22 + c30+c31+c32 + c40+c41+c42)/9;
	d = (c20 - m10); half v10 = dot(d,d);
	d = (c21 - m10); v10 += dot(d,d);
	d = (c22 - m10); v10 += dot(d,d);
	d = (c30 - m10); v10 += dot(d,d);
	d = (c31 - m10); v10 += dot(d,d);
	d = (c32 - m10); v10 += dot(d,d);
	d = (c40 - m10); v10 += dot(d,d);
	d = (c41 - m10); v10 += dot(d,d);
	d = (c42 - m10); v10 += dot(d,d);
	half3 m11 = (c22+c23+c24 + c32+c33+c34 + c42+c43+c44)/9;
	d = (c22 - m11); half v11 = dot(d,d);
	d = (c23 - m11); v11 += dot(d,d);
	d = (c24 - m11); v11 += dot(d,d);
	d = (c32 - m11); v11 += dot(d,d);
	d = (c33 - m11); v11 += dot(d,d);
	d = (c34 - m11); v11 += dot(d,d);
	d = (c42 - m11); v11 += dot(d,d);
	d = (c43 - m11); v11 += dot(d,d);
	d = (c44 - m11); v11 += dot(d,d);
	half3 result = m00;
	half rv = v00;
	if (v01 < rv) { result = m01; rv = v01; }
	if (v10 < rv) { result = m10; rv = v10; }
	if (v11 < rv) { result = m11; }
	return half4(result,1);
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
        SetPixelShader( CompileShader( ps_4_0, kuwaharaPS(gSceneSampler) ) );
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
	PixelShader = compile ps_3_0 kuwaharaPS(gSceneSampler);
    }
}

//////////////////////////////////////////////
/////////////////////////////////////// eof //
//////////////////////////////////////////////
