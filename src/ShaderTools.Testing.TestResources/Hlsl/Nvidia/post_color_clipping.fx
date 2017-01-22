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

% Clip low and high colors so that the resulting image is within a
% narrower range, e.g. for TV signals.  Two Techniques are provided
% -- one clips the colors that go outside the specified range
% between Min and Max, while the other stretches or compresses the
% total color space to conform to the indicated range.

keywords: image_processing
date: 070420



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
    string ScriptOrder = "postprocess";
    string ScriptOutput = "color";
    string Script = "Technique=Technique?Clip:Stretched;";
> = 0.8; // version #

// color and depth used for full-screen clears

float4 gClearColor <
    string UIWidget = "Color";
    string UIName = "Background";
> = {0,0,0,0};

float gClearDepth <string UIWidget = "none";> = 1.0;

///////////////////////////////////////////////////////////
/////////////////////////////////////// Textures //////////
///////////////////////////////////////////////////////////

float4 MinColor <
    string UIWidget = "color";
    string UIName = "Min";
> = {0.0,0.0,0.0,0.0};

float4 MaxColor <
    string UIWidget = "color";
    string UIName = "Max";
> = {1.0,1.0,1.0,1.0};

DECLARE_QUAD_TEX(gSceneTexture,gSceneSampler,"A8R8G8B8")
DECLARE_QUAD_DEPTH_BUFFER(DepthBuffer,"D24S8")

//////////////////////////////////////////////////////
/////////////////////////////////// pixel shader /////
//////////////////////////////////////////////////////

QUAD_REAL4 Clipper(QuadVertexOutput IN,
		    uniform sampler2D SceneSampler) : COLOR
{   
    QUAD_REAL4 orig = tex2D(SceneSampler, IN.UV);
    orig = min(MaxColor,orig);
    orig = max(MinColor,orig);
    return orig;
}  

static QUAD_REAL4 ColorRange = (MaxColor-MinColor);

QUAD_REAL4 Squisher(QuadVertexOutput IN,
		    uniform sampler2D SceneSampler) : COLOR
{   
    QUAD_REAL4 orig = tex2D(SceneSampler, IN.UV);
    orig = min(MaxColor,orig);
    orig = max(MinColor,orig);
    orig = (orig-MinColor)/ColorRange;
    return orig;
}  

////////////////////////////////////////////////////////////
/////////////////////////////////////// techniques /////////
////////////////////////////////////////////////////////////

technique Clip <
    string Script =
	    "RenderColorTarget0=gSceneTexture;"
	    "RenderDepthStencilTarget=DepthBuffer;"
		"ClearSetColor=gClearColor;"
		"ClearSetDepth=gClearDepth;"
		    "Clear=Color;"
		    "Clear=Depth;"
		"ScriptExternal=color;"
	    "Pass=Convert;";
> {
    pass Convert <
    	string Script = "RenderColorTarget0=;"
			"RenderDepthStencilTarget=;"
			"Draw=Buffer;";
    > {
	VertexShader = compile vs_3_0 ScreenQuadVS2(QuadTexelOffsets);
		ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		CullMode = None;
	PixelShader  = compile ps_3_0 Clipper(gSceneSampler);
    }
}

technique Stretched <
    string Script =
	    "RenderColorTarget0=gSceneTexture;"
	    "RenderDepthStencilTarget=DepthBuffer;"
		"ClearSetColor=gClearColor;"
		"ClearSetDepth=gClearDepth;"
		    "Clear=Color;"
		    "Clear=Depth;"
		"ScriptExternal=color;"
	    "Pass=Convert;";
> {
    pass Convert <
    	string Script = "RenderColorTarget0=;"
			"RenderDepthStencilTarget=;"
			"Draw=Buffer;";
    > {
	VertexShader = compile vs_3_0 ScreenQuadVS2(QuadTexelOffsets);
		ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		CullMode = None;
	PixelShader  = compile ps_3_0 Squisher(gSceneSampler);
    }
}
