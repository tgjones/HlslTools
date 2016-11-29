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

% Mandelbrot set browser using PS_3 branching
	Scene geometry is ignored.
	For more speed, reduce "iterations"
	For more detail, increase "iterations"

keywords: image_processing pattern



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
	string Script = "Technique=Main;";
> = 0.8; // version #

///////////////////////////////////////////////////////////
/////////////////////////////////////// Tweakables ////////
///////////////////////////////////////////////////////////

int gIterate <
    float UIMin = 2;
    float UIMax = 200;
    string UIName = "Iteration Count";
> = 50;

float gScale <
    string UIWidget = "slider";
    float UIMin = 0.001;
    float UIMax = 3.0;
    float UIStep = 0.001;
> = 0.11f;

// float2 gCenter = {0.5,0.1};
float gCenterX <
    string UIWidget = "slider";
    string UIName = "X Center";
    float UIMin = 0.0;
    float UIMax = 1.0;
    float UIStep = 0.001;
> = 0.709f;

float gCenterY <
    string UIWidget = "slider";
    string UIName = "Y Center";
    float UIMin = 0.0;
    float UIMax = 1.0;
    float UIStep = 0.001;
> = 0.350f;

float gRange <
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 0.05;
    float UIStep = 0.001;
    string UIName = "Outer Color Gradation";
> = 0.05f;

float3 gInColor <
    string UIWidget = "Color";
    string UIName = "Inside Region";
> = {0,0,0};

float3 gOutColorA <
    string UIWidget = "Color";
    string UIName = "Outer Region";
> = {1,0,.3};

float3 gOutColorB <
    string UIWidget = "Color";
    string UIName = "Edge Region";
> = {.2,1,0};

/////////////////////////////////////////////////////
////////////////////////////////// pixel shader /////
/////////////////////////////////////////////////////

float4 mandyPS(QuadVertexOutput IN,
	    uniform int Iterate,
	    uniform float Scale,
	    uniform float CenterX,
	    uniform float CenterY,
	    uniform float Range,
	    uniform float3 InColor,
	    uniform float3 OutColorA,
	    uniform float3 OutColorB
) : COLOR {
    //float3 c = mandelbrot_color(IN.TexCoord0);
    float2 pos = frac(IN.UV.xy);
    float real = ((pos.x - 0.5)*Scale)-CenterX;
    float imag = ((0.5 - pos.y)*Scale)-CenterY;
    float Creal = real;
    float Cimag = imag;
    float r2 = 0;
    float i;
    for (i=0; (i<Iterate) && (r2<4.0); i++) {
		float tempreal = real;
		real = (tempreal*tempreal) - (imag*imag) + Creal;
		imag = 2*tempreal*imag + Cimag;
		r2 = (real*real) + (imag*imag);
    }
    float3 finalColor;
    if (r2 < 4) {
       finalColor = InColor;
    } else {
    	finalColor = lerp(OutColorA,OutColorB,frac(i * Range));
    }
    return float4(finalColor,1);
}

///////////////////////////////////////
/// TECHNIQUES ////////////////////////
///////////////////////////////////////


technique Main <
	string Script = "Pass=p0;";
> {
    pass p0 <
    	string Script = "RenderColorTarget0=;"
			"Draw=Buffer;";
    > {
	VertexShader = compile vs_3_0 ScreenQuadVS2(QuadTexelOffsets);
	    ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		CullMode = None;
	PixelShader = compile ps_3_0 mandyPS(gIterate,gScale,
						    gCenterX,gCenterY,
						    gRange,
						    gInColor,
						    gOutColorA,gOutColorB);
    }
}

//////////////////////////////////////////////////////////
////////////////////////////////////////////////// eof ///
//////////////////////////////////////////////////////////
