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

% Put a color ramp behind the current scene

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

#include <include\\Quad.fxh>

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
> = {1.0f, 1.0f, 1.0f};

QUAD_REAL3 gCUR <
	string UIName = "Upper Right";
	string UIWidget = "Color";
> = {0.0f, 0.0f, 0.35f};

QUAD_REAL3 gCUL <
	string UIName = "Upper Left";
	string UIWidget = "Color";
> = {0.0f, 0.0f, 0.0f};

float gClearDepth <string UIWidget = "none";> = 1.0;

/////// shader ///////////

QUAD_REAL4 gradPS(QuadVertexOutput IN,
	    uniform float3 CLR,
	    uniform float3 CLL,
	    uniform float3 CUR,
	    uniform float3 CUL) : COLOR
{   
    QUAD_REAL3 lo = lerp(CLL,CLR,IN.UV.x);
    QUAD_REAL3 hi = lerp(CUL,CUR,IN.UV.x);
    QUAD_REAL3 gr = lerp(lo,hi,1.0-IN.UV.y);
    return QUAD_REAL4(gr,0);	// leave alpha alone
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
	PixelShader  = compile ps_3_0 gradPS(gCLR,gCLL,gCUR,gCUL);
    }
}

/***************************** eof ***/
