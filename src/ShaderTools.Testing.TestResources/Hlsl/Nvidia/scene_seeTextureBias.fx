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

% Simple texture biasing demo/experiment, shows whats possible
% with biased blurring.

keywords: image_processing
'


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

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "scene";
    string ScriptOrder = "standard";
    string ScriptOutput = "color";
    string Script = "Technique=Main;";
> = 0.8; // version #

#include <include\\Quad.fxh>

float gBias <
    string UIWidget = "slider";
    string UIName = "Texture Bias";
    float UIMin = 0.0;
    float UIMax = 8.0;
    float UIStep = 0.1;
> = 5.0f;

float gGradient <
    string UIWidget = "slider";
    string UIName = "Gradient Effect";
    float UIMin = 0.0;
    float UIMax = 1.0;
    float UIStep = 0.01;
> = 0.5f;

////////////////////////////////////////////////

texture gColorTex <
    string ResourceName = "default_color.dds";
    string UIName = "Background Texture";
    string ResourceType = "2D";
>;

sampler2D gColorSampler = sampler_state {
    Texture = <gColorTex>;
    AddressU = Clamp;
    AddressV = Clamp;
#if DIRECT3D_VERSION >= 0xa00
    Filter = MIN_MAG_MIP_LINEAR;
#else /* DIRECT3D_VERSION < 0xa00 */
    MinFilter = Linear;
    MipFilter = Linear;
    MagFilter = Linear;
#endif /* DIRECT3D_VERSION */
};

/****************************************/
/*** The dead-simple shaders: ***********/
/****************************************/

struct VS_OUTPUT {
    float4 Position : POSITION;
    float4 TexCoord0	: TEXCOORD0;
};

VS_OUTPUT VS_Quad(float3 Position : POSITION, 
		float3 TexCoord : TEXCOORD0,
		uniform float Bias,
		uniform float Gradient
) {
    VS_OUTPUT OUT = (VS_OUTPUT)0;
    OUT.Position = float4(Position, 1);
    float g = Bias * lerp(1.0,TexCoord.x,Gradient);
    OUT.TexCoord0 = float4(TexCoord.xy+QuadTexelOffsets,TexCoord.z,g); 
    return OUT;
}

float4 drawMapPS(VS_OUTPUT IN,
		uniform sampler2D ColorSampler
) : COLOR {
    return tex2Dbias(ColorSampler,IN.TexCoord0);
}

///////////////////////////////////////
/// TECHNIQUES ////////////////////////
///////////////////////////////////////


// fill fb with previous state of paint texture
technique Main <
    string Script = "Pass=p0;";
> {
    pass p0 <
    	string Script = "RenderColorTarget0=;"
			"Draw=Buffer;";
    > {
	VertexShader = compile vs_3_0 VS_Quad(gBias,gGradient);
	    ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		CullMode = None;
	PixelShader  = compile ps_3_0 drawMapPS(gColorSampler);
    }
}

/////////////////////////////////////////////
///////////////////////////////////// eof ///
/////////////////////////////////////////////
