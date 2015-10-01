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

% An .FX Paint Program. Scene geometry is ignored.
% Draw with the left mouse button.
% To clear screen, just make the brush big and paint everything.
% Resizing the window will mess up your drawing.
% Brush strokes will change in size and opacity over time, set "FadeTime"
% 	to a high value for more even (though less expressive) strokes.
keywords: image_processing paint
date: 070622



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

#include <include\\Quad.fxh>

float Script : STANDARDSGLOBAL <
	string UIWidget = "none";
	string ScriptClass = "scene";
	string ScriptOrder = "standard";
	string ScriptOutput = "color";
	// We just call a script in the main technique.
	string Script = "Technique=Main;";
> = 0.8; // version #

bool bReset : FXCOMPOSER_RESETPULSE <
	string UIName="Clear Canvas";
>;

bool bRevert <
	string UIName="Use BgPic instead of BgColor?";
> = true;

// color and depth used for full-screen clears

float4 gClearColor <
    string UIWidget = "Color";
    string UIName = "Background";
> = {1.0, 0.95, 0.92, 1.0};

float gClearDepth <string UIWidget = "none";> = 1.0;

///////////// untweakables //////////////////////////

float4 gMouseL : LEFTMOUSEDOWN < string UIWidget="None"; >;
float3 gMousePos : MOUSEPOSITION < string UIWidget="None"; >;
float gTimer : TIME < string UIWidget = "None"; >;

////////////// tweakables /////////////////////////

float3 gPaintColor <
    string UIWidget = "Color";
    string UIName = "Brush";
> = {0.4f, 0.3f, 1.0f};

float gOpacity <
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 1.0;
    float UIStep = 0.01;
> = 0.45f;

float gBrushSizeStart <
    string UIWidget = "slider";
    float UIMin = 0.001;
    float UIMax = 0.15;
    float UIStep = 0.001;
    string UIName = "Brush Size Start";
> = 0.07f;

float gBrushSizeEnd <
    string UIWidget = "slider";
    float UIMin = 0.001;
    float UIMax = 0.15;
    float UIStep = 0.001;
    string UIName = "Brush Size End";
> = 0.01f;

float gFadeTime <
    string UIWidget = "slider";
    float UIMin = 0.1;
    float UIMax = 10.0;
    float UIStep = 0.1;
    string UIName = "Fade Time";
> = 2.00f;

// VM functions (executed by CPU) ////////////

float fadeout(uniform float Timer,
		uniform float4 MouseL,
		uniform float FadeTime
) {
	float f = 1 - min(1,max(0,(Timer - MouseL.w))/FadeTime);
	return f;
}

float lerpsize(uniform float Timer,
		uniform float4 MouseL,
		uniform float BrushSizeStart,
		uniform float BrushSizeEnd,
		uniform float FadeTime
) {
	float f = fadeout(Timer,MouseL,FadeTime);
	float ds = lerp(BrushSizeEnd,BrushSizeStart,f);
	return ds;
}

FILE_TEXTURE_2D_MODAL(BgPic,BgSampler,"Veggie.dds",CLAMP)

/***************************************************/
/*** This shader performs the clear/revert *********/
/***************************************************/

float4 revertPS(QuadVertexOutput IN,
	uniform float UseTex,
	uniform float4 ClearColor
) : COLOR
{
//	float4 result = ClearColor;
//	float4 texc = tex2D(BgSampler,IN.UV);
//	if (bRevert) {
//		result = texc;
//	}
	// return result;
    return lerp(ClearColor,tex2D(BgSampler,IN.UV),UseTex);
}

/***************************************************/
/*** The dead-simple paintbrush shader *************/
/***************************************************/

float4 strokePS(QuadVertexOutput IN,
	uniform float Fadeout,
	uniform float BrushSize,
	uniform float4 MouseL,
	uniform float3 MousePos,
	uniform float3 PaintColor,
	uniform float Opacity
) : COLOR {
    float2 delta = IN.UV.xy-MousePos.xy;
    float dd = MouseL.z*(1-min(length(delta)/BrushSize,1));
    dd *= Opacity * Fadeout;
    return float4(PaintColor.xyz,dd);
}

////////////////// Technique ////////

technique Main <
    string Script =
	"RenderColorTarget0=;"
	"RenderDepthStencilTarget=;"
	"LoopByCount=bReset;"
	    "Pass=revert;"
	"LoopEnd=;"
	"Pass=splat;";
> {
    pass revert < string Script = "Draw=Buffer;"; > {
	VertexShader = compile vs_3_0 ScreenQuadVS2(QuadTexelOffsets);
	    ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		CullMode = None;
	PixelShader  = compile ps_3_0 revertPS(true,gClearColor);
    }
    pass splat < string Script = "Draw=Buffer;"; > {
	VertexShader = compile vs_3_0 ScreenQuadVS2(QuadTexelOffsets);
	    ZEnable = true;
		ZWriteEnable = true;
		AlphaBlendEnable = true;
		SrcBlend = SrcAlpha;
		DestBlend = InvSrcAlpha;
		CullMode = None;
	PixelShader  = compile ps_3_0 strokePS(fadeout(gTimer,gMouseL,gFadeTime),
				lerpsize(gTimer,gMouseL,
					    gBrushSizeStart,
					    gBrushSizeEnd,
					    gFadeTime),
				gMouseL,gMousePos,
				gPaintColor,gOpacity);
    }
}

/////////////////////////////////////////////
///////////////////////////////////// eof ///
/////////////////////////////////////////////
