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

% Place a gradient background into the scene.  The colors are
% animatable and the interpolation of the gradient occurs in HSV
% color space, rather than RB, to provide more-consistent luminance
% changes
keywords: image_processing



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

#define QUAD_FLOAT
#include <include\\Quad.fxh>
#include <include\\color_spaces.fxh>

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "scene";
    string ScriptOrder = "preprocess";
    string ScriptOutput = "color";
    string Script = "Technique=Main;";
> = 0.8;

QUAD_REAL3 gCLR <
	string UIName = "Lower Right";
	string UIWidget = "Color";
> = {1.0f, 1.0f, 0.6f};

QUAD_REAL3 gCLL <
	string UIName = "Lower Left";
	string UIWidget = "Color";
> = {1.0f, 0.0f, 0.0f};

QUAD_REAL3 gCUR <
	string UIName = "Upper Right";
	string UIWidget = "Color";
> = {0.0f, 0.0f, 1.0f};

QUAD_REAL3 gCUL <
	string UIName = "Upper Left";
	string UIWidget = "Color";
> = {0.0f, 0.4f, 0.0f};

static QUAD_REAL3 hCLL = rgb_to_hsv(gCLL);
static QUAD_REAL3 hCLR = rgb_to_hsv(gCLR);
static QUAD_REAL3 hCUL = rgb_to_hsv(gCUL);
static QUAD_REAL3 hCUR = rgb_to_hsv(gCUR);

float gClearDepth <string UIWidget = "none";> = 1.0;

///////////// shader ////////////

QUAD_REAL4 gradPS(QuadVertexOutput IN,
	uniform QUAD_REAL3 hCLR,  // in HSV
	uniform QUAD_REAL3 hCLL,  // in HSV
	uniform QUAD_REAL3 hCUR,  // in HSV
	uniform QUAD_REAL3 hCUL   // in HSV
) : COLOR
{
    QUAD_REAL3 botEdge = hsv_lerp(hCLL,hCLR,IN.UV.x);
    QUAD_REAL3 topEdge = hsv_lerp(hCUL,hCUR,IN.UV.x);
    QUAD_REAL3 hGrad = hsv_lerp(botEdge,topEdge,1.0-IN.UV.y);
    QUAD_REAL3 rgr = hsv_to_rgb(hsv_safe(hGrad));
    //QUAD_REAL3 c = color_cylinder(hCLR);
    //QUAD_REAL3 f  = from_cylinder(c);
    //rgr = hsv_to_rgb(f);
    return QUAD_REAL4(rgr,0);	// leave alpha alone
}  

///////////////////////////////////////
/// TECHNIQUES ////////////////////////
///////////////////////////////////////

technique Main <
    string Script =
	"ClearSetDepth=gClearDepth;"
	"Clear=Depth;" // no need for color clear, our technique will do it
	"Pass=PreP0;"
	"ScriptExternal=Scene;";
> {
    pass PreP0 < string Script = "Draw=Buffer;"; > {
	VertexShader = compile vs_3_0 ScreenQuadVS2(QuadTexelOffsets);
	    ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		CullMode = None;
	PixelShader  = compile ps_3_0 gradPS(hCLR,hCLL,hCUR,hCUL);
    }
}

/***************************** eof ***/
