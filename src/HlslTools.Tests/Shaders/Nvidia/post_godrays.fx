/*********************************************************************NVMH3****
*******************************************************************************
$Revision$

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

% Based on a radal blur effect, then with the origin image re-composited.

keywords: image_processing
date: 080410


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

#include <include\\Quad.fxh>

///////////////////////////////////////////////////////////
/////////////////////////////////////// Tweakables ////////
///////////////////////////////////////////////////////////

float gIntensity <
    string UIName = "Intensity";
    string UIWidget = "slider";
    float UIMin = 0.0f;
    float UIMax = 8.0f;
    float UIStep = 0.01f;
> = 6.0f;

float gGlowGamma <
    string UIName = "Centrality";
    string UIWidget = "slider";
    float UIMin = 0.5f;
    float UIMax = 2.0f;
    float UIStep = 0.01f;
> = 1.6f;

float gBlurStart <
    string UIName = "Blur Start";
    string UIWidget = "slider";
    float UIMin = 0.0f;
    float UIMax = 1.0f;
    float UIStep = 0.01f;
> = 1.0f;

float gBlurWidth <
    string UIName = "Blur Width";
    string UIWidget = "slider";
    float UIMin = -1.0f;
    float UIMax = 1.0f;
    float UIStep = 0.01f;
> = -0.3f;

float gCX <
    string UIName = "X Center";
    string UIWidget = "slider";
    float UIMin = -1.0f;
    float UIMax = 2.0f;
    float UIStep = 0.01f;
> = 0.5f;

float gCY <
    string UIName = "Y Center";
    string UIWidget = "slider";
    float UIMin = -1.0f;
    float UIMax = 2.0f;
    float UIStep = 0.01f;
> = 0.5f;

///////////////////////////////////////////////////////////
///////////////////////////// Render-to-Texture Data //////
///////////////////////////////////////////////////////////

DECLARE_QUAD_TEX(gSceneTexture,gSceneSampler,"A8R8G8B8")
DECLARE_QUAD_DEPTH_BUFFER(DepthBuffer,"D24S8")

////////////////////////////////////////////////////////////
////////////////////////////////// vertex shaders //////////
////////////////////////////////////////////////////////////

QuadVertexOutput VS_GodRays(float4 Position : POSITION, 
				float2 TexCoord : TEXCOORD0,
				uniform float2 TexelOffset,
				uniform float CX,
				uniform float CY)
{
    QuadVertexOutput OUT;
    OUT.Position = Position;
    float2 ctrPt = float2(CX,CY);
    OUT.UV = TexCoord + TexelOffset  - ctrPt;
    return OUT;
}

//////////////////////////////////////////////////////
////////////////////////////////// pixel shaders /////
//////////////////////////////////////////////////////

half4 PS_GodRays(QuadVertexOutput IN,
				uniform sampler2D tex,
				uniform float BlurStart,
				uniform float BlurWidth,
				uniform float CX,
				uniform float CY,
				uniform float Intensity,
				uniform float GlowGamma,
				uniform int nsamples
) : COLOR {
    half4 blurred = 0;
    float2 ctrPt = float2(CX,CY);
    // this loop will be unrolled by compiler and the constants precalculated:
    for(int i=0; i<nsamples; i++) {
    	float scale = BlurStart + BlurWidth*(i/(float) (nsamples-1));
    	blurred += tex2D(tex, IN.UV.xy*scale + ctrPt );
    }
    blurred /= nsamples;
    blurred.rgb = pow(blurred.rgb,GlowGamma);
    blurred.rgb *= Intensity;
    blurred.rgb = saturate(blurred.rgb);
    half4 origTex = tex2D(tex, IN.UV.xy + ctrPt );
    half3 newC = origTex.rgb + (1.0-origTex.a)* blurred.rgb;
    half newA = max(origTex.a,blurred.a);
    return half4(newC.rgb,newA);
} 

////////////////////////////////////////////////////////////
/////////////////////////////////////// techniques /////////
////////////////////////////////////////////////////////////

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
        SetVertexShader( CompileShader( vs_4_0, VS_GodRays(QuadTexelOffsets,gCX,gCY) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS_GodRays(gSceneSampler,
					gBlurStart, gBlurWidth,
					gCX,gCY,
					gIntensity, gGlowGamma,
					32) ) );
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
	VertexShader = compile vs_3_0 VS_GodRays(QuadTexelOffsets,gCX,gCY);
		ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		CullMode = None;
	PixelShader = compile ps_3_0 PS_GodRays(gSceneSampler,
					gBlurStart, gBlurWidth,
					gCX,gCY,
					gIntensity, gGlowGamma,
					32);
    }
}

////////////////////// eof ///
