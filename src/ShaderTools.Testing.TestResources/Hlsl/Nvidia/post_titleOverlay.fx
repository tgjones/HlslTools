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

% Animatable overlay titling - uses the ROP rather than render-to-texture
keywords: image_processing animation
date: 061220



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
    string Script = "Technique=Technique?title:withOpacity;";
> = 0.8;

// color and depth used for full-screen clears

float4 gClearColor <
    string UIWidget = "Color";
    string UIName = "Background";
> = {0,0,0,0};

float gClearDepth <string UIWidget = "none";> = 1.0;

/////////////////////////////////////////////////////
//// Textures for Input Images //////////////////////
/////////////////////////////////////////////////////

FILE_TEXTURE_2D_MODAL(gTitleCard,gBlendImgSampler,"Blended.dds",Border)

/////////////////////////////////////////////////////
//// Tweakables /////////////////////////////////////
/////////////////////////////////////////////////////

float gOpacity <
    string UIWidget = "slider";
    float uimin = 0.0;
    float uimax = 1.0;
    float uistep = 0.01;
    string UIName = "Title Opacity";
> = 1.0;

float gXScale <
    string UIName = "Width";
    string UIWidget = "slider";
    float UIMin = 0.1;
    float UIMax = 10.0;
    float UIStep = 0.01;
> = 1.0f;

float gYScale <
    string UIName = "Height";
    string UIWidget = "slider";
    float UIMin = 0.1;
    float UIMax = 10.0;
    float UIStep = 0.01;
> = 1.0f;

float gXPos <
    string UIName = "X Position";
    string UIWidget = "slider";
    float UIMin = -2.0;
    float UIMax = 4.0;
    float UIStep = 0.001;
> = 0.0f;

float gYPos <
    string UIName = "Y Position";
    string UIWidget = "slider";
    float UIMin = -2.0;
    float UIMax = 4.0;
    float UIStep = 0.001;
> = 0.0f;

#define CTR QUAD_REAL2(0.5,0.5)

/**************************************************************/
/********* utility pixel-shader functions and macros **********/
/**************************************************************/


QuadVertexOutput TitleQuadVS(
		QUAD_REAL3 Position : POSITION, 
		QUAD_REAL3 TexCoord : TEXCOORD0,
		uniform float XScale,
		uniform float YScale,
		uniform float XPos,
		uniform float YPos
) {
    QuadVertexOutput OUT;
    OUT.Position = QUAD_REAL4(Position, 1);
    float2 Scale = 1.0/float2(XScale,YScale);
    float2 Pos = float2(-XPos,YPos) + CTR;
    QUAD_REAL2 nuv = Pos + (TexCoord.xy-CTR) * Scale;
#ifdef NO_TEXEL_OFFSET
    OUT.UV = nuv;
#else /* NO_TEXEL_OFFSET */
    OUT.UV = QUAD_REAL2(nuv+QuadTexelOffsets); 
#endif /* NO_TEXEL_OFFSET */
    return OUT;
}

QUAD_REAL4 TexQuadOPS(QuadVertexOutput IN,
			uniform sampler2D InputSampler,
			uniform float Opacity) : COLOR
{   
    QUAD_REAL4 texCol = tex2D(InputSampler, IN.UV);
    texCol.a *= Opacity;
    return texCol;
}  

///////////////////////////////////////
/// TECHNIQUES ////////////////////////
///////////////////////////////////////


technique title <
    string Script =
	"ClearSetColor=gClearColor;"
	"ClearSetDepth=gClearDepth;"
	"Clear=Color;"
	"Clear=Depth;"
	"ScriptExternal=color;"
	"Pass=p0;";
> {
    pass p0 < string Script="Draw=Buffer;"; > {
	VertexShader = compile vs_3_0 TitleQuadVS(gXScale, gYScale,
							gXPos, gYPos);
	    ZEnable = true;
		ZWriteEnable = true;
		AlphaBlendEnable = true;
		SrcBlend = SrcAlpha;
		DestBlend = InvSrcAlpha;
		CullMode = None;
	PixelShader = compile ps_3_0 TexQuadPS(gBlendImgSampler);
    }
}

technique withOpacity <
    string Script =
	"ClearSetColor=gClearColor;"
	"ClearSetDepth=gClearDepth;"
	"Clear=Color;"
	"Clear=Depth;"
	"ScriptExternal=color;"
	"Pass=p0;";
> {
    pass p0 < string Script="Draw=Buffer;"; > {
	VertexShader = compile vs_3_0 TitleQuadVS(gXScale, gYScale,
							gXPos, gYPos);
	    ZEnable = true;
		ZWriteEnable = true;
		AlphaBlendEnable = true;
		SrcBlend = SrcAlpha;
		DestBlend = InvSrcAlpha;
		CullMode = None;
	PixelShader = compile ps_3_0 TexQuadOPS(gBlendImgSampler,
						    gOpacity);
    }
}


/***************************** eof ***/
