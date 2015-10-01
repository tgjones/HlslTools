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

% A simple way to view 1D Textures (e.g., color ramps) as a 2D graph
% This is an imaging shader, all geometry is ignored



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
float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "scene";
    string ScriptOrder = "standard";
    string ScriptOutput = "color";
    string Script = "Technique=Main;";
> = 0.8;

#include <include\\Quad.fxh>

///////////////////////////////////////////////////////////
/////////////////////////////////////// Tweakables ////////
///////////////////////////////////////////////////////////

QUAD_REAL gTopOfScale <
    string UIWidget = "slider";
    QUAD_REAL UIMin = 0.2f;
    QUAD_REAL UIMax = 4.0f;
    QUAD_REAL UIStep = 0.01f;
    string UIName = "Maximum Y";
> = 1.0f;

QUAD_REAL gLeft <
    string UIWidget = "slider";
    QUAD_REAL UIMin = 0.0f;
    QUAD_REAL UIMax = 0.99f;
    QUAD_REAL UIStep = 0.001f;
    string UIName = "Minimum X";
> = 0.0f;

QUAD_REAL gRight <
    string UIWidget = "slider";
    QUAD_REAL UIMin = 0.01f;
    QUAD_REAL UIMax = 1.0f;
    QUAD_REAL UIStep = 0.001f;
    string UIName = "Maximum X";
> = 1.0f;


QUAD_REAL gAlphaEffect <
    string UIWidget = "slider";
    QUAD_REAL UIMin = 0.0f;
    QUAD_REAL UIMax = 1.0f;
    QUAD_REAL UIStep = 0.1f;
    string UIName = "Alpha Effect (if any)";
> = 0.2f;

///////////////////////////////////////////////////////////
///////////////////////////// Curve Texture ///////////////
///////////////////////////////////////////////////////////

#ifdef PROCEDURAL_TEXTURE
//
// This code just builds a sample curve for testing
//

// assume "t" ranges from 0 to 1 safely
// brute-force this, since it's running on the CPU
QUAD_REAL3 c_bezier(QUAD_REAL3 c0, QUAD_REAL3 c1, QUAD_REAL3 c2, QUAD_REAL3 c3, QUAD_REAL t)
{
	QUAD_REAL t2 = t*t;
	QUAD_REAL t3 = t2*t;
	QUAD_REAL nt = 1.0 - t;
	QUAD_REAL nt2 = nt*nt;
	QUAD_REAL nt3 = nt2 * nt;
	QUAD_REAL3 b = nt3*c0 + (3.0*t*nt2)*c1 + (3.0*t2*nt)*c2 + t3*c3;
	return b;
}

// function used to fill the volume noise texture
QUAD_REAL4 color_curve(QUAD_REAL2 Pos : POSITION) : COLOR
{
    QUAD_REAL3 kolor0 = QUAD_REAL3(0.0,0.0,0.0);
    QUAD_REAL3 kolor1 = QUAD_REAL3(0.9,0.7,0.0);
    QUAD_REAL3 kolor2 = QUAD_REAL3(0.3,0.5,0.95);
    QUAD_REAL3 kolor3 = QUAD_REAL3(1.0,0.9,1.0);
	QUAD_REAL3 sp = c_bezier(kolor0,kolor1,kolor2,kolor3,Pos.x);
    return QUAD_REAL4(sp,Pos.x);
}

texture gProcColorCurveTex  <
    string ResourceType = "2D";
    string function = "color_curve";
    string UIWidget = "None";
    float2 Dimensions = { 256.0f, 4.0f}; // could be height=1, but I want it to be visible in the Texture View...
>;
#else /* ! PROCEDURAL_TEXTURE */
texture gColorCurveTex  <
    string ResourceName="grad1.png";
    string ResourceType = "2D";
    string UIName = "1D Ramp Texture";
>;
#endif /* ! PROCEDURAL_TEXTURE */

sampler2D gColorCurveSampler = sampler_state 
{
#ifdef PROCEDUAL_TEXTURE
	Texture = <gProcColorCurveTex>;
#else /* ! PROCEDURAL_TEXTURE */
	Texture = <gColorCurveTex>;
#endif /* ! PROCEDURAL_TEXTURE */
    AddressU  = Clamp;        
    AddressV  = Clamp;
#if DIRECT3D_VERSION >= 0xa00
    Filter = MIN_MAG_MIP_LINEAR;
#else /* DIRECT3D_VERSION < 0xa00 */
    MinFilter = Linear;
    MipFilter = Linear;
    MagFilter = Linear;
#endif /* DIRECT3D_VERSION */
};

///////////////////////////////////////////////////////////
///////////////////////////// Render-to-Texture Data //////
///////////////////////////////////////////////////////////

QUAD_REAL4 graphPS(QuadVertexOutput IN,
		uniform sampler2D ColorCurveSampler,
		uniform QUAD_REAL TopOfScale,
		uniform QUAD_REAL Left,
		uniform QUAD_REAL Right,
		uniform QUAD_REAL AlphaEffect
) : COLOR {   
    QUAD_REAL nx = Left + (IN.UV.x*(Right-Left));
    QUAD_REAL2 nuv = QUAD_REAL2(nx,0.5);
    QUAD_REAL4 tx = tex2D(ColorCurveSampler,nuv);
    QUAD_REAL4 f = 0;
    f = (tx/TopOfScale > (1-IN.UV.y).xxxx);
    f.xyz *= lerp(f.xyz,f.www,AlphaEffect);
    return QUAD_REAL4(f);
}  

///////////////////////////////////////
/// TECHNIQUES ////////////////////////
///////////////////////////////////////


technique Main < string Script = "Pass=p0;"; > {
    pass p0 < string Script = "Draw=Buffer;"; > {
	VertexShader = compile vs_3_0 ScreenQuadVS2(QuadTexelOffsets);
		ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		CullMode = None;
	PixelShader  = compile ps_3_0 graphPS(
			    gColorCurveSampler,
			    gTopOfScale,
			    gLeft,gRight,
			    gAlphaEffect);
    }
}

///////////////////////////////////////////////////////////////
////////////////////////////////////////////////////// eof ////
///////////////////////////////////////////////////////////////
