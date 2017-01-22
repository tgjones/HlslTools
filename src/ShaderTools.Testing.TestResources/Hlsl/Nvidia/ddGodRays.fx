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

% A "godrays" effect done as a single pass. Make sure the input
%  texture has alpha! It works best on a simple planar card, but
%  go ahead and experiment with all sorts of geometry. The size of
%  the glow is kept constant in screen space by adjustng the rays according
%  to the partial derivates of UV in screenspace x and y -- that is,
%    using ddx(UV) and ddy(UV)

keywords: material image_processing derivatives texture
date: 080423
$Date$

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

/******* Lighting Macros *******/
/** To use "Object-Space" lighting definitions, change these two macros: **/
#define LIGHT_COORDS "World"
// #define OBJECT_SPACE_LIGHTS /* Define if LIGHT_COORDS is "Object" */

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "object";
    string ScriptOrder = "standard";
    string ScriptOutput = "color";
    string Script = "Technique=Technique?Main:Main10;";
> = 0.8;

/**** UNTWEAKABLES: Hidden & Automatically-Tracked Parameters **********/

// transform object vertices to world-space:
float4x4 gWorldXf : World < string UIWidget="None"; >;
// transform object normals, tangents, & binormals to world-space:
float4x4 gWorldITXf : WorldInverseTranspose < string UIWidget="None"; >;
// transform object vertices to view space and project them in perspective:
float4x4 gWvpXf : WorldViewProjection < string UIWidget="None"; >;
// provide tranform from "view" or "eye" coords back to world-space:
float4x4 gViewIXf : ViewInverse < string UIWidget="None"; >;

/************* TWEAKABLES **************/

float gDD <
    string UIWidget = "slider";
    string UIName = "blur span";
    float UIMin = -12.0;
    float UIMax = 12.0;
    float UIStep = 0.01;
> = 7.0;

float gIntensity <
    string UIName = "Intensity";
    string UIWidget = "slider";
    float UIMin = 0.0f;
    float UIMax = 8.0f;
    float UIStep = 0.01f;
> = 4.0f;

float gGlowGamma <
    string UIName = "Centrality";
    string UIWidget = "slider";
    float UIMin = 0.5f;
    float UIMax = 3.0f;
    float UIStep = 0.01f;
> = 1.8f;

float gBlurStart <
    string UIName = "Blur Start";
    string UIWidget = "slider";
    float UIMin = -1.0f;
    float UIMax = 1.0f;
    float UIStep = 0.01f;
> = 1.0f;

float gBlurWidth <
    string UIName = "Blur Width";
    string UIWidget = "slider";
    float UIMin = -8.0f;
    float UIMax = 8.0f;
    float UIStep = 0.01f;
> = -6.0f;

float gRepeat <
    string UIWidget = "slider";
    string UIName = "Shrink";
    float UIMin = 0.2;
    float UIMax = 8.0;
    float UIStep = 0.1;
> = 4.0;

bool gFlipY <
	string UIName = "Flip Y?";
> = true;

/************** model texture **************/

texture gImageTex <
    string ResourceName = "FunTime.dds";
    string ResourceType = "2D";
>;

sampler2D gImageSampler = sampler_state {
    Texture = <gImageTex>;
#if DIRECT3D_VERSION >= 0xa00
    Filter = MIN_MAG_MIP_LINEAR;
#else /* DIRECT3D_VERSION < 0xa00 */
    MinFilter = Linear;
    MipFilter = Linear;
    MagFilter = Linear;
#endif /* DIRECT3D_VERSION */
    AddressU = Border;
    AddressV = Border;
};

/**************************************/
/** Connectors ************************/
/**************************************/

struct appdata {
    float3 Position	: POSITION;
    float2 UV		: TEXCOORD0;
    float4 Normal	: NORMAL0;
};

struct texposVertexOutput {
    float4 HPosition	: POSITION;
    float2 UV	: TEXCOORD0;
    float4 ScreenCoords	: TEXCOORD1; // we copy HPosition into this
};

/****************************************/
/*** SHADERS ****************************/
/****************************************/

texposVertexOutput ddTexVS(appdata IN,
    uniform float4x4 WorldITXf, // our four standard "untweakable" xforms
	uniform float4x4 WorldXf,
	uniform float4x4 ViewIXf,
	uniform float4x4 WvpXf,
    uniform float Repeat,
    uniform bool FlipY
) {
    texposVertexOutput OUT = (texposVertexOutput)0;
    float4 Po = float4(IN.Position.xyz,1.0);
    OUT.HPosition = OUT.ScreenCoords = mul(Po,WvpXf);
    const float2 uvCenter = float2(0.5,0.5);
    float2 nuv = IN.UV - uvCenter;
    if (FlipY) {
    	nuv.y = - nuv.y;
    }
    OUT.UV = float2(max(0.001,Repeat) * nuv.x,
		    max(0.001,Repeat) * nuv.y) + uvCenter;
    return OUT;
}

float4 ddTexPS(texposVertexOutput IN,
	    uniform sampler2D ColorSampler,
	    uniform float DD,		// overall effect scaling
	    uniform float BlurStart,	// range of blor values
	    uniform float BlurWidth,
	    uniform float Intensity,	// color manipulation of glow
	    uniform float GlowGamma,
	    uniform int nsamples	// how much sampling
) : COLOR
{
    // location on screen relative to image center
    float2 sUV = float2(IN.ScreenCoords.x/IN.ScreenCoords.w,
			IN.ScreenCoords.y/IN.ScreenCoords.w);
    // we just want this as a 2D direction, so normalize it
    sUV = normalize(sUV); 
    // duv will be our screen-radial 2D UV vector -- that is, our step size in UV based
    //    on the local partial derivatives of UV in screen-x and screen-y.
    //    Note that ddx()/ddy() return "float2" here
    float2 duv = (sUV.x * ddx(IN.UV)) - (sUV.y * ddy(IN.UV));
    duv *= DD;
    // now we can use this step to accumulate our color samples
    float4 blurred = 0;
    for(int i=0; i<nsamples; i++) {
    	float scale = BlurStart + BlurWidth*(i/(float) (nsamples-1));
    	blurred += tex2D(ColorSampler, IN.UV + scale * duv);
    }
    blurred /= nsamples;
    // tweak the color a bit
    blurred.rgb = pow(blurred.rgb,GlowGamma);
    blurred.rgb *= Intensity;
    blurred.rgb = saturate(blurred.rgb);
    // now composite original pic back on top of the blur
    float4 origTex = tex2D(ColorSampler, IN.UV);
    float3 newC = origTex.rgb + (1.0-origTex.a)*blurred.rgb;
    float newA = max(origTex.a,blurred.a);
    return float4(newC.rgb,newA);
}

///////////////////////////////////////
/// TECHNIQUES ////////////////////////
///////////////////////////////////////

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

technique10 Main10 <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        SetVertexShader( CompileShader( vs_4_0, ddTexVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,
		    gRepeat, gFlipY) ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, ddTexPS(gImageSampler,gDD,
				gBlurStart,gBlurWidth,
				gIntensity,gGlowGamma,32) ) );
	    SetRasterizerState(DisableCulling);
	    SetDepthStencilState(DepthEnabling, 0);
	    SetBlendState(DisableBlend, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF);
    }
}

#endif /* DIRECT3D_VERSION >= 0xa00 */

technique Main <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
    > {
        VertexShader = compile vs_3_0 ddTexVS(gWorldITXf,gWorldXf,
				gViewIXf,gWvpXf,
		    gRepeat, gFlipY);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		AlphaBlendEnable = false;
		CullMode = None;
        PixelShader = compile ps_3_0 ddTexPS(gImageSampler,gDD,
				gBlurStart,gBlurWidth,
				gIntensity,gGlowGamma,32);
    }
}

/***************************** eof ***/
