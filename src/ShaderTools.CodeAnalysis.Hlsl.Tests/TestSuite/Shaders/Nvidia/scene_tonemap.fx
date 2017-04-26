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

% Simple tone mapping shader
% with exposure and gamma controls.
% This is an HDR example, so it requires a GPU capable of supporting
% the FP16 formats used in typical HDR formats such as OpenEXR.
Supports both "straight" HDR images and RGBE 
% http://www.openexr.org/



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

#define USE_RGBE

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "scene";
    string ScriptOrder = "standard";
    string ScriptOutput = "color";
    string Script = "Technique=Main;";
> = 0.8;

texture gImage <
    string ResourceName = "nave.hdr";
    string ResourceType = "2D";
>;

sampler2D gImageSampler = sampler_state {
    Texture = <gImage>;
#if DIRECT3D_VERSION >= 0xa00
    Filter = MIN_MAG_MIP_LINEAR;
#else /* DIRECT3D_VERSION < 0xa00 */
    MinFilter = Linear;
    MipFilter = Linear;
    MagFilter = Linear;
#endif /* DIRECT3D_VERSION */
};

///////////////////////////////////

float gExposure <
    string UIWidget = "slider";
    string UIName = "Exposure";
    float UIMin = -10.0;
    float UIMax = 10.0;
    float UIStep = 0.1;
> = 0.0;

float gDefog <
    string UIWidget = "slider";
    string UIName = "De-fog";
    float UIMin = 0.0; float UIMax = 0.1; float UIStep = 0.001;
> = 0.0;


float gGamma <
    string UIWidget = "slider";
    string UIName = "Gamma";
    float UIMin = 0.0; float UIMax = 1.0; float UIStep = 0.01;
> = 1.0 / 2.2;

float3 gFogColor <
    string UIWidget = "color";
    string UIName = "Fog";
> = { 1.0, 1.0, 1.0 };

//////////////////////////////

struct a2v {
    float4 position  : POSITION;
    float4 texcoord  : TEXCOORD0;
};

struct v2f {
    float4 position  : POSITION;
    float2 UV 	: TEXCOORD0;
    float exposure   : TEXCOORD1;
};

/////////////////////////////////

v2f TonemapVS(a2v IN,
		uniform float Exposure
) {
    v2f OUT = (v2f)0;
    OUT.position = IN.position;
    OUT.UV = IN.texcoord.xy;
    OUT.exposure = pow(2.0, Exposure);
    return OUT;
}

static half3 gsDefogColor = (gDefog*gFogColor);

half4 TonemapPS(v2f IN,
		    uniform float Gamma,
		    uniform float3 DefogColor,
		    uniform sampler2D ImageSampler
) : COLOR {
    #ifdef USE_RGBE
    half4 rgbe = tex2D(ImageSampler, IN.UV);
    half expScale = pow(2.0,((rgbe.a * 255)-128.0));
    half3 c = expScale * rgbe.rgb;
#else /* ! USE_RGBE */
    half3 c = tex2D(ImageSampler, IN.UV).rgb;
#endif /* ! USE_RGBE */
    c = c - DefogColor;
    c = max(((half3)0), c);
    c *= IN.exposure;
    // gamma correction - could use texture lookups for this
    c = pow(c, Gamma);
    return half4(c.rgb, 1.0);
}

///////////////////////////////////////
/// TECHNIQUES ////////////////////////
///////////////////////////////////////


technique Main <
    string Script = "Pass=p0;";
> {
    pass p0 <
    	string Script =
	    "RenderColorTarget0=;"
	    "Draw=Buffer;";
    > {
	VertexShader = compile vs_3_0 TonemapVS(gExposure);
	    ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		CullMode = None;
	PixelShader = compile ps_3_0 TonemapPS(gGamma,
			    gsDefogColor,
			    gImageSampler);
    }
}

/////////////// eof //
